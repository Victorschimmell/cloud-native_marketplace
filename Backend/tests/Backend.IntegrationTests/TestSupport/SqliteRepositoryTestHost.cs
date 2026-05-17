using System.Globalization;
using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Infrastructure.Common;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Persistence.Interceptors;
using Backend.Infrastructure.Persistence.Import.Abstractions;
using Backend.Infrastructure.Persistence.Import.Services;
using Backend.Infrastructure.Persistence.Repositories;
using Backend.Infrastructure.Persistence.Seeding;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Backend.IntegrationTests.TestSupport;

internal sealed class SqliteRepositoryTestHost : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _serviceProvider;

    private SqliteRepositoryTestHost(SqliteConnection connection, ServiceProvider serviceProvider)
    {
        _connection = connection;
        _serviceProvider = serviceProvider;
    }

    public static async Task<SqliteRepositoryTestHost> CreateAsync(Action<OlistSeedOptions>? configureOlistOptions = null)
    {
        // EF Core SQLite registers a decimal collation that calls decimal.Parse on raw SQLite
        // text values. When the OS culture uses ',' as the decimal separator the parse fails
        // with FormatException. Pinning to InvariantCulture before opening the connection
        // ensures the collation always uses '.' as the decimal point.
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var services = new ServiceCollection();

        services.AddLogging();
        services.AddScoped<IDateTimeProvider, InfrastructureDateTimeProvider>();
        services.AddScoped<AuditTimestampInterceptor>();
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            options.UseSqlite(connection)
                .AddInterceptors(serviceProvider.GetRequiredService<AuditTimestampInterceptor>()));
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<ICsvDatasetReader, CsvDatasetReader>();
        services.AddScoped<IOlistDataSeeder, OlistDataSeeder>();

        var olistOptions = new OlistSeedOptions();
        configureOlistOptions?.Invoke(olistOptions);
        services.AddSingleton(Options.Create(olistOptions));

        services.AddScoped<IUserAccountRepository, UserAccountRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ISellerRepository, SellerRepository>();
        services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductListingRepository, ProductListingRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderItemRepository, OrderItemRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IOrderReviewRepository, OrderReviewRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<ISellerVerificationRequestRepository, SellerVerificationRequestRepository>();
        services.AddScoped<IShipmentRepository, ShipmentRepository>();
        services.AddScoped<ICurrencyRepository, CurrencyRepository>();

        var serviceProvider = services.BuildServiceProvider();

        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        return new SqliteRepositoryTestHost(connection, serviceProvider);
    }

    public IServiceScope CreateScope() => _serviceProvider.CreateScope();

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
        await _serviceProvider.DisposeAsync();
    }
}
