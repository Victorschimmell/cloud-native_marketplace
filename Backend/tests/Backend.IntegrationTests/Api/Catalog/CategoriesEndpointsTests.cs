using System.Net;
using System.Net.Http.Json;
using Backend.Api;
using Backend.Api.Contracts.Catalog.Categories;

namespace Backend.IntegrationTests;

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

        var categories = await response.Content.ReadFromJsonAsync<IReadOnlyList<CategoryResponse>>(TestContext.Current.CancellationToken);
        Assert.NotNull(categories);
    }

    [Fact]
    public async Task GetCategoryById_ReturnsNotImplemented()
    {
        // Act
        var categoryId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/categories/{categoryId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_ReturnsNotImplemented()
    {
        // Arrange
        var createRequest = new CreateCategoryRequest
        {
            CategoryNamePt = "Test Category",
            CategoryNameEn = "Test Category"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/categories", createRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCategory_ReturnsNotImplemented()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var updateRequest = new UpdateCategoryRequest
        {
            CategoryNamePt = "Updated Category",
            CategoryNameEn = "Updated Category"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/categories/{categoryId}", updateRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_ReturnsNotImplemented()
    {
        // Act
        var categoryId = Guid.NewGuid();
        var response = await _client.DeleteAsync($"/api/categories/{categoryId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }
}
