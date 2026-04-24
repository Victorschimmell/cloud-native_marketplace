using System.Net;
using System.Net.Http.Json;
using Backend.Api;
using Backend.Api.Contracts.Catalog.Products;

namespace Backend.IntegrationTests;

public class ProductsEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;

    public ProductsEndpointsTests(MarketplaceApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_ReturnsNotImplemented()
    {
        // Act
        var response = await _client.GetAsync("/api/products", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task GetProductById_ReturnsNotImplemented()
    {
        // Act
        var productId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/products/{productId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_ReturnsNotImplemented()
    {
        // Arrange
        var createRequest = new CreateProductRequest
        {
            ProductName = "Test Product",
            CategoryId = Guid.NewGuid(),
            Description = "Test Description",
            ProductPhotosQty = 1,
            ProductWeightG = 100,
            ProductLengthCm = 10,
            ProductHeightCm = 10,
            ProductWidthCm = 10
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", createRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProduct_ReturnsNotImplemented()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var updateRequest = new UpdateProductRequest
        {
            ProductName = "Updated Product",
            CategoryId = Guid.NewGuid(),
            Description = "Updated Description",
            ProductPhotosQty = 1,
            ProductWeightG = 100,
            ProductLengthCm = 10,
            ProductHeightCm = 10,
            ProductWidthCm = 10
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/products/{productId}", updateRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_ReturnsNotImplemented()
    {
        // Act
        var productId = Guid.NewGuid();
        var response = await _client.DeleteAsync($"/api/products/{productId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task GetProductReviews_ReturnsNotImplemented()
    {
        // Act
        var productId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/products/{productId}/reviews", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }
}
