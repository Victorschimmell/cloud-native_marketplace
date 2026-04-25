using System.Net;
using Backend.Api;

namespace Backend.IntegrationTests;

public class CustomersEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;

    public CustomersEndpointsTests(MarketplaceApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCustomers_ReturnsNotImplemented()
    {
        // Act
        var response = await _client.GetAsync("/api/customers", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomerById_ReturnsNotImplemented()
    {
        // Act
        var customerId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/customers/{customerId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task GetCartByCustomerId_ReturnsNotImplemented()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/customers/{customerId}/cart", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task GetOrdersByCustomerId_ReturnsNotImplemented()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/customers/{customerId}/orders", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }
}
