using Backend.Application.Abstractions.Repositories;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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

    public static async Task<SqliteRepositoryTestHost> CreateAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var services = new ServiceCollection();

        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection));

        services.AddScoped<IUserAccountRepository, UserAccountRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ISellerRepository, SellerRepository>();
        services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductListingRepository, ProductListingRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderItemRepository, OrderItemRepository>();
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
