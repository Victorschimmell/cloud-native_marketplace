using Backend.Application.Abstractions.Repositories;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.IntegrationTests;

public sealed class RepositoryDITests
{
    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

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

        return services.BuildServiceProvider();
    }

    [Fact]
    public void ApplicationDbContext_CanBeResolved()
    {
        using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Assert.NotNull(dbContext);
    }

    [Fact]
    public void ApplicationDbContext_CanBeInstantiated_WithInMemoryProvider()
    {
        using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Assert.NotNull(dbContext.Model);
    }

    [Fact]
    public void IUserAccountRepository_ResolvesFromServiceProvider()
    {
        using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<IUserAccountRepository>();

        Assert.NotNull(repository);
    }

    [Fact]
    public void ICustomerRepository_ResolvesFromServiceProvider()
    {
        using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();

        Assert.NotNull(repository);
    }

    [Fact]
    public void ISellerRepository_ResolvesFromServiceProvider()
    {
        using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<ISellerRepository>();

        Assert.NotNull(repository);
    }

    [Fact]
    public void IProductCategoryRepository_ResolvesFromServiceProvider()
    {
        using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<IProductCategoryRepository>();

        Assert.NotNull(repository);
    }

    [Fact]
    public void IProductRepository_ResolvesFromServiceProvider()
    {
        using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<IProductRepository>();

        Assert.NotNull(repository);
    }

    [Fact]
    public void IProductListingRepository_ResolvesFromServiceProvider()
    {
        using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<IProductListingRepository>();

        Assert.NotNull(repository);
    }

    [Fact]
    public void IOrderRepository_ResolvesFromServiceProvider()
    {
        using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

        Assert.NotNull(repository);
    }

    [Fact]
    public void IOrderItemRepository_ResolvesFromServiceProvider()
    {
        using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<IOrderItemRepository>();

        Assert.NotNull(repository);
    }

    [Fact]
    public void IOrderReviewRepository_ResolvesFromServiceProvider()
    {
        using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<IOrderReviewRepository>();

        Assert.NotNull(repository);
    }

    [Fact]
    public void ICartRepository_ResolvesFromServiceProvider()
    {
        using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<ICartRepository>();

        Assert.NotNull(repository);
    }

    [Fact]
    public void IAuditLogRepository_ResolvesFromServiceProvider()
    {
        using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<IAuditLogRepository>();

        Assert.NotNull(repository);
    }

    [Fact]
    public void ISellerVerificationRequestRepository_ResolvesFromServiceProvider()
    {
        using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<ISellerVerificationRequestRepository>();

        Assert.NotNull(repository);
    }

    [Fact]
    public void IShipmentRepository_ResolvesFromServiceProvider()
    {
        using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<IShipmentRepository>();

        Assert.NotNull(repository);
    }

    [Fact]
    public void ICurrencyRepository_ResolvesFromServiceProvider()
    {
        using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<ICurrencyRepository>();

        Assert.NotNull(repository);
    }

    [Fact]
    public async Task ProductCategoryRepository_SmokeTest_AddAndGetById()
    {
        using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var repository = scope.ServiceProvider.GetRequiredService<IProductCategoryRepository>();

        var category = new Domain.Entities.Catalog.ProductCategory
        {
            CategoryNamePt = "Eletrônicos",
            CategoryNameEn = "Electronics"
        };

        await repository.AddAsync(category);

        var retrieved = await repository.GetByIdAsync(category.Id);

        Assert.NotNull(retrieved);
        Assert.Equal("Eletrônicos", retrieved.CategoryNamePt);
        Assert.Equal("Electronics", retrieved.CategoryNameEn);
    }
}
