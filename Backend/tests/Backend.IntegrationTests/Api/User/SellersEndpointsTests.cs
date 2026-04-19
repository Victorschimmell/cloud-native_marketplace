using System.Net;
using Backend.Api;

namespace Backend.IntegrationTests;

public class SellersEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;

    public SellersEndpointsTests(MarketplaceApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetSellers_ReturnsNotImplemented()
    {
        // Act
        var response = await _client.GetAsync("/api/sellers");

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task GetSellerById_ReturnsNotImplemented()
    {
        // Act
        var sellerId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/sellers/{sellerId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }
}
