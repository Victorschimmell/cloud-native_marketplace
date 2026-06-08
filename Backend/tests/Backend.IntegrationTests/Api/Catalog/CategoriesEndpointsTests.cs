using System.Net;
using System.Net.Http.Json;
using Backend.Api.Contracts.Catalog.Categories;

namespace Backend.IntegrationTests;

[Collection("PostgresDocker")]
public class CategoriesEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;

    public CategoriesEndpointsTests(MarketplaceApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCategories_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/categories", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var categories = await response.Content.ReadFromJsonAsync<IReadOnlyList<CategoryResponse>>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);
        Assert.NotNull(categories);
    }
}
