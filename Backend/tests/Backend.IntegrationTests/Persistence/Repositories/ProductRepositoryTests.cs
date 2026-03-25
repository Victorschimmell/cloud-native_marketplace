using Backend.Application.Abstractions.Repositories;
using Backend.IntegrationTests.TestSupport;
using Backend.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.IntegrationTests;

public sealed class ProductRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsStableAlphabeticalPages()
    {
        await using var host = await SqliteRepositoryTestHost.CreateAsync();
        using var scope = host.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IProductRepository>();

        var category = TestEntityFactory.CreateCategory("books", "Books");

        dbContext.ProductCategories.Add(category);
        dbContext.Products.AddRange(
            TestEntityFactory.CreateProduct(category.Id, "Delta"),
            TestEntityFactory.CreateProduct(category.Id, "Alpha"),
            TestEntityFactory.CreateProduct(category.Id, "Charlie"),
            TestEntityFactory.CreateProduct(category.Id, "Bravo"));
        await dbContext.SaveChangesAsync();

        var firstPage = await repository.GetAllAsync(page: 1, pageSize: 2);
        var secondPage = await repository.GetAllAsync(page: 2, pageSize: 2);

        Assert.Equal(["Alpha", "Bravo"], firstPage.Select(product => product.ProductName).ToArray());
        Assert.Equal(["Charlie", "Delta"], secondPage.Select(product => product.ProductName).ToArray());
    }

    [Fact]
    public async Task GetByCategoryIdAsync_ReturnsOnlyProductsInRequestedCategory()
    {
        await using var host = await SqliteRepositoryTestHost.CreateAsync();
        using var scope = host.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IProductRepository>();

        var furniture = TestEntityFactory.CreateCategory("moveis", "Furniture");
        var electronics = TestEntityFactory.CreateCategory("eletronicos", "Electronics");

        dbContext.ProductCategories.AddRange(furniture, electronics);
        dbContext.Products.AddRange(
            TestEntityFactory.CreateProduct(furniture.Id, "Chair"),
            TestEntityFactory.CreateProduct(furniture.Id, "Desk"),
            TestEntityFactory.CreateProduct(electronics.Id, "Keyboard"));
        await dbContext.SaveChangesAsync();

        var products = await repository.GetByCategoryIdAsync(furniture.Id, page: 1, pageSize: 10);

        Assert.Equal(["Chair", "Desk"], products.Select(product => product.ProductName).ToArray());
    }
}
