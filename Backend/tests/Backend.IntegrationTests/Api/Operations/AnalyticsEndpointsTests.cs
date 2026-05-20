using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Backend.Api;
using Backend.Api.Contracts.Operation.Analytics;
using Backend.Infrastructure.Persistence;
using Backend.IntegrationTests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using DomainEnums = Backend.Domain.Enums;

namespace Backend.IntegrationTests;

[Collection("PostgresDocker")]
public class AnalyticsEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;
    private readonly MarketplaceApiFactory _factory;

    public AnalyticsEndpointsTests(MarketplaceApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetSalesStatistics_ReturnsNotImplemented()
    {
        // Arrange: seed orders in a year that belongs to this test only
        var adminId = await SeedUserAsync("analytics-sales-admin@example.com", isAdmin: true);
        var (customerId, addressId) = await SeedCustomerWithAddressAsync("analytics-sales-customer@example.com");
        var purchasedAt = new DateTimeOffset(2030, 6, 1, 0, 0, 0, TimeSpan.Zero);
        await SeedOrderAsync(customerId, addressId, DomainEnums.OrderStatus.Delivered, total: 100m, purchasedAt: purchasedAt);
        await SeedOrderAsync(customerId, addressId, DomainEnums.OrderStatus.Delivered, total: 200m, purchasedAt: purchasedAt);
        await SeedOrderAsync(customerId, addressId, DomainEnums.OrderStatus.Cancelled, total: 999m, purchasedAt: purchasedAt);
        AuthenticateAs(adminId, isAdmin: true);

        var from = new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2030, 12, 31, 23, 59, 59, TimeSpan.Zero);

        // Act
        var response = await _client.GetAsync(
            $"/api/analytics/sales?fromUtc={Uri.EscapeDataString(from.ToString("O"))}&toUtc={Uri.EscapeDataString(to.ToString("O"))}",
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<SalesStatisticsResponse>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(2, body.OrderCount);
        Assert.Equal(300m, body.TotalSalesAmount);
        Assert.Equal(150m, body.AverageOrderValue);
    }

    [Fact]
    public async Task GetOrdersStatistics_ReturnsNotImplemented()
    {
        // Arrange: seed orders in a year that belongs to this test only
        var adminId = await SeedUserAsync("analytics-orders-admin@example.com", isAdmin: true);
        var (customerId, addressId) = await SeedCustomerWithAddressAsync("analytics-orders-customer@example.com");
        var purchasedAt = new DateTimeOffset(2031, 6, 1, 0, 0, 0, TimeSpan.Zero);
        await SeedOrderAsync(customerId, addressId, DomainEnums.OrderStatus.Delivered, purchasedAt: purchasedAt);
        await SeedOrderAsync(customerId, addressId, DomainEnums.OrderStatus.Delivered, purchasedAt: purchasedAt);
        await SeedOrderAsync(customerId, addressId, DomainEnums.OrderStatus.Cancelled, purchasedAt: purchasedAt);
        await SeedOrderAsync(customerId, addressId, DomainEnums.OrderStatus.Pending, purchasedAt: purchasedAt);
        AuthenticateAs(adminId, isAdmin: true);

        var from = new DateTimeOffset(2031, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2031, 12, 31, 23, 59, 59, TimeSpan.Zero);

        // Act
        var response = await _client.GetAsync(
            $"/api/analytics/orders?fromUtc={Uri.EscapeDataString(from.ToString("O"))}&toUtc={Uri.EscapeDataString(to.ToString("O"))}",
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<OrdersStatisticsResponse>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(4, body.TotalOrders);
        Assert.Equal(1, body.CancelledOrders);
        Assert.Equal(2, body.CompletedOrders);
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private async Task<Guid> SeedUserAsync(string email, bool isAdmin)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = TestEntityFactory.CreateUserAccount(email);
        user.IsAdmin = isAdmin;

        dbContext.UserAccounts.Add(user);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return user.Id;
    }

    private async Task<(Guid CustomerId, Guid AddressId)> SeedCustomerWithAddressAsync(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var address = TestEntityFactory.CreateAddress();
        var user = TestEntityFactory.CreateUserAccount(email);
        var customer = TestEntityFactory.CreateCustomer(user.Id);

        dbContext.Addresses.Add(address);
        dbContext.UserAccounts.Add(user);
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return (customer.Id, address.Id);
    }

    private async Task SeedOrderAsync(
        Guid customerId,
        Guid addressId,
        DomainEnums.OrderStatus status,
        decimal total = 100m,
        DateTimeOffset? purchasedAt = null)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var order = TestEntityFactory.CreateOrder(
            customerId,
            addressId,
            $"ORD-{Guid.NewGuid():N}".Substring(0, 16),
            purchasedAt ?? DateTimeOffset.UtcNow);
        order.OrderStatus = status;
        order.SubtotalAmount = total;
        order.FreightAmount = 0m;
        order.TotalAmount = total;

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private void AuthenticateAs(Guid userId, bool isAdmin)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            IntegrationTestAuth.CreateBearerToken(userId, isAdmin));
    }
}
