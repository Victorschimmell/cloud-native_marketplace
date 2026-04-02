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

        await seeder.SeedAsync();
        await seeder.SeedAsync();

        var dbContext = firstScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Assert.Equal(1, await dbContext.Currencies.CountAsync());
        Assert.Equal(3, await dbContext.ProductCategories.CountAsync());
        Assert.Equal(2, await dbContext.Customers.CountAsync());
        Assert.Equal(2, await dbContext.Sellers.CountAsync());
        Assert.Equal(3, await dbContext.Products.CountAsync());
        Assert.Equal(4, await dbContext.ProductListings.CountAsync());
        Assert.Equal(3, await dbContext.Orders.CountAsync());
        Assert.Equal(5, await dbContext.OrderItems.CountAsync());
        Assert.Equal(4, await dbContext.OrderPayments.CountAsync());
        Assert.Equal(4, await dbContext.OrderReviews.CountAsync());

        var importedOrder = await dbContext.Orders.SingleAsync(order => order.OrderNumber == "order-1");
        Assert.Equal(1149m, importedOrder.SubtotalAmount);
        Assert.Equal(16m, importedOrder.FreightAmount);
        Assert.Equal(1165m, importedOrder.TotalAmount);

        var importedProduct = await dbContext.Products.SingleAsync(product => product.OlistProductId == "product-1");
        Assert.Contains("bed_bath_table", importedProduct.Description);

        var uncategorizedProduct = await dbContext.Products
            .Include(product => product.Category)
            .SingleAsync(product => product.OlistProductId == "product-3");
        Assert.Equal("olist_sem_categoria", uncategorizedProduct.Category!.CategoryNamePt);

        var listing = await dbContext.ProductListings
            .SingleAsync(productListing => productListing.Sku == "OLIST-SELLER-1-PRODUCT-2");
        Assert.Equal(60m, listing.ListingPrice);

        var importedReviews = await dbContext.OrderReviews
            .Where(review => review.Order!.OrderNumber == "order-3")
            .OrderBy(review => review.OlistReviewId)
            .ToListAsync();

        Assert.Equal(2, importedReviews.Count);
        Assert.Equal("review-3", importedReviews[0].OlistReviewId);
        Assert.Equal("review-4", importedReviews[1].OlistReviewId);
    }
}
