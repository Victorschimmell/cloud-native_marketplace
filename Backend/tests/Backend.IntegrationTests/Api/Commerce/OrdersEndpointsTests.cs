using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Backend.Api.Contracts.Commerce.Orders;
using Backend.Api.Contracts.Commerce.Shipments;
using Backend.Infrastructure.Persistence;
using Backend.IntegrationTests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using DomainOrderStatus = Backend.Domain.Enums.OrderStatus;

namespace Backend.IntegrationTests;

[Collection("PostgresDocker")]
public class OrdersEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;
    private readonly MarketplaceApiFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions = IntegrationTestJson.Options;

    public OrdersEndpointsTests(MarketplaceApiFactory factory)
    {
        _factory = factory;
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

    [Fact]
    public async Task CancelOrder_WhenAuthenticatedCustomerOwnsPendingOrder_ReturnsCancelledOrder()
    {
        // Arrange
        var (userId, orderId) = await SeedOrderAsync(
            "cancel-owner@example.com",
            DomainOrderStatus.Pending,
            $"ORDER-CANCEL-{Guid.NewGuid():N}");
        AuthenticateAs(userId);

        var cancelRequest = new CancelOrderRequest
        {
            Reason = "Customer requested cancellation."
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/orders/{orderId}/cancel?currency=USD",
            cancelRequest,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var order = await response.Content.ReadFromJsonAsync<OrderModel>(
            _jsonOptions,
            TestContext.Current.CancellationToken);

        Assert.NotNull(order);
        Assert.Equal(orderId, order.Id);
        Assert.Equal(userId, order.UserId);
        Assert.Equal(OrderStatus.Cancelled, order.OrderStatus);
        Assert.Equal("USD", order.CurrencyCode);
        Assert.Equal(20.70m, order.TotalAmount);
    }

    [Fact]
    public async Task CancelOrder_WhenOrderStatusIsNotCancellable_ReturnsBadRequest()
    {
        // Arrange
        var (userId, orderId) = await SeedOrderAsync(
            "cancel-status@example.com",
            DomainOrderStatus.Shipped,
            $"ORDER-SHIP-{Guid.NewGuid():N}");
        AuthenticateAs(userId);

        var cancelRequest = new CancelOrderRequest
        {
            Reason = "Too late to cancel."
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/orders/{orderId}/cancel?currency=BRL",
            cancelRequest,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var responseJson = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal(
            "Order cannot be cancelled in its current status of Shipped.",
            responseJson.RootElement.GetProperty("error").GetString());
    }

    [Fact]
    public async Task CancelOrder_WhenOrderDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var userId = await SeedCustomerAsync("cancel-missing@example.com");
        AuthenticateAs(userId);
        var cancelRequest = new CancelOrderRequest
        {
            Reason = "Order does not exist."
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/orders/{Guid.NewGuid()}/cancel?currency=BRL",
            cancelRequest,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CancelOrder_WhenOrderBelongsToAnotherCustomer_ReturnsUnauthorized()
    {
        // Arrange
        var (_, orderId) = await SeedOrderAsync(
            "cancel-owner-mismatch@example.com",
            DomainOrderStatus.Pending,
            $"ORDER-OWNER-{Guid.NewGuid():N}");
        var otherUserId = await SeedCustomerAsync("cancel-other@example.com");
        AuthenticateAs(otherUserId);

        var cancelRequest = new CancelOrderRequest
        {
            Reason = "Not the owner."
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/orders/{orderId}/cancel?currency=BRL",
            cancelRequest,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CancelOrder_WhenCurrencyIsUnsupported_ReturnsBadRequest()
    {
        // Arrange
        var (userId, orderId) = await SeedOrderAsync(
            "cancel-currency@example.com",
            DomainOrderStatus.Pending,
            $"ORDER-CURRENCY-{Guid.NewGuid():N}");
        AuthenticateAs(userId);

        var cancelRequest = new CancelOrderRequest
        {
            Reason = "Unsupported currency."
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/orders/{orderId}/cancel?currency=EUR",
            cancelRequest,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var responseJson = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal(
            "Currency must be one of BRL, USD, or DKK.",
            responseJson.RootElement.GetProperty("error").GetString());
    }

    private async Task<(Guid UserId, Guid OrderId)> SeedOrderAsync(
        string email,
        DomainOrderStatus status,
        string orderNumber)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var customerUser = TestEntityFactory.CreateUserAccount(email);
        var customer = TestEntityFactory.CreateCustomer(customerUser.Id);
        var address = TestEntityFactory.CreateAddress();
        var order = TestEntityFactory.CreateOrder(customer.Id, address.Id, orderNumber, DateTimeOffset.UtcNow);
        order.OrderStatus = status;

        dbContext.UserAccounts.Add(customerUser);
        dbContext.Customers.Add(customer);
        dbContext.Addresses.Add(address);
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return (customerUser.Id, order.Id);
    }

    private async Task<Guid> SeedCustomerAsync(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var customerUser = TestEntityFactory.CreateUserAccount(email);
        var customer = TestEntityFactory.CreateCustomer(customerUser.Id);

        dbContext.UserAccounts.Add(customerUser);
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return customerUser.Id;
    }

    private void AuthenticateAs(Guid userId)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            IntegrationTestAuth.CreateBearerToken(userId));
    }
}
