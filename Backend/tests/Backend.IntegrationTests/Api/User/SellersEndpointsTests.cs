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
    public async Task GetSellers_ReturnsNotImplemented()
    {
        // Act
        var response = await _client.GetAsync("/api/sellers", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task GetSellerById_ReturnsNotImplemented()
    {
        // Act
        var sellerId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/sellers/{sellerId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task GetMyOrders_WhenOrderContainsSellerItem_ReturnsOnlySellerOrdersAndLines()
    {
        // Arrange
        var (sellerUserId, expectedOrderNumber) = await SeedSellerOrdersAsync();
        AuthenticateAs(sellerUserId);

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
        Assert.Equal(expectedOrderNumber, order.OrderNumber);
        Assert.Equal("Test Customer", order.CustomerName);
        Assert.Equal("seller-orders-customer@example.com", order.CustomerEmail);
        Assert.Equal(50m, order.SubtotalAmount);
        Assert.Equal(5m, order.FreightAmount);
        Assert.Equal(55m, order.TotalAmount);
        Assert.All(order.Items, item => Assert.Equal("Seller owned product", item.ProductName));

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

    private async Task<(Guid SellerUserId, string ExpectedOrderNumber)> SeedSellerOrdersAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var customerUser = TestEntityFactory.CreateUserAccount("seller-orders-customer@example.com");
        var customer = TestEntityFactory.CreateCustomer(customerUser.Id);
        var sellerUser = TestEntityFactory.CreateUserAccount("seller-orders-seller@example.com");
        var seller = TestEntityFactory.CreateSeller(sellerUser.Id);
        var otherSellerUser = TestEntityFactory.CreateUserAccount("seller-orders-other@example.com");
        var otherSeller = TestEntityFactory.CreateSeller(otherSellerUser.Id);
        var address = TestEntityFactory.CreateAddress();
        var category = TestEntityFactory.CreateCategory("categoria", "Category");
        var sellerProduct = TestEntityFactory.CreateProduct(category.Id, "Seller owned product");
        var otherProduct = TestEntityFactory.CreateProduct(category.Id, "Other seller product");
        var sellerListing = TestEntityFactory.CreateListing(seller.Id, sellerProduct.Id, $"SKU-{Guid.NewGuid():N}", 25m);
        var otherListing = TestEntityFactory.CreateListing(otherSeller.Id, otherProduct.Id, $"SKU-{Guid.NewGuid():N}", 100m);
        var expectedOrderNumber = $"SELLER-ORDER-{Guid.NewGuid():N}";
        var sellerOrder = TestEntityFactory.CreateOrder(customer.Id, address.Id, expectedOrderNumber, DateTimeOffset.UtcNow);
        sellerOrder.OrderStatus = DomainOrderStatus.Approved;
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
        dbContext.Set<OrderItem>().AddRange(
            new OrderItem
            {
                OrderId = sellerOrder.Id,
                OrderItemId = 1,
                ListingId = sellerListing.Id,
                ProductId = sellerProduct.Id,
                SellerId = seller.Id,
                Quantity = 2,
                UnitPrice = 25m,
                FreightValue = 5m
            },
            new OrderItem
            {
                OrderId = sellerOrder.Id,
                OrderItemId = 2,
                ListingId = otherListing.Id,
                ProductId = otherProduct.Id,
                SellerId = otherSeller.Id,
                Quantity = 1,
                UnitPrice = 100m,
                FreightValue = 10m
            },
            new OrderItem
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

        return (sellerUser.Id, expectedOrderNumber);
    }

    private void AuthenticateAs(Guid userId)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            IntegrationTestAuth.CreateBearerToken(userId));
    }
}
