using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Persistence.Seeding;
using Backend.IntegrationTests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.IntegrationTests.Persistence.Seeding;

public sealed class OlistDataSeederTests
{
    [Fact]
    public async Task SeedAsync_ImportsFixtureDatasetAndIsIdempotent()
    {
        var datasetPath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "Olist");

        await using var host = await SqliteRepositoryTestHost.CreateAsync(options =>
        {
            options.Enabled = true;
            options.DatasetRootPath = datasetPath;
        });

        using var firstScope = host.CreateScope();
        var seeder = firstScope.ServiceProvider.GetRequiredService<IOlistDataSeeder>();

        await seeder.SeedAsync(TestContext.Current.CancellationToken);
        var dbContext = firstScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var listingBeforeReseed = await dbContext.ProductListings
            .SingleAsync(productListing => productListing.Sku == "OLIST-SELLER-1-PRODUCT-2", TestContext.Current.CancellationToken);
        Assert.InRange(listingBeforeReseed.InventoryQuantity, 0, 10);

        listingBeforeReseed.InventoryQuantity = 0;
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        await seeder.SeedAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, await dbContext.Currencies.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(3, await dbContext.ProductCategories.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(2, await dbContext.Customers.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(2, await dbContext.Sellers.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(3, await dbContext.Products.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(4, await dbContext.ProductListings.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(3, await dbContext.Orders.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(5, await dbContext.OrderItems.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(4, await dbContext.OrderPayments.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(2, await dbContext.OrderReviews.CountAsync(TestContext.Current.CancellationToken));

        var importedOrder = await dbContext.Orders.SingleAsync(order => order.OrderNumber == "order-1", TestContext.Current.CancellationToken);
        Assert.Equal(1149m, importedOrder.SubtotalAmount);
        Assert.Equal(16m, importedOrder.FreightAmount);
        Assert.Equal(1165m, importedOrder.TotalAmount);

        var importedProduct = await dbContext.Products.SingleAsync(product => product.OlistProductId == "product-1", TestContext.Current.CancellationToken);
        Assert.Contains("bed_bath_table", importedProduct.Description);

        var uncategorizedProduct = await dbContext.Products
            .Include(product => product.Category)
            .SingleAsync(product => product.OlistProductId == "product-3", TestContext.Current.CancellationToken);
        Assert.Equal("olist_sem_categoria", uncategorizedProduct.Category!.CategoryNamePt);

        var listing = await dbContext.ProductListings
            .SingleAsync(productListing => productListing.Sku == "OLIST-SELLER-1-PRODUCT-2", TestContext.Current.CancellationToken);
        Assert.Equal(60m, listing.ListingPrice);
        Assert.Equal(0, listing.InventoryQuantity);

        var importedReviews = await dbContext.OrderReviews
            .Where(review => review.Order!.OrderNumber == "order-3")
            .OrderBy(review => review.OlistReviewId)
            .ToListAsync(TestContext.Current.CancellationToken);

        var importedReview = Assert.Single(importedReviews);
        Assert.Equal("review-3", importedReviews[0].OlistReviewId);
        Assert.True(importedReview.OrderItemId > 0);
        Assert.NotEqual(Guid.Empty, importedReview.CustomerId);
        Assert.NotEqual(Guid.Empty, importedReview.ProductId);

        Assert.False(await dbContext.OrderReviews.AnyAsync(
            review => review.Order!.OrderNumber == "order-1",
            TestContext.Current.CancellationToken));
    }
}
