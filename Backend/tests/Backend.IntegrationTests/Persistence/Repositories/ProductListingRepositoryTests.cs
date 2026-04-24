using Backend.Application.Abstractions.Repositories;
using Backend.IntegrationTests.TestSupport;
using Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.IntegrationTests;

public sealed class ProductListingRepositoryTests
{
    [Fact]
    public async Task DeleteAsync_SoftDeletesListing_AndRemovesItFromQueryResults()
    {
        await using var host = await SqliteRepositoryTestHost.CreateAsync();
        using var scope = host.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IProductListingRepository>();

        var sellerUser = TestEntityFactory.CreateUserAccount("seller@example.com");
        var seller = TestEntityFactory.CreateSeller(sellerUser.Id);
        var category = TestEntityFactory.CreateCategory("livros", "Books");
        var product = TestEntityFactory.CreateProduct(category.Id, "Domain-Driven Design");
        var listing = TestEntityFactory.CreateListing(seller.Id, product.Id, "BOOK-001", 199.95m);

        dbContext.UserAccounts.Add(sellerUser);
        dbContext.Sellers.Add(seller);
        dbContext.ProductCategories.Add(category);
        dbContext.Products.Add(product);
        dbContext.ProductListings.Add(listing);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        await repository.DeleteAsync(listing, TestContext.Current.CancellationToken);

        var storedListing = await dbContext.ProductListings.SingleAsync(entity => entity.Id == listing.Id, TestContext.Current.CancellationToken);
        var productListings = await repository.GetByProductIdAsync(product.Id, TestContext.Current.CancellationToken);

        Assert.True(storedListing.IsDeleted);
        Assert.Empty(productListings);
    }

    [Fact]
    public async Task GetByProductIdAsync_ReturnsOnlyActiveListingsForThatProduct_OrderedByPrice()
    {
        await using var host = await SqliteRepositoryTestHost.CreateAsync();
        using var scope = host.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IProductListingRepository>();

        var sellerUser = TestEntityFactory.CreateUserAccount("seller2@example.com");
        var otherSellerUser = TestEntityFactory.CreateUserAccount("seller3@example.com");
        var seller = TestEntityFactory.CreateSeller(sellerUser.Id);
        var otherSeller = TestEntityFactory.CreateSeller(otherSellerUser.Id);
        var category = TestEntityFactory.CreateCategory("games", "Games");
        var targetProduct = TestEntityFactory.CreateProduct(category.Id, "Console");
        var otherProduct = TestEntityFactory.CreateProduct(category.Id, "Headset");

        var lowPrice = TestEntityFactory.CreateListing(seller.Id, targetProduct.Id, "CONSOLE-LOW", 399m);
        var highPrice = TestEntityFactory.CreateListing(otherSeller.Id, targetProduct.Id, "CONSOLE-HIGH", 499m);
        var deleted = TestEntityFactory.CreateListing(seller.Id, targetProduct.Id, "CONSOLE-DELETED", 299m);
        var unrelated = TestEntityFactory.CreateListing(otherSeller.Id, otherProduct.Id, "HEADSET-001", 99m);
        deleted.IsDeleted = true;

        dbContext.UserAccounts.AddRange(sellerUser, otherSellerUser);
        dbContext.Sellers.AddRange(seller, otherSeller);
        dbContext.ProductCategories.Add(category);
        dbContext.Products.AddRange(targetProduct, otherProduct);
        dbContext.ProductListings.AddRange(lowPrice, highPrice, deleted, unrelated);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var listings = await repository.GetByProductIdAsync(targetProduct.Id, TestContext.Current.CancellationToken);

        Assert.Equal(["CONSOLE-LOW", "CONSOLE-HIGH"], listings.Select(listing => listing.Sku).ToArray());
    }

    [Fact]
    public async Task AddAsync_RejectsDuplicateSku()
    {
        await using var host = await SqliteRepositoryTestHost.CreateAsync();
        using var scope = host.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IProductListingRepository>();

        var sellerUser = TestEntityFactory.CreateUserAccount("sku-seller@example.com");
        var seller = TestEntityFactory.CreateSeller(sellerUser.Id);
        var category = TestEntityFactory.CreateCategory("music", "Music");
        var product = TestEntityFactory.CreateProduct(category.Id, "Turntable");

        dbContext.UserAccounts.Add(sellerUser);
        dbContext.Sellers.Add(seller);
        dbContext.ProductCategories.Add(category);
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        await repository.AddAsync(TestEntityFactory.CreateListing(seller.Id, product.Id, "SKU-001", 999m), TestContext.Current.CancellationToken);

        var duplicate = TestEntityFactory.CreateListing(seller.Id, product.Id, "SKU-001", 899m);

        await Assert.ThrowsAsync<DbUpdateException>(() => repository.AddAsync(duplicate, TestContext.Current.CancellationToken));
    }
}
