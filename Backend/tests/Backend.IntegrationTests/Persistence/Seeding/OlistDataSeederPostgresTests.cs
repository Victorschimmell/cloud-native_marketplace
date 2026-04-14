using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Persistence.Seeding;
using Backend.IntegrationTests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.IntegrationTests.Persistence.Seeding;

[Collection("PostgresDocker")]
public sealed class OlistDataSeederPostgresTests
{
    [Fact]
    public async Task SeedAsync_ImportsFixtureDatasetIntoMigratedPostgresDatabase()
    {
        var datasetPath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "Olist");

        await using var host = await PostgresSeederTestHost.CreateAsync(options =>
        {
            options.Enabled = true;
            options.DatasetRootPath = datasetPath;
        });

        using (var seedScope = host.CreateScope())
            await seedScope.ServiceProvider.GetRequiredService<IOlistDataSeeder>().SeedAsync();

        using (var seedScope = host.CreateScope())
            await seedScope.ServiceProvider.GetRequiredService<IOlistDataSeeder>().SeedAsync();

        using var verifyScope = host.CreateScope();
        var dbContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Assert.Contains(
            "20260328195500_AddOlistReviewIdToOrderReviews",
            await dbContext.Database.GetAppliedMigrationsAsync());

        Assert.Equal(3, await dbContext.ProductCategories.CountAsync());
        Assert.Equal(3, await dbContext.Products.CountAsync());
        Assert.Equal(5, await dbContext.OrderItems.CountAsync());
        Assert.Equal(4, await dbContext.OrderReviews.CountAsync());

        var importedReviews = await dbContext.OrderReviews
            .Where(review => review.Order!.OrderNumber == "order-3")
            .OrderBy(review => review.OlistReviewId)
            .Select(review => review.OlistReviewId)
            .ToListAsync();

        Assert.Equal(new[] { "review-3", "review-4" }, importedReviews);
    }
}
