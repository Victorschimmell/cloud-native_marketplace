using System.Net;
using System.Net.Http.Json;
using Backend.Api;
using Backend.Api.Contracts.Commerce.Cart;
using Backend.Infrastructure.Persistence;
using Backend.IntegrationTests.TestSupport;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.IntegrationTests;

[Collection("PostgresDocker")]
public class CartEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;
    private readonly MarketplaceApiFactory _factory;

    public CartEndpointsTests(MarketplaceApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AddCartItem_ReturnsCartWithItem()
    {
        // Arrange
        var listingId = await SeedProductListingAsync("Cart product", "CART-001", 39.95m);
        var addItemRequest = new AddCartItemRequest
        {
            ListingId = listingId,
            Quantity = 2
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/cart", addItemRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var cart = await response.Content.ReadFromJsonAsync<CartResponse>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);
        Assert.NotNull(cart);
        Assert.Null(cart.SessionId);
        Assert.Null(cart.UserId);
        var item = Assert.Single(cart.Items);
        Assert.Equal(listingId, item.ListingId);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(39.95m, item.UnitPriceAtAddition);
    }

    [Fact]
    public async Task AddCartItem_WithExistingCartAndDifferentListing_ReturnsCartWithBothItems()
    {
        // Arrange
        var firstListingId = await SeedProductListingAsync("First cart product", "CART-002", 19.95m);
        var secondListingId = await SeedProductListingAsync("Second cart product", "CART-003", 29.95m);
        var firstResponse = await _client.PostAsJsonAsync(
            "/api/cart",
            new AddCartItemRequest
            {
                ListingId = firstListingId,
                Quantity = 1
            },
            TestContext.Current.CancellationToken);

        var firstCart = await firstResponse.Content.ReadFromJsonAsync<CartResponse>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);
        Assert.NotNull(firstCart);

        // Act
        var secondResponse = await _client.PostAsJsonAsync(
            "/api/cart",
            new AddCartItemRequest
            {
                CartId = firstCart.Id,
                ListingId = secondListingId,
                Quantity = 1
            },
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        var cart = await secondResponse.Content.ReadFromJsonAsync<CartResponse>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);
        Assert.NotNull(cart);
        Assert.Equal(firstCart.Id, cart.Id);
        Assert.Equal(2, cart.Items.Count);
        Assert.Contains(cart.Items, item => item.ListingId == firstListingId);
        Assert.Contains(cart.Items, item => item.ListingId == secondListingId);
    }

    [Fact]
    public async Task AddCartItem_WhenListingIsOutOfStock_ReturnsBadRequest()
    {
        // Arrange
        var listingId = await SeedProductListingAsync("Out of stock cart product", "CART-OUT-001", 39.95m, inventoryQuantity: 0);
        var addItemRequest = new AddCartItemRequest
        {
            ListingId = listingId,
            Quantity = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/cart", addItemRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddCartItem_WhenQuantityExceedsStock_ReturnsBadRequest()
    {
        // Arrange
        var listingId = await SeedProductListingAsync("Limited stock cart product", "CART-LIMIT-001", 39.95m, inventoryQuantity: 2);
        var addItemRequest = new AddCartItemRequest
        {
            ListingId = listingId,
            Quantity = 3
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/cart", addItemRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RemoveCartItem_ReturnsNotImplemented()
    {
        // Arrange
        var removeItemRequest = new RemoveCartItemRequest
        {
            UserId = Guid.NewGuid(),
            ListingId = Guid.NewGuid()
        };

        // Act
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/cart")
        {
            Content = JsonContent.Create(removeItemRequest)
        };
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    private async Task<Guid> SeedProductListingAsync(string productName, string sku, decimal price, int inventoryQuantity = 10)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var sellerUser = TestEntityFactory.CreateUserAccount($"{Guid.NewGuid():N}@seller.example");
        var seller = TestEntityFactory.CreateSeller(sellerUser.Id);
        var category = TestEntityFactory.CreateCategory("cart_category", "Cart category");
        var product = TestEntityFactory.CreateProduct(category.Id, productName);
        var listing = TestEntityFactory.CreateListing(seller.Id, product.Id, sku, price);
        listing.InventoryQuantity = inventoryQuantity;

        dbContext.UserAccounts.Add(sellerUser);
        dbContext.Sellers.Add(seller);
        dbContext.ProductCategories.Add(category);
        dbContext.Products.Add(product);
        dbContext.ProductListings.Add(listing);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return listing.Id;
    }
}
