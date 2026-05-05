using System.Net;
using System.Net.Http.Json;
using Backend.Api;
using Backend.Api.Contracts.Commerce.Orders;
using Backend.Api.Contracts.Commerce.Shipments;

namespace Backend.IntegrationTests;

[Collection("PostgresDocker")]
public class OrdersEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;

    public OrdersEndpointsTests(MarketplaceApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetOrderById_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Act
        var orderId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/orders/{orderId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetOrderItems_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Act
        var orderId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/orders/{orderId}/items", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateOrderStatus_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var updateRequest = new UpdateOrderStatusRequest
        {
            Status = OrderStatus.Pending
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/orders/{orderId}", updateRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CancelOrder_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var cancelRequest = new CancelOrderRequest();

        // Act
        var response = await _client.PostAsJsonAsync($"/api/orders/{orderId}/cancel", cancelRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetOrderReviews_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Act
        var orderId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/orders/{orderId}/reviews", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetOrderShipments_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Act
        var orderId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/orders/{orderId}/shipments", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrderShipment_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var recordRequest = new RecordShipmentRequest
        {
            SellerId = Guid.NewGuid(),
            CarrierName = "DHL",
            TrackingNumber = "123456",
            ShipmentStatus = ShipmentStatus.Pending
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/orders/{orderId}/shipments", recordRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
