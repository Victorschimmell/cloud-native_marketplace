using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Backend.Api;
using Backend.Api.Contracts.Commerce.Orders;
using Backend.Api.Contracts.Common;
using Backend.Domain.Entities.Orders;
using Backend.Infrastructure.Persistence;
using Backend.IntegrationTests.TestSupport;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.IntegrationTests;

[Collection("PostgresDocker")]
public class CustomersEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;
    private readonly MarketplaceApiFactory _factory;

    public CustomersEndpointsTests(MarketplaceApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCustomers_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/customers", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomers_WhenAdmin_ReturnsOk()
    {
        // Arrange
        var adminUserId = await SeedAdminAsync("customers-admin@example.com");
        AuthenticateAs(adminUserId, isAdmin: true);

        // Act
        var response = await _client.GetAsync("/api/customers", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomerById_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Act
        var customerId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/customers/{customerId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomerById_WhenAuthenticatedCustomerRequestsAnotherCustomer_ReturnsForbidden()
    {
        // Arrange
        var userId = await SeedCustomerAsync("customer-access@example.com");
        AuthenticateAs(userId);

        // Act
        var response = await _client.GetAsync($"/api/customers/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomerById_WhenAuthenticatedCustomerRequestsSelf_ReturnsOk()
    {
        // Arrange
        var userId = await SeedCustomerAsync("customer-self@example.com");
        AuthenticateAs(userId);

        // Act
        var response = await _client.GetAsync($"/api/customers/{userId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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
    public async Task GetOrdersByCustomerId_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/customers/{customerId}/orders", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetOrdersByCustomerId_WhenAuthenticatedCustomerHasOrders_ReturnsOrderHistory()
    {
        // Arrange
        var (userId, _) = await SeedCustomerOrderAsync("orders-history@example.com", "History product", "ORDER-HISTORY-001");
        AuthenticateAs(userId);

        // Act
        var response = await _client.GetAsync($"/api/customers/{userId}/orders?currency=BRL", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var orders = await response.Content.ReadFromJsonAsync<PageResponse<OrderSummaryModel>>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);

        Assert.NotNull(orders);
        Assert.Equal(1, orders.TotalCount);
        var order = Assert.Single(orders.Items);
        Assert.Equal(userId, order.UserId);
        Assert.Equal(115m, order.TotalAmount);
        var item = Assert.Single(order.Items);
        Assert.Equal("History product", item.ProductName);
    }

    [Fact]
    public async Task GetOrderById_WhenAuthenticatedCustomerOwnsOrder_ReturnsOrderDetails()
    {
        // Arrange
        var (userId, orderId) = await SeedCustomerOrderAsync("orders-details@example.com", "Details product", "ORDER-DETAILS-001");
        AuthenticateAs(userId);

        // Act
        var response = await _client.GetAsync($"/api/orders/{orderId}?currency=USD", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var order = await response.Content.ReadFromJsonAsync<OrderModel>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);

        Assert.NotNull(order);
        Assert.Equal(userId, order.UserId);
        Assert.Equal("USD", order.CurrencyCode);
        Assert.Equal(20.7m, order.TotalAmount);
        var item = Assert.Single(order.Items);
        Assert.Equal("Details product", item.ProductName);
    }

    private async Task<(Guid UserId, Guid OrderId)> SeedCustomerOrderAsync(string email, string productName, string orderNumber)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var customerUser = TestEntityFactory.CreateUserAccount(email);
        var customer = TestEntityFactory.CreateCustomer(customerUser.Id);
        var sellerUser = TestEntityFactory.CreateUserAccount($"{Guid.NewGuid():N}@seller.example");
        var seller = TestEntityFactory.CreateSeller(sellerUser.Id);
        var category = TestEntityFactory.CreateCategory("orders_category", "Orders category");
        var product = TestEntityFactory.CreateProduct(category.Id, productName);
        var listing = TestEntityFactory.CreateListing(seller.Id, product.Id, $"{Guid.NewGuid():N}", 100m);
        var address = TestEntityFactory.CreateAddress();
        var order = TestEntityFactory.CreateOrder(customer.Id, address.Id, orderNumber, DateTimeOffset.UtcNow);
        order.OrderStatus = Backend.Domain.Enums.OrderStatus.Approved;
        var orderItem = new OrderItem
        {
            OrderId = order.Id,
            OrderItemId = 1,
            ListingId = listing.Id,
            ProductId = product.Id,
            SellerId = seller.Id,
            Quantity = 1,
            UnitPrice = 100m,
            FreightValue = 15m
        };

        dbContext.UserAccounts.AddRange(customerUser, sellerUser);
        dbContext.Customers.Add(customer);
        dbContext.Sellers.Add(seller);
        dbContext.ProductCategories.Add(category);
        dbContext.Products.Add(product);
        dbContext.ProductListings.Add(listing);
        dbContext.Addresses.Add(address);
        dbContext.Orders.Add(order);
        dbContext.OrderItems.Add(orderItem);
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

    private async Task<Guid> SeedAdminAsync(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var adminUser = TestEntityFactory.CreateUserAccount(email);
        adminUser.IsAdmin = true;

        dbContext.UserAccounts.Add(adminUser);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return adminUser.Id;
    }

    private void AuthenticateAs(Guid userId, bool isAdmin = false)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            IntegrationTestAuth.CreateBearerToken(userId, isAdmin));
    }
}
