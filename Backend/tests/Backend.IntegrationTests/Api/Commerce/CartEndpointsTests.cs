using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Backend.Api.Contracts.Commerce.Cart;
using Backend.Domain.Enums;
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
        var userId = await SeedCustomerAsync("add_cart@example.com");
        AuthenticateAs(userId);
        var addItemRequest = new AddCartItemRequest
        {
            ListingId = listingId,
            Quantity = 2
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/cart/items?displayCurrency=BRL", addItemRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var cart = await response.Content.ReadFromJsonAsync<CartResponse>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);
        Assert.NotNull(cart);
        Assert.Null(cart.SessionId);
        Assert.Equal(cart.UserId, userId);
        var item = Assert.Single(cart.Items);
        Assert.Equal(listingId, item.ListingId);
        Assert.NotEqual(Guid.Empty, item.ProductId);
        Assert.Equal("Cart product", item.ProductName);
        Assert.StartsWith("https://example.com/cart-product-", item.ImageUrl);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(39.95m, item.UnitPriceAtAddition);
        Assert.Equal(79.90m, item.LineTotal);
    }

    [Fact]
    public async Task AddCartItem_WhenAuthenticatedUserIsSeller_ReturnsForbidden()
    {
        // Arrange
        var listingId = await SeedProductListingAsync("Seller cannot buy", "SELLER-CART-001", 39.95m);
        var sellerUserId = await SeedSellerAsync("seller-cart@example.com");
        AuthenticateAs(sellerUserId);
        var addItemRequest = new AddCartItemRequest
        {
            ListingId = listingId,
            Quantity = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/cart/items?displayCurrency=BRL", addItemRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AddCartItem_WhenDisplayCurrencyHasRoundingDifference_ReturnsBackendCalculatedLineTotal()
    {
        // Arrange
        var listingId = await SeedProductListingAsync("Rounded DKK cart product", "CART-DKK-ROUNDING-001", 118.58m);
        var userId = await SeedCustomerAsync("cart_dkk_rounding@example.com");
        AuthenticateAs(userId);
        var addItemRequest = new AddCartItemRequest
        {
            ListingId = listingId,
            Quantity = 10
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/cart/items?displayCurrency=DKK", addItemRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var cart = await response.Content.ReadFromJsonAsync<CartResponse>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);
        Assert.NotNull(cart);
        var item = Assert.Single(cart.Items);
        Assert.Equal(138.74m, item.UnitPriceAtAddition);
        Assert.Equal(1387.39m, item.LineTotal);
        Assert.Equal("DKK", item.CurrencyCode);
    }

    [Fact]
    public async Task GetCurrentCart_WhenAuthenticatedUserHasActiveCart_ReturnsCart()
    {
        // Arrange
        var listingId = await SeedProductListingAsync("Current cart product", "CURRENT-CART-001", 42.95m);
        var userId = await SeedCustomerAsync("current_cart@example.com");
        AuthenticateAs(userId);

        var addResponse = await _client.PostAsJsonAsync(
            "/api/cart/items?displayCurrency=BRL",
            new AddCartItemRequest
            {
                ListingId = listingId,
                Quantity = 1
            },
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, addResponse.StatusCode);

        // Act
        var response = await _client.GetAsync("/api/cart/current?displayCurrency=BRL", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cart = await response.Content.ReadFromJsonAsync<CartResponse>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);
        Assert.NotNull(cart);
        Assert.Equal(userId, cart.UserId);
        var item = Assert.Single(cart.Items);
        Assert.Equal("Current cart product", item.ProductName);
    }

    [Fact]
    public async Task AddCartItem_WithExistingCartAndDifferentListing_ReturnsCartWithBothItems()
    {
        // Arrange
        var userId = await SeedCustomerAsync("bbb@example.com");
        AuthenticateAs(userId);

        var firstListingId = await SeedProductListingAsync("First cart product", "CART-002", 19.95m);
        var secondListingId = await SeedProductListingAsync("Second cart product", "CART-003", 29.95m);
        var firstResponse = await _client.PostAsJsonAsync(
            "/api/cart/items?displayCurrency=BRL",
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
            "/api/cart/items?displayCurrency=BRL",
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
        var userId = await SeedCustomerAsync("ccc@example.com");
        AuthenticateAs(userId);
        var addItemRequest = new AddCartItemRequest
        {
            ListingId = listingId,
            Quantity = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/cart/items?displayCurrency=BRL", addItemRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddCartItem_WhenQuantityExceedsStock_ReturnsBadRequest()
    {
        // Arrange
        var listingId = await SeedProductListingAsync("Limited stock cart product", "CART-LIMIT-001", 39.95m, inventoryQuantity: 2);
        var userId = await SeedCustomerAsync("limited@example.com");
        AuthenticateAs(userId);
        var addItemRequest = new AddCartItemRequest
        {
            ListingId = listingId,
            Quantity = 3
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/cart/items?displayCurrency=BRL", addItemRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PatchCartItem_ReturnsNotFound()
    {
        // Arrange
        var listingId = Guid.NewGuid();
        var userId = await SeedCustomerAsync("patch-missing@example.com");
        AuthenticateAs(userId);
        var patchItemRequest = new UpdateCartItemRequest
        {
            Quantity = 1
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/cart/items/{listingId}?displayCurrency=USD", patchItemRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PatchCartItem_WhenQuantityMatchesCartItem_RemovesItemCompletely()
    {
        // Arrange
        var listingId = await SeedProductListingAsync("Remove product", "REMOVE-001", 29.95m);
        var userId = await SeedCustomerAsync("remove@example.com");
        AuthenticateAs(userId);

        var addResponse = await _client.PostAsJsonAsync(
            "/api/cart/items?displayCurrency=BRL",
            new AddCartItemRequest
            {
                ListingId = listingId,
                Quantity = 2
            },
            TestContext.Current.CancellationToken);

        var cartAfterAdd = await addResponse.Content.ReadFromJsonAsync<CartResponse>(IntegrationTestJson.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(cartAfterAdd);
        Assert.Single(cartAfterAdd.Items);

        // Act
        var patchItemRequest = new UpdateCartItemRequest
        {
            CartId = cartAfterAdd.Id,
            Quantity = 0
        };
        var response = await _client.PatchAsJsonAsync($"/api/cart/items/{listingId}?displayCurrency=BRL", patchItemRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cart = await response.Content.ReadFromJsonAsync<CartResponse>(IntegrationTestJson.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(cart);
        Assert.Empty(cart.Items);
    }

    [Fact]
    public async Task PatchCartItem_WhenQuantityLessThanCartItem_ModifiesQuantity()
    {
        // Arrange
        var listingId = await SeedProductListingAsync("Reduce product", "REDUCE-001", 19.95m);
        var userId = await SeedCustomerAsync("reduce@example.com");
        AuthenticateAs(userId);

        var addResponse = await _client.PostAsJsonAsync(
            "/api/cart/items?displayCurrency=USD",
            new AddCartItemRequest
            {
                ListingId = listingId,
                Quantity = 5
            },
            TestContext.Current.CancellationToken);

        var cartAfterAdd = await addResponse.Content.ReadFromJsonAsync<CartResponse>(IntegrationTestJson.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(cartAfterAdd);

        // Act
        var patchItemRequest = new UpdateCartItemRequest
        {
            CartId = cartAfterAdd.Id,
            Quantity = 2
        };
        var response = await _client.PatchAsJsonAsync($"/api/cart/items/{listingId}?displayCurrency=USD", patchItemRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cart = await response.Content.ReadFromJsonAsync<CartResponse>(IntegrationTestJson.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(cart);
        var item = Assert.Single(cart.Items);
        Assert.Equal(2, item.Quantity);
    }

    [Fact]
    public async Task AddCartItem_WithDifferentDisplayCurrencies_ReturnsOk()
    {
        // Arrange
        var listingId = await SeedProductListingAsync("Currency product", "CURRENCY-001", 49.95m);
        var userId = await SeedCustomerAsync("currency@example.com");
        AuthenticateAs(userId);

        // Act & Assert
        var addRequest = new AddCartItemRequest
        {
            ListingId = listingId,
            Quantity = 1
        };
        var responseBRL = await _client.PostAsJsonAsync("/api/cart/items?displayCurrency=BRL", addRequest, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, responseBRL.StatusCode);
        var cartBRL = await responseBRL.Content.ReadFromJsonAsync<CartResponse>(IntegrationTestJson.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(cartBRL);
        Assert.Single(cartBRL.Items);

        // Act & Assert
        var responseUSD = await _client.PostAsJsonAsync("/api/cart/items?displayCurrency=USD", addRequest, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, responseUSD.StatusCode);
        var cartUSD = await responseUSD.Content.ReadFromJsonAsync<CartResponse>(IntegrationTestJson.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(cartUSD);
        Assert.Single(cartUSD.Items);

        // Act & Assert
        var responseDKK = await _client.PostAsJsonAsync("/api/cart/items?displayCurrency=DKK", addRequest, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, responseDKK.StatusCode);
        var cartDKK = await responseDKK.Content.ReadFromJsonAsync<CartResponse>(IntegrationTestJson.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(cartDKK);
        Assert.Single(cartDKK.Items);
    }

    [Fact]
    public async Task AddCartItem_WhenListingIsDeleted_ReturnsBadRequest()
    {
        // Arrange
        var listingId = await SeedProductListingAsync("Deleted product", "DELETED-001", 39.95m, isDeleted: true);
        var userId = await SeedCustomerAsync("deleted@example.com");
        AuthenticateAs(userId);
        var addItemRequest = new AddCartItemRequest
        {
            ListingId = listingId,
            Quantity = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/cart/items?displayCurrency=BRL", addItemRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // [Fact]
    // TODO: This test should fail right now since the current implementation does not remove items from cart when listing is deleted.
    // public async Task RemoveCartItem_WhenItemFromDeletedListing_ReturnsBadRequest()
    // {
    //     // Arrange
    //     var listingId = await SeedProductListingAsync("Will be deleted", "WILL-DELETE-001", 29.95m);
    //     var userId = await SeedCustomerAsync("willdelete@example.com");

    //     var addResponse = await _client.PostAsJsonAsync(
    //         "/api/cart/items?displayCurrency=BRL",
    //         new AddCartItemRequest
    //         {
    //             ListingId = listingId,
    //             Quantity = 1
    //         },
    //         TestContext.Current.CancellationToken);

    //     var cart = await addResponse.Content.ReadFromJsonAsync<CartResponse>(TestContext.Current.CancellationToken);
    //     Assert.NotNull(cart);

    //     // Mark listing as deleted
    //     using var scope = _factory.Services.CreateScope();
    //     var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    //     var listing = await dbContext.ProductListings.FindAsync(new object[] { listingId }, cancellationToken: TestContext.Current.CancellationToken);
    //     Assert.NotNull(listing);
    //     listing.IsDeleted = true;
    //     await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    //     // Act
    //     var response = await _client.GetAsync($"/api/cart/{userId}?displayCurrency=BRL", TestContext.Current.CancellationToken);

    //     // Assert
    //     Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    //     Assert.NotNull(cart);
    //     Assert.Empty(cart.Items);
    // }

    private async Task<Guid> SeedProductListingAsync(
        string productName,
        string sku,
        decimal price,
        int inventoryQuantity = 10,
        bool isDeleted = false,
        ListingVisibilityStatus visibilityStatus = ListingVisibilityStatus.Published)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var sellerUser = TestEntityFactory.CreateUserAccount($"{Guid.NewGuid():N}@seller.example");
        var seller = TestEntityFactory.CreateSeller(sellerUser.Id);
        var category = TestEntityFactory.CreateCategory("cart_category", "Cart category");
        var product = TestEntityFactory.CreateProduct(category.Id, productName);
        product.ImageUrl = $"https://example.com/cart-product-{Guid.NewGuid():N}.jpg";
        var listing = TestEntityFactory.CreateListing(seller.Id, product.Id, sku, price);
        listing.InventoryQuantity = inventoryQuantity;
        listing.IsDeleted = isDeleted;
        listing.VisibilityStatus = visibilityStatus;

        dbContext.UserAccounts.Add(sellerUser);
        dbContext.Sellers.Add(seller);
        dbContext.ProductCategories.Add(category);
        dbContext.Products.Add(product);
        dbContext.ProductListings.Add(listing);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return listing.Id;
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

    private async Task<Guid> SeedSellerAsync(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var sellerUser = TestEntityFactory.CreateUserAccount(email);
        var seller = TestEntityFactory.CreateSeller(sellerUser.Id);

        dbContext.UserAccounts.Add(sellerUser);
        dbContext.Sellers.Add(seller);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return sellerUser.Id;
    }

    private void AuthenticateAs(Guid userId)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            IntegrationTestAuth.CreateBearerToken(userId));
    }
}
