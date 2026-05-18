using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Domain.Entities.IdentityAccess;
using Backend.Domain.Enums;
using Backend.Domain.ValueObjects;
using Backend.Infrastructure.Auth;
using Backend.Infrastructure.Common;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Persistence.Import.Abstractions;
using Backend.Infrastructure.Persistence.Import.Services;
using Backend.Infrastructure.Persistence.Interceptors;
using Backend.Infrastructure.Persistence.Repositories;
using Backend.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Backend.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        services.AddScoped<AuditTimestampInterceptor>();
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            options.UseNpgsql(connectionString)
                .AddInterceptors(serviceProvider.GetRequiredService<AuditTimestampInterceptor>()));
        services.AddDbContextFactory<ApplicationDbContext>((serviceProvider, options) =>
            options.UseNpgsql(connectionString), ServiceLifetime.Scoped);
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        var olistOptions = BuildOlistSeedOptions(configuration);
        services.AddSingleton(Options.Create(olistOptions));
        services.AddScoped<ICsvDatasetReader, CsvDatasetReader>();
        services.AddScoped<IOlistDataSeeder, OlistDataSeeder>();
        services.AddScoped<IDateTimeProvider, InfrastructureDateTimeProvider>();
        services.AddScoped<IPasswordHasher, InfrastructurePasswordHasher>();
        services.AddScoped<IAuthTokenGenerator, InfrastructureAuthTokenGenerator>();

        services.AddScoped<IUserAccountRepository, UserAccountRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ISellerRepository, SellerRepository>();
        services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductListingRepository, ProductListingRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderItemRepository, OrderItemRepository>();
        services.AddScoped<IOrderNumberGenerator, OrderNumberRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IOrderReviewRepository, OrderReviewRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAdminIssueRepository, AdminIssueRepository>();
        services.AddScoped<ISellerVerificationRequestRepository, SellerVerificationRequestRepository>();
        services.AddScoped<IShipmentRepository, ShipmentRepository>();
        services.AddScoped<ICurrencyRepository, CurrencyRepository>();

        return services;
    }

    public static async Task ApplyMigrationsAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    public static async Task SeedOlistDataAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IOlistDataSeeder>();
        await seeder.SeedAsync(cancellationToken);
    }

    public static async Task SeedAdminDataAsync(this IServiceProvider serviceProvider, IConfiguration configuration, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var email = configuration.GetSection("AdminUser")["Email"]?.Trim().ToLowerInvariant() ?? "";
        var password = configuration.GetSection("AdminUser")["Password"] ?? "";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var adminEmail = new EmailAddress(email);
        var adminPasswordHash = passwordHasher.HashPassword(password.Trim());

        // Check if an admin user already exists
        if (await dbContext.UserAccounts.AnyAsync(u => u.Email == adminEmail, cancellationToken))
        {
            return;
        }

        var adminUser = new UserAccount
        {
            Email = adminEmail,
            PasswordHash = adminPasswordHash,
            AccountStatus = AccountStatus.Active,
            IsAdmin = true
        };
        await dbContext.UserAccounts.AddAsync(adminUser, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    // Mirrors SeedAdminDataAsync: reads SellerUser:{Email,Password,...} from
    // configuration, creates the UserAccount + Seller row on first run
    // This is neccesary to debug seller dashboard as a registered seller - can be removed at a later phase.
    public static async Task SeedSellerDataAsync(this IServiceProvider serviceProvider, IConfiguration configuration, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var email = configuration.GetSection("SellerUser")["Email"]?.Trim().ToLowerInvariant() ?? "";
        var password = configuration.GetSection("SellerUser")["Password"] ?? "";
        var businessName = configuration.GetSection("SellerUser")["BusinessName"]?.Trim() ?? "Demo Shop";
        var registrationNumber = configuration.GetSection("SellerUser")["RegistrationNumber"]?.Trim() ?? "DK-DEMO-001";
        var payoutInformation = configuration.GetSection("SellerUser")["PayoutInformation"]?.Trim() ?? "IBAN DEMO";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var sellerEmail = new EmailAddress(email);

        // No-op if either the user already exists or already has a Seller row attached.
        var existingUser = await dbContext.UserAccounts
            .FirstOrDefaultAsync(u => u.Email == sellerEmail, cancellationToken);

        if (existingUser is not null)
        {
            var hasSeller = await dbContext.Sellers.AnyAsync(s => s.UserId == existingUser.Id, cancellationToken);
            if (hasSeller)
            {
                return;
            }

            // User exists but no Seller row (legacy / half-created state): repair it.
            var sellerForExisting = new Seller
            {
                UserId = existingUser.Id,
                BusinessName = businessName,
                RegistrationNumber = registrationNumber,
                PayoutInformation = payoutInformation,
                VerificationStatus = VerificationStatus.Verified,
                VerifiedAtUtc = DateTimeOffset.UtcNow
            };
            await dbContext.Sellers.AddAsync(sellerForExisting, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        var sellerUser = new UserAccount
        {
            Email = sellerEmail,
            PasswordHash = passwordHasher.HashPassword(password.Trim()),
            AccountStatus = AccountStatus.Active,
            IsAdmin = false
        };
        await dbContext.UserAccounts.AddAsync(sellerUser, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var seller = new Seller
        {
            UserId = sellerUser.Id,
            BusinessName = businessName,
            RegistrationNumber = registrationNumber,
            PayoutInformation = payoutInformation,
            VerificationStatus = VerificationStatus.Verified,
            VerifiedAtUtc = DateTimeOffset.UtcNow
        };
        await dbContext.Sellers.AddAsync(seller, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static OlistSeedOptions BuildOlistSeedOptions(IConfiguration configuration)
    {
        var section = configuration.GetSection(OlistSeedOptions.SectionName);

        return new OlistSeedOptions
        {
            Enabled = bool.TryParse(section["Enabled"], out var enabled) && enabled,
            DatasetRootPath = section["DatasetRootPath"],
            ProductCategoryTranslationFileName = section["ProductCategoryTranslationFileName"] ?? "product_category_name_translation.csv",
            CustomersFileName = section["CustomersFileName"] ?? "olist_customers_dataset.csv",
            SellersFileName = section["SellersFileName"] ?? "olist_sellers_dataset.csv",
            ProductsFileName = section["ProductsFileName"] ?? "olist_products_dataset.csv",
            OrdersFileName = section["OrdersFileName"] ?? "olist_orders_dataset.csv",
            OrderItemsFileName = section["OrderItemsFileName"] ?? "olist_order_items_dataset.csv",
            OrderPaymentsFileName = section["OrderPaymentsFileName"] ?? "olist_order_payments_dataset.csv",
            OrderReviewsFileName = section["OrderReviewsFileName"] ?? "olist_order_reviews_dataset.csv"
        };
    }
}
