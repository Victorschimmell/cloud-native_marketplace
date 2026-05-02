using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Infrastructure.Auth;
using Backend.Infrastructure.Common;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Persistence.Interceptors;
using Backend.Infrastructure.Persistence.Import.Abstractions;
using Backend.Infrastructure.Persistence.Import.Services;
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
