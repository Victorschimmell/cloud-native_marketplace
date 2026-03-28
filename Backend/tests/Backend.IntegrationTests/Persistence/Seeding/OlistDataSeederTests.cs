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
        Assert.Equal(2, await dbContext.ProductCategories.CountAsync());
        Assert.Equal(2, await dbContext.Customers.CountAsync());
        Assert.Equal(2, await dbContext.Sellers.CountAsync());
        Assert.Equal(2, await dbContext.Products.CountAsync());
        Assert.Equal(3, await dbContext.ProductListings.CountAsync());
        Assert.Equal(2, await dbContext.Orders.CountAsync());
        Assert.Equal(3, await dbContext.OrderItems.CountAsync());
        Assert.Equal(3, await dbContext.OrderPayments.CountAsync());
        Assert.Equal(2, await dbContext.OrderReviews.CountAsync());

        var importedOrder = await dbContext.Orders.SingleAsync(order => order.OrderNumber == "order-1");
        Assert.Equal(150m, importedOrder.SubtotalAmount);
        Assert.Equal(15m, importedOrder.FreightAmount);
        Assert.Equal(165m, importedOrder.TotalAmount);

        var importedProduct = await dbContext.Products.SingleAsync(product => product.OlistProductId == "product-1");
        Assert.Contains("bed_bath_table", importedProduct.Description);

        var importedReview = await dbContext.OrderReviews.SingleAsync(review => review.OrderId == importedOrder.Id);
        Assert.Contains("Great, works", importedReview.ReviewCommentMessage);
    }
}
