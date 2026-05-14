using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Backend.Api;
using Backend.Api.Contracts.Commerce.Reviews;
using Backend.Domain.Entities.Orders;
using Backend.Domain.Enums;
using Backend.Infrastructure.Persistence;
using Backend.IntegrationTests.TestSupport;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.IntegrationTests;

[Collection("PostgresDocker")]
public class ReviewsEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;
    private readonly MarketplaceApiFactory _factory;

    public ReviewsEndpointsTests(MarketplaceApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RecordReview_WhenUnauthenticated_ReturnsUnauthorized()
    {
        var recordRequest = new RecordReviewRequest
        {
            OrderId = Guid.NewGuid(),
            OrderItemId = 1,
            ReviewScore = 5
        };

        var response = await _client.PostAsJsonAsync("/api/reviews", recordRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RecordReview_ForDeliveredPurchasedOrderItem_ReturnsReview()
    {
        var scenario = await SeedDeliveredOrderItemAsync();
        AuthenticateAs(scenario.CustomerUserId);

        var response = await _client.PostAsJsonAsync(
            "/api/reviews",
            new RecordReviewRequest
            {
                OrderId = scenario.OrderId,
                OrderItemId = scenario.OrderItemId,
                ReviewScore = 5,
                ReviewCommentTitle = "Excellent",
                ReviewCommentMessage = "Arrived in great condition."
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var review = await response.Content.ReadFromJsonAsync<ReviewResponse>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);

        Assert.NotNull(review);
        Assert.Equal(scenario.OrderId, review.OrderId);
        Assert.Equal(scenario.OrderItemId, review.OrderItemId);
        Assert.Equal(scenario.ProductId, review.ProductId);
        Assert.Equal(5, review.ReviewScore);
        Assert.Equal("Test Customer", review.ReviewerDisplayName);
    }

    [Fact]
    public async Task RecordReview_WhenOrderItemAlreadyReviewed_ReturnsConflict()
    {
        var scenario = await SeedDeliveredOrderItemAsync();
        AuthenticateAs(scenario.CustomerUserId);

        var request = new RecordReviewRequest
        {
            OrderId = scenario.OrderId,
            OrderItemId = scenario.OrderItemId,
            ReviewScore = 4
        };

        var firstResponse = await _client.PostAsJsonAsync("/api/reviews", request, TestContext.Current.CancellationToken);
        var secondResponse = await _client.PostAsJsonAsync("/api/reviews", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task RecordReview_WhenCustomerAlreadyReviewedProductFromAnotherOrder_ReturnsConflict()
    {
        var scenario = await SeedDeliveredOrderItemAsync();
        var secondScenario = await SeedDeliveredOrderItemAsync(
            customerUserId: scenario.CustomerUserId,
            productId: scenario.ProductId,
            productName: "Repeated product review");
        AuthenticateAs(scenario.CustomerUserId);

        var firstResponse = await _client.PostAsJsonAsync(
            "/api/reviews",
            new RecordReviewRequest
            {
                OrderId = scenario.OrderId,
                OrderItemId = scenario.OrderItemId,
                ReviewScore = 5
            },
            TestContext.Current.CancellationToken);
        var secondResponse = await _client.PostAsJsonAsync(
            "/api/reviews",
            new RecordReviewRequest
            {
                OrderId = secondScenario.OrderId,
                OrderItemId = secondScenario.OrderItemId,
                ReviewScore = 4
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }


    [Fact]
    public async Task RecordReview_ForReturnedPurchasedOrderItem_ReturnsReview()
    {
        var scenario = await SeedDeliveredOrderItemAsync(OrderStatus.Returned, deliveredAt: DateTimeOffset.UtcNow.AddDays(-2));
        AuthenticateAs(scenario.CustomerUserId);

        var response = await _client.PostAsJsonAsync(
            "/api/reviews",
            new RecordReviewRequest
            {
                OrderId = scenario.OrderId,
                OrderItemId = scenario.OrderItemId,
                ReviewScore = 3
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RecordReview_ForAnotherCustomersOrder_ReturnsNotFound()
    {
        var scenario = await SeedDeliveredOrderItemAsync();
        var otherCustomerUserId = await SeedCustomerUserAsync("other-reviewer");
        AuthenticateAs(otherCustomerUserId);

        var response = await _client.PostAsJsonAsync(
            "/api/reviews",
            new RecordReviewRequest
            {
                OrderId = scenario.OrderId,
                OrderItemId = scenario.OrderItemId,
                ReviewScore = 4
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RecordReview_ForUndeliveredOrder_ReturnsBadRequest()
    {
        var scenario = await SeedDeliveredOrderItemAsync(OrderStatus.Processing, deliveredAt: null);
        AuthenticateAs(scenario.CustomerUserId);

        var response = await _client.PostAsJsonAsync(
            "/api/reviews",
            new RecordReviewRequest
            {
                OrderId = scenario.OrderId,
                OrderItemId = scenario.OrderItemId,
                ReviewScore = 4
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetProductReviews_ReturnsReviewsForThatProductOnly()
    {
        var targetScenario = await SeedDeliveredOrderItemAsync();
        var otherScenario = await SeedDeliveredOrderItemAsync(productName: "Different review product");

        await SeedReviewAsync(targetScenario, 5);
        await SeedReviewAsync(otherScenario, 2);

        var response = await _client.GetAsync($"/api/products/{targetScenario.ProductId}/reviews", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var reviews = await response.Content.ReadFromJsonAsync<IReadOnlyList<ReviewResponse>>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);

        Assert.NotNull(reviews);
        var review = Assert.Single(reviews);
        Assert.Equal(targetScenario.ProductId, review.ProductId);
        Assert.Equal(5, review.ReviewScore);
    }

    private void AuthenticateAs(Guid userId)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            IntegrationTestAuth.CreateBearerToken(userId));
    }

    private async Task<Guid> SeedCustomerUserAsync(string emailPrefix)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = TestEntityFactory.CreateUserAccount($"{emailPrefix}-{Guid.NewGuid():N}@customer.example");
        var customer = TestEntityFactory.CreateCustomer(user.Id);

        dbContext.UserAccounts.Add(user);
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return user.Id;
    }

    private async Task<ReviewScenario> SeedDeliveredOrderItemAsync(
        OrderStatus orderStatus = OrderStatus.Delivered,
        DateTimeOffset? deliveredAt = null,
        string productName = "Reviewable product",
        Guid? customerUserId = null,
        Guid? productId = null)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var customerUser = customerUserId.HasValue
            ? await dbContext.UserAccounts.FindAsync([customerUserId.Value], TestContext.Current.CancellationToken)
            : TestEntityFactory.CreateUserAccount($"{Guid.NewGuid():N}@customer.example");
        Assert.NotNull(customerUser);

        var customer = customerUserId.HasValue
            ? dbContext.Customers.Single(customer => customer.UserId == customerUserId.Value)
            : TestEntityFactory.CreateCustomer(customerUser.Id);
        var sellerUser = TestEntityFactory.CreateUserAccount($"{Guid.NewGuid():N}@seller.example");
        var seller = TestEntityFactory.CreateSeller(sellerUser.Id);
        var address = TestEntityFactory.CreateAddress();
        var category = TestEntityFactory.CreateCategory("review_category", "Review category");
        var product = productId.HasValue
            ? await dbContext.Products.FindAsync([productId.Value], TestContext.Current.CancellationToken)
            : TestEntityFactory.CreateProduct(category.Id, productName);
        Assert.NotNull(product);
        var listing = TestEntityFactory.CreateListing(seller.Id, product.Id, $"REVIEW-{Guid.NewGuid():N}", 42m);
        var order = TestEntityFactory.CreateOrder(customer.Id, address.Id, $"ORDER-{Guid.NewGuid():N}", DateTimeOffset.UtcNow.AddDays(-4));

        order.OrderStatus = orderStatus;
        order.OrderDeliveredCustomerDateUtc = deliveredAt ?? (orderStatus == OrderStatus.Delivered ? DateTimeOffset.UtcNow.AddDays(-1) : null);

        var orderItem = new OrderItem
        {
            OrderId = order.Id,
            OrderItemId = 1,
            ListingId = listing.Id,
            ProductId = product.Id,
            SellerId = seller.Id,
            Quantity = 1,
            UnitPrice = 42m,
            FreightValue = 5m
        };

        if (!customerUserId.HasValue)
        {
            dbContext.UserAccounts.Add(customerUser);
            dbContext.Customers.Add(customer);
        }

        dbContext.UserAccounts.Add(sellerUser);
        dbContext.Sellers.Add(seller);
        dbContext.Addresses.Add(address);
        if (!productId.HasValue)
        {
            dbContext.ProductCategories.Add(category);
            dbContext.Products.Add(product);
        }

        dbContext.ProductListings.Add(listing);
        dbContext.Orders.Add(order);
        dbContext.OrderItems.Add(orderItem);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return new ReviewScenario(customerUser.Id, customer.Id, order.Id, orderItem.OrderItemId, product.Id);
    }

    private async Task SeedReviewAsync(ReviewScenario scenario, int score)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.OrderReviews.Add(new OrderReview
        {
            OrderId = scenario.OrderId,
            OrderItemId = scenario.OrderItemId,
            CustomerId = scenario.CustomerId,
            ProductId = scenario.ProductId,
            ReviewScore = score,
            ReviewCommentMessage = $"Score {score}",
            ReviewCreationDateUtc = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private sealed record ReviewScenario(Guid CustomerUserId, Guid CustomerId, Guid OrderId, int OrderItemId, Guid ProductId);
}
