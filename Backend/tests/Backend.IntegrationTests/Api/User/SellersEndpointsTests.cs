using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Backend.Api.Contracts.Commerce.Orders;
using Backend.Api.Contracts.Common;
using Backend.Api;
using Backend.Domain.Entities.Orders;
using Backend.Infrastructure.Persistence;
using Backend.IntegrationTests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using DomainOrderStatus = Backend.Domain.Enums.OrderStatus;
using DomainVerificationStatus = Backend.Domain.Enums.VerificationStatus;

namespace Backend.IntegrationTests;

[Collection("PostgresDocker")]
public class SellersEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;
    private readonly MarketplaceApiFactory _factory;

    public SellersEndpointsTests(MarketplaceApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetMyOrders_WhenOrderContainsSellerItem_ReturnsOnlySellerOrdersAndLines()
    {
        // Arrange
        var seed = await SeedSellerOrdersAsync();
        AuthenticateAs(seed.SellerUserId);

        // Act
        var response = await _client.GetAsync(
            "/api/sellers/me/orders?page=1&pageSize=10&currency=BRL",
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var page = await response.Content.ReadFromJsonAsync<PageResponse<SellerOrderSummaryModel>>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);

        Assert.NotNull(page);
        var order = Assert.Single(page.Items);
        Assert.Equal(seed.ExpectedOrderNumber, order.OrderNumber);
        Assert.Equal("Test Customer", order.CustomerName);
        Assert.Equal(seed.CustomerEmail, order.CustomerEmail);
        Assert.Equal(50m, order.SubtotalAmount);
        Assert.Equal(5m, order.FreightAmount);
        Assert.Equal(55m, order.TotalAmount);
        Assert.True(order.CanUpdateStatus);
        Assert.All(order.Items, item => Assert.Equal("Seller owned product", item.ProductName));
        Assert.All(order.Items, item => Assert.Equal(seed.SellerProductImageUrl, item.ImageUrl));
        Assert.All(order.Items, item => Assert.Equal(OrderStatus.Pending, item.FulfillmentStatus));

        var statsResponse = await _client.GetAsync(
            "/api/sellers/me/order-stats?currency=BRL",
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, statsResponse.StatusCode);

        var stats = await statsResponse.Content.ReadFromJsonAsync<SellerOrderStatsModel>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);
        Assert.NotNull(stats);
        Assert.Equal(1, stats.TotalOrders);
        Assert.Equal(1, stats.ActiveOrders);
        Assert.Equal(55m, stats.TotalRevenue);
    }

    [Fact]
    public async Task GetMyOrders_WhenSellerIsNotVerified_ReturnsForbidden()
    {
        // Arrange
        var seed = await SeedSellerOrdersAsync(sellerIsVerified: false);
        AuthenticateAs(seed.SellerUserId);

        // Act
        var response = await _client.GetAsync(
            "/api/sellers/me/orders?page=1&pageSize=10&currency=BRL",
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetMyOrderById_WhenOrderContainsSellerItem_ReturnsOnlySellerOrderLines()
    {
        // Arrange
        var seed = await SeedSellerOrdersAsync();
        AuthenticateAs(seed.SellerUserId);

        // Act
        var response = await _client.GetAsync(
            $"/api/sellers/me/orders/{seed.SellerOrderId}?currency=BRL",
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var order = await response.Content.ReadFromJsonAsync<SellerOrderSummaryModel>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);

        Assert.NotNull(order);
        Assert.Equal(seed.ExpectedOrderNumber, order.OrderNumber);
        Assert.Equal(50m, order.SubtotalAmount);
        Assert.Equal(5m, order.FreightAmount);
        Assert.Equal(55m, order.TotalAmount);
        Assert.True(order.CanUpdateStatus);
        var item = Assert.Single(order.Items);
        Assert.Equal("Seller owned product", item.ProductName);
        Assert.Equal(seed.SellerProductImageUrl, item.ImageUrl);
        Assert.Equal(OrderStatus.Pending, item.FulfillmentStatus);
    }

    [Fact]
    public async Task UpdateMyOrderStatus_WhenOrderContainsAnotherSellerLine_UpdatesSellerItemsAndWaitsForOtherSellers()
    {
        // Arrange
        var seed = await SeedSellerOrdersAsync();
        AuthenticateAs(seed.SellerUserId);
        var request = new UpdateOrderStatusRequest
        {
            Status = OrderStatus.Approved
        };

        // Act
        var response = await _client.PatchAsJsonAsync(
            $"/api/sellers/me/orders/{seed.SellerOrderId}/status?currency=BRL",
            request,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var order = await response.Content.ReadFromJsonAsync<SellerOrderSummaryModel>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);
        Assert.NotNull(order);
        Assert.Equal(OrderStatus.Pending, order.OrderStatus);
        var sellerItem = Assert.Single(order.Items);
        Assert.Equal(OrderStatus.Approved, sellerItem.FulfillmentStatus);

        AuthenticateAs(seed.OtherSellerUserId);
        var otherSellerResponse = await _client.PatchAsJsonAsync(
            $"/api/sellers/me/orders/{seed.SellerOrderId}/status?currency=BRL",
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, otherSellerResponse.StatusCode);
        var otherSellerOrder = await otherSellerResponse.Content.ReadFromJsonAsync<SellerOrderSummaryModel>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);
        Assert.NotNull(otherSellerOrder);
        Assert.Equal(OrderStatus.Approved, otherSellerOrder.OrderStatus);
    }

    [Fact]
    public async Task UpdateMyOrderStatus_WhenSellerOwnsWholeOrder_DerivesOrderStatusFromItemStatus()
    {
        // Arrange
        var seed = await SeedSellerOrdersAsync(includeOtherSellerLine: false);
        AuthenticateAs(seed.SellerUserId);

        // Act
        var response = await _client.PatchAsJsonAsync(
            $"/api/sellers/me/orders/{seed.SellerOrderId}/status?currency=BRL",
            new UpdateOrderStatusRequest { Status = OrderStatus.Approved },
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        response = await _client.PatchAsJsonAsync(
            $"/api/sellers/me/orders/{seed.SellerOrderId}/status?currency=BRL",
            new UpdateOrderStatusRequest { Status = OrderStatus.Shipped },
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        response = await _client.PatchAsJsonAsync(
            $"/api/sellers/me/orders/{seed.SellerOrderId}/status?currency=BRL",
            new UpdateOrderStatusRequest { Status = OrderStatus.Processing },
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        response = await _client.PatchAsJsonAsync(
            $"/api/sellers/me/orders/{seed.SellerOrderId}/status?currency=BRL",
            new UpdateOrderStatusRequest { Status = OrderStatus.Shipped },
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var order = await response.Content.ReadFromJsonAsync<SellerOrderSummaryModel>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);
        Assert.NotNull(order);
        Assert.Equal(OrderStatus.Shipped, order.OrderStatus);
        Assert.NotNull(order.OrderDeliveredCarrierDateUtc);
        Assert.True(order.CanUpdateStatus);
        Assert.All(order.Items, item => Assert.Equal("Seller owned product", item.ProductName));
        Assert.All(order.Items, item => Assert.Equal(seed.SellerProductImageUrl, item.ImageUrl));
        Assert.All(order.Items, item => Assert.Equal(OrderStatus.Shipped, item.FulfillmentStatus));

        response = await _client.PatchAsJsonAsync(
            $"/api/sellers/me/orders/{seed.SellerOrderId}/status?currency=BRL",
            new UpdateOrderStatusRequest { Status = OrderStatus.Delivered },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        order = await response.Content.ReadFromJsonAsync<SellerOrderSummaryModel>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);
        Assert.NotNull(order);
        Assert.Equal(OrderStatus.Delivered, order.OrderStatus);
        Assert.NotNull(order.OrderDeliveredCustomerDateUtc);
        Assert.False(order.CanUpdateStatus);
        Assert.All(order.Items, item => Assert.Equal(OrderStatus.Delivered, item.FulfillmentStatus));

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var savedOrder = await dbContext.Orders.FindAsync([seed.SellerOrderId], TestContext.Current.CancellationToken);
        Assert.NotNull(savedOrder);
        Assert.Equal(DomainOrderStatus.Delivered, savedOrder.OrderStatus);
        Assert.NotNull(savedOrder.OrderDeliveredCarrierDateUtc);
        Assert.NotNull(savedOrder.OrderDeliveredCustomerDateUtc);
        var savedItem = await dbContext.OrderItems.FindAsync([seed.SellerOrderId, 1], TestContext.Current.CancellationToken);
        Assert.NotNull(savedItem);
        Assert.Equal(DomainOrderStatus.Delivered, savedItem.FulfillmentStatus);
        Assert.NotNull(savedItem.FulfillmentApprovedAtUtc);
        Assert.NotNull(savedItem.FulfillmentProcessingAtUtc);
        Assert.NotNull(savedItem.FulfillmentShippedAtUtc);
    }

    private async Task<SellerOrdersSeed> SeedSellerOrdersAsync(bool includeOtherSellerLine = true, bool sellerIsVerified = true)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var unique = Guid.NewGuid().ToString("N");
        var customerEmail = $"seller-orders-customer-{unique}@example.com";
        var customerUser = TestEntityFactory.CreateUserAccount(customerEmail);
        var customer = TestEntityFactory.CreateCustomer(customerUser.Id);
        var sellerUser = TestEntityFactory.CreateUserAccount($"seller-orders-seller-{unique}@example.com");
        var seller = TestEntityFactory.CreateSeller(sellerUser.Id);
        if (sellerIsVerified)
        {
            seller.VerificationStatus = DomainVerificationStatus.Verified;
        }
        var otherSellerUser = TestEntityFactory.CreateUserAccount($"seller-orders-other-{unique}@example.com");
        var otherSeller = TestEntityFactory.CreateSeller(otherSellerUser.Id);
        otherSeller.VerificationStatus = DomainVerificationStatus.Verified;
        var address = TestEntityFactory.CreateAddress();
        var category = TestEntityFactory.CreateCategory("categoria", "Category");
        var sellerProduct = TestEntityFactory.CreateProduct(category.Id, "Seller owned product");
        sellerProduct.ImageUrl = $"https://example.com/seller-product-{unique}.jpg";
        var otherProduct = TestEntityFactory.CreateProduct(category.Id, "Other seller product");
        var sellerListing = TestEntityFactory.CreateListing(seller.Id, sellerProduct.Id, $"SKU-{Guid.NewGuid():N}", 25m);
        var otherListing = TestEntityFactory.CreateListing(otherSeller.Id, otherProduct.Id, $"SKU-{Guid.NewGuid():N}", 100m);
        var expectedOrderNumber = $"SELLER-ORDER-{Guid.NewGuid():N}";
        var sellerOrder = TestEntityFactory.CreateOrder(customer.Id, address.Id, expectedOrderNumber, DateTimeOffset.UtcNow);
        sellerOrder.OrderStatus = DomainOrderStatus.Pending;
        sellerOrder.SubtotalAmount = 150m;
        sellerOrder.FreightAmount = 15m;
        sellerOrder.TotalAmount = 165m;
        var otherOrder = TestEntityFactory.CreateOrder(customer.Id, address.Id, $"OTHER-ORDER-{Guid.NewGuid():N}", DateTimeOffset.UtcNow.AddMinutes(-5));

        dbContext.UserAccounts.AddRange(customerUser, sellerUser, otherSellerUser);
        dbContext.Customers.Add(customer);
        dbContext.Sellers.AddRange(seller, otherSeller);
        dbContext.Addresses.Add(address);
        dbContext.ProductCategories.Add(category);
        dbContext.Products.AddRange(sellerProduct, otherProduct);
        dbContext.ProductListings.AddRange(sellerListing, otherListing);
        dbContext.Orders.AddRange(sellerOrder, otherOrder);
        dbContext.Set<OrderItem>().Add(new OrderItem
        {
            OrderId = sellerOrder.Id,
            OrderItemId = 1,
            ListingId = sellerListing.Id,
            ProductId = sellerProduct.Id,
            SellerId = seller.Id,
            Quantity = 2,
            UnitPrice = 25m,
            FreightValue = 5m
        });

        if (includeOtherSellerLine)
        {
            dbContext.Set<OrderItem>().Add(new OrderItem
            {
                OrderId = sellerOrder.Id,
                OrderItemId = 2,
                ListingId = otherListing.Id,
                ProductId = otherProduct.Id,
                SellerId = otherSeller.Id,
                Quantity = 1,
                UnitPrice = 100m,
                FreightValue = 10m
            });
        }

        dbContext.Set<OrderItem>().Add(new OrderItem
        {
            OrderId = otherOrder.Id,
            OrderItemId = 1,
            ListingId = otherListing.Id,
            ProductId = otherProduct.Id,
            SellerId = otherSeller.Id,
            Quantity = 1,
            UnitPrice = 100m,
            FreightValue = 10m
        });

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return new SellerOrdersSeed(sellerUser.Id, otherSellerUser.Id, sellerOrder.Id, expectedOrderNumber, customerEmail, sellerProduct.ImageUrl);
    }

    private void AuthenticateAs(Guid userId)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            IntegrationTestAuth.CreateBearerToken(userId));
    }

    private sealed record SellerOrdersSeed(
        Guid SellerUserId,
        Guid OtherSellerUserId,
        Guid SellerOrderId,
        string ExpectedOrderNumber,
        string CustomerEmail,
        string? SellerProductImageUrl);
}
