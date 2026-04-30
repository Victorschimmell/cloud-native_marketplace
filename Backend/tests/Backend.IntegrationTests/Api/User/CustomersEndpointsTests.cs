using System.Net;
using Backend.Api;

namespace Backend.IntegrationTests;

[Collection("PostgresDocker")]
public class CustomersEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;

    public CustomersEndpointsTests(MarketplaceApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCustomers_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/customers", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomerById_ReturnsNotFound()
    {
        // Act
        var customerId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/customers/{customerId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCartByCustomerId_ReturnsNotFound()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/customers/{customerId}/cart?currency=USD", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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
