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

    public static TheoryData<Type> RepositoryTypes =>
    [
        typeof(IUserAccountRepository),
        typeof(ICustomerRepository),
        typeof(ISellerRepository),
        typeof(IProductCategoryRepository),
        typeof(IProductRepository),
        typeof(IProductListingRepository),
        typeof(IOrderRepository),
        typeof(IOrderItemRepository),
        typeof(IOrderReviewRepository),
        typeof(ICartRepository),
        typeof(IAuditLogRepository),
        typeof(ISellerVerificationRequestRepository),
        typeof(IShipmentRepository),
        typeof(ICurrencyRepository),
    ];

    [Theory]
    [MemberData(nameof(RepositoryTypes))]
    public void Repository_ResolvesFromServiceProvider(Type repositoryType)
    {
        using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService(repositoryType);

        Assert.NotNull(repository);
    }

    [Fact]
    public async Task ProductCategoryRepository_SmokeTest_AddAndGetById()
    {
        using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        var repository = scope.ServiceProvider.GetRequiredService<IProductCategoryRepository>();

        var category = new Domain.Entities.Catalog.ProductCategory
        {
            CategoryNamePt = "Eletronicos",
            CategoryNameEn = "Electronics"
        };

        await repository.AddAsync(category, TestContext.Current.CancellationToken);

        var retrieved = await repository.GetByIdAsync(category.Id, TestContext.Current.CancellationToken);

        Assert.NotNull(retrieved);
        Assert.Equal("Eletronicos", retrieved.CategoryNamePt);
        Assert.Equal("Electronics", retrieved.CategoryNameEn);
    }
}
