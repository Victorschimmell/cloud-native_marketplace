using System.Net;
using Backend.Api;

namespace Backend.IntegrationTests;

public class AnalyticsEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;

    public AnalyticsEndpointsTests(MarketplaceApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetSalesStatistics_ReturnsNotImplemented()
    {
        // Act
        var response = await _client.GetAsync("/api/analytics/sales");

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task GetOrdersStatistics_ReturnsNotImplemented()
    {
        // Act
        var response = await _client.GetAsync("/api/analytics/orders");

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }
}
