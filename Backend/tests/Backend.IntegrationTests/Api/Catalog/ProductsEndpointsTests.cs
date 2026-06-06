using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Backend.Api;
using Backend.Api.Contracts.Catalog.Products;
using Backend.Api.Contracts.Common;
using Backend.Domain.Entities.Orders;
using Backend.Domain.Enums;
using Backend.Infrastructure.Persistence;
using Backend.IntegrationTests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DomainVerificationStatus = Backend.Domain.Enums.VerificationStatus;

namespace Backend.IntegrationTests;

[Collection("PostgresDocker")]
public class ProductsEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;
    private readonly MarketplaceApiFactory _factory;
    private readonly ITestOutputHelper _output;

    public ProductsEndpointsTests(MarketplaceApiFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _output = output;
    }

    [Fact]
    public async Task GetProducts_ReturnsBrowseProductsPage()
    {
        // Arrange
        var productName = $"Browse review summary {Guid.NewGuid():N}";
        await SeedProductListingAsync(productName, $"BROWSE-{Guid.NewGuid():N}", 149.99m, [4, 5]);

        // Act
        var response = await _client.GetAsync($"/api/products?search={Uri.EscapeDataString(productName)}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var products = await response.Content.ReadFromJsonAsync<PageResponse<BrowseProductResponse>>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);
        Assert.NotNull(products);
        Assert.Equal(1, products.Page);
        Assert.Equal(20, products.PageSize);

        var product = Assert.Single(products.Items);
        Assert.Equal(productName, product.ProductName);
        Assert.Equal(2, product.ReviewCount);
        Assert.Equal(4.5, product.AverageReviewScore);
    }

    [Fact]
    public async Task GetProductById_ReturnsProductDetails()
    {
        // Arrange
        var (productId, listingId) = await SeedProductListingAsync("Product details test", "DETAIL-001", 149.99m);

        // Act
        var response = await _client.GetAsync($"/api/products/{productId}?listingId={listingId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var product = await response.Content.ReadFromJsonAsync<ProductDetailsResponse>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);
        Assert.NotNull(product);
        Assert.Equal(productId, product.ProductId);
        Assert.Equal(listingId, product.ListingId);
        Assert.Equal(149.99m, product.Price);
        Assert.Equal("BRL", product.CurrencyCode);
        Assert.Equal(10, product.StockQuantity);
        Assert.Equal("MarketplaceTraders", product.SellerName);
        Assert.Equal("Pending", product.SellerVerificationStatus);
    }

    [Fact]
    public async Task GetProductById_WithCurrency_ReturnsConvertedProductDetails()
    {
        // Arrange
        var (productId, listingId) = await SeedProductListingAsync("Converted product details test", "DETAIL-USD-001", 100m);

        // Act
        var response = await _client.GetAsync($"/api/products/{productId}?listingId={listingId}&currency=USD", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var product = await response.Content.ReadFromJsonAsync<ProductDetailsResponse>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);
        Assert.NotNull(product);
        Assert.Equal("USD", product.CurrencyCode);
        Assert.Equal(18m, product.Price);
    }

    [Theory]
    [InlineData("BRL", "BRL", 100, 100)]
    [InlineData("BRL", "DKK", 100, 117)]
    [InlineData("BRL", "USD", 100, 18)]
    [InlineData("USD", "BRL", 100, 555.56)]
    [InlineData("USD", "DKK", 100, 650)]
    [InlineData("USD", "USD", 100, 100)]
    [InlineData("DKK", "BRL", 100, 85.47)]
    [InlineData("DKK", "DKK", 100, 100)]
    [InlineData("DKK", "USD", 100, 15.38)]
    public async Task CreateProduct_WhenAccessedInBrl_ReturnsConvertedProductDetails(
        string createCurrency,
        string returnCurrency,
        decimal createPrice,
        decimal expectedBrlPrice)
    {
        // Arrange & Act
        var product = await CreateProductAndFetchDetailsAsync(createCurrency, returnCurrency, createPrice);

        // Assert
        Assert.Equal(returnCurrency, product.CurrencyCode);
        Assert.True(Math.Abs(expectedBrlPrice - product.Price) < 0.02m, $"Expected price to be approximately {expectedBrlPrice} but was {product.Price}.");
    }

    [Fact]
    public async Task CreateProduct_WhenCurrencyIsUnsupported_ReturnsBadRequest()
    {
        // Arrange
        var seed = await SeedVerifiedSellerAndCategoryAsync();
        AuthenticateAs(seed.SellerUserId);

        var createRequest = new CreateProductRequest
        {
            ProductName = $"Created product {Guid.NewGuid():N}",
            CategoryId = seed.CategoryId,
            Description = "Created by integration test.",
            Price = 100m,
            InventoryQuantity = 10,
            ProductPhotosQty = 1,
            ProductWeightG = 100,
            ProductLengthCm = 10,
            ProductHeightCm = 10,
            ProductWidthCm = 10
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/products?currency=INVALID_CODE", createRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetProductById_WhenProductDoesNotExist_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/products/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var createRequest = new CreateProductRequest
        {
            ProductName = "Test Product",
            CategoryId = Guid.NewGuid(),
            Description = "Test Description",
            Price = 9.99m,
            InventoryQuantity = 10,
            ProductPhotosQty = 1,
            ProductWeightG = 100,
            ProductLengthCm = 10,
            ProductHeightCm = 10,
            ProductWidthCm = 10
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products?currency=BRL", createRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProduct_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var updateRequest = new UpdateProductRequest
        {
            ProductName = "Updated Product",
            CategoryId = Guid.NewGuid(),
            Description = "Updated Description",
            Price = 19.99m,
            InventoryQuantity = 10,
            ProductPhotosQty = 1,
            ProductWeightG = 100,
            ProductLengthCm = 10,
            ProductHeightCm = 10,
            ProductWidthCm = 10
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/products/listings/{productId}?currency=BRL", updateRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var productId = Guid.NewGuid();
        var response = await _client.DeleteAsync($"/api/products/listings/{productId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateListing_WhenProductIsShared_ForksProductForSellerListing()
    {
        // Arrange
        var seed = await SeedSharedProductListingAsync();
        AuthenticateAs(seed.SellerUserId);
        var updateRequest = new UpdateProductRequest
        {
            ProductName = "Seller-specific product",
            CategoryId = seed.CategoryId,
            Description = "Updated only for the authenticated seller.",
            ImageUrl = "https://example.com/seller-specific-product.jpg",
            Price = 42m,
            InventoryQuantity = 7,
            VisibilityStatus = "Published"
        };

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/products/listings/{seed.SellerListingId}?currency=BRL",
            updateRequest,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var sellerListing = await dbContext.ProductListings.FindAsync([seed.SellerListingId], TestContext.Current.CancellationToken);
        var otherSellerListing = await dbContext.ProductListings.FindAsync([seed.OtherSellerListingId], TestContext.Current.CancellationToken);
        var originalProduct = await dbContext.Products.FindAsync([seed.SharedProductId], TestContext.Current.CancellationToken);

        Assert.NotNull(sellerListing);
        Assert.NotNull(otherSellerListing);
        Assert.NotNull(originalProduct);
        Assert.NotEqual(seed.SharedProductId, sellerListing.ProductId);
        Assert.Equal(seed.SharedProductId, otherSellerListing.ProductId);
        Assert.Equal("Shared product", originalProduct.ProductName);
        Assert.Equal(42m, sellerListing.ListingPrice);
        Assert.Equal(7, sellerListing.InventoryQuantity);

        var sellerProduct = await dbContext.Products.FindAsync([sellerListing.ProductId], TestContext.Current.CancellationToken);
        Assert.NotNull(sellerProduct);
        Assert.Equal("Seller-specific product", sellerProduct.ProductName);
        Assert.Equal("https://example.com/seller-specific-product.jpg", sellerProduct.ImageUrl);
    }

    [Fact]
    public async Task UpdateListing_WhenCategoryDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var seed = await SeedSharedProductListingAsync();
        AuthenticateAs(seed.SellerUserId);
        var updateRequest = new UpdateProductRequest
        {
            ProductName = "Seller-specific product",
            CategoryId = Guid.NewGuid(),
            Description = "Updated only for the authenticated seller.",
            Price = 42m,
            InventoryQuantity = 7,
            VisibilityStatus = "Published"
        };

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/products/listings/{seed.SellerListingId}?currency=BRL",
            updateRequest,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateListing_WhenPublishedListingIsChangedToDraft_RemovesItFromBrowse()
    {
        var seed = await SeedVerifiedSellerAndCategoryAsync();
        AuthenticateAs(seed.SellerUserId);
        var productName = $"Drafted product {Guid.NewGuid():N}";
        var createRequest = new CreateProductRequest
        {
            ProductName = productName,
            CategoryId = seed.CategoryId,
            Description = "Product that will be published and drafted again.",
            Price = 100m,
            InventoryQuantity = 10
        };

        var createResponse = await _client.PostAsJsonAsync("/api/products?currency=BRL", createRequest, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductResponse>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);
        Assert.NotNull(createdProduct);
        var listingId = await GetListingIdByProductIdAsync(createdProduct.Id);

        var publishedRequest = new UpdateProductRequest
        {
            ProductName = productName,
            CategoryId = seed.CategoryId,
            Description = createRequest.Description,
            Price = 100m,
            InventoryQuantity = 10,
            VisibilityStatus = "Published"
        };
        var publishResponse = await _client.PutAsJsonAsync($"/api/products/listings/{listingId}?currency=BRL", publishedRequest, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, publishResponse.StatusCode);

        var publishedBrowseResponse = await _client.GetAsync($"/api/products?search={Uri.EscapeDataString(productName)}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, publishedBrowseResponse.StatusCode);
        var publishedBrowse = await publishedBrowseResponse.Content.ReadFromJsonAsync<PageResponse<BrowseProductResponse>>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);
        Assert.NotNull(publishedBrowse);
        Assert.Single(publishedBrowse.Items);

        var draftRequest = publishedRequest with { VisibilityStatus = "Draft" };
        var draftResponse = await _client.PutAsJsonAsync($"/api/products/listings/{listingId}?currency=BRL", draftRequest, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, draftResponse.StatusCode);

        var draftBrowseResponse = await _client.GetAsync($"/api/products?search={Uri.EscapeDataString(productName)}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, draftBrowseResponse.StatusCode);
        var draftBrowse = await draftBrowseResponse.Content.ReadFromJsonAsync<PageResponse<BrowseProductResponse>>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);
        Assert.NotNull(draftBrowse);
        Assert.Empty(draftBrowse.Items);
        Assert.Equal(0, draftBrowse.TotalCount);
    }

    [Fact]
    public async Task UpdateListing_WhenCurrencyIsDkk_ReturnsEditedPriceWithoutOneCentDrift()
    {
        var seed = await SeedVerifiedSellerAndCategoryAsync();
        AuthenticateAs(seed.SellerUserId);
        var productName = $"DKK edited product {Guid.NewGuid():N}";
        var createRequest = new CreateProductRequest
        {
            ProductName = productName,
            CategoryId = seed.CategoryId,
            Description = "Product with DKK price edits.",
            Price = 100m,
            InventoryQuantity = 10
        };
        var createResponse = await _client.PostAsJsonAsync("/api/products?currency=DKK", createRequest, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductResponse>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);
        Assert.NotNull(createdProduct);
        var listingId = await GetListingIdByProductIdAsync(createdProduct.Id);

        var updateRequest = new UpdateProductRequest
        {
            ProductName = productName,
            CategoryId = seed.CategoryId,
            Description = createRequest.Description,
            Price = 150m,
            InventoryQuantity = 10,
            VisibilityStatus = "Published"
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/products/listings/{listingId}?currency=DKK", updateRequest, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var listingsResponse = await _client.GetAsync("/api/products/my-listings?currency=DKK", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, listingsResponse.StatusCode);
        var listings = await listingsResponse.Content.ReadFromJsonAsync<IReadOnlyList<SellerListingResponse>>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);
        Assert.NotNull(listings);
        var listing = Assert.Single(listings, item => item.ListingId == listingId);
        Assert.Equal("DKK", listing.CurrencyCode);
        Assert.Equal(150m, listing.ListingPrice);

        var detailsResponse = await _client.GetAsync($"/api/products/{createdProduct.Id}?listingId={listingId}&currency=DKK", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, detailsResponse.StatusCode);
        var details = await detailsResponse.Content.ReadFromJsonAsync<ProductDetailsResponse>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);
        Assert.NotNull(details);
        Assert.Equal(150m, details.Price);
    }

    [Fact]
    public async Task DeleteListing_WhenOwned_DoesNotDeleteSharedProductOrOtherSellerListing()
    {
        // Arrange
        var seed = await SeedSharedProductListingAsync();
        AuthenticateAs(seed.SellerUserId);

        // Act
        var response = await _client.DeleteAsync(
            $"/api/products/listings/{seed.SellerListingId}",
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var sellerListing = await dbContext.ProductListings.FindAsync([seed.SellerListingId], TestContext.Current.CancellationToken);
        var otherSellerListing = await dbContext.ProductListings.FindAsync([seed.OtherSellerListingId], TestContext.Current.CancellationToken);
        var sharedProduct = await dbContext.Products.FindAsync([seed.SharedProductId], TestContext.Current.CancellationToken);

        Assert.NotNull(sellerListing);
        Assert.NotNull(otherSellerListing);
        Assert.NotNull(sharedProduct);
        Assert.True(sellerListing.IsDeleted);
        Assert.False(otherSellerListing.IsDeleted);
        Assert.Equal(seed.SharedProductId, otherSellerListing.ProductId);
    }

    [Fact]
    public async Task GetProductReviews_ReturnsOk()
    {
        // Act
        var productId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/products/{productId}/reviews", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<(Guid ProductId, Guid ListingId)> SeedProductListingAsync(
        string productName,
        string sku,
        decimal price,
        IReadOnlyList<int>? reviewScores = null)
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var sellerUser = TestEntityFactory.CreateUserAccount($"{Guid.NewGuid():N}@seller.example");
        var seller = TestEntityFactory.CreatePendingSeller(sellerUser.Id);
        var category = TestEntityFactory.CreateCategory("test_category", "Test category");
        var product = TestEntityFactory.CreateProduct(category.Id, productName);
        var listing = TestEntityFactory.CreateListing(seller.Id, product.Id, sku, price);

        dbContext.UserAccounts.Add(sellerUser);
        dbContext.Sellers.Add(seller);
        dbContext.ProductCategories.Add(category);
        dbContext.Products.Add(product);
        dbContext.ProductListings.Add(listing);

        if (reviewScores is not null)
        {
            for (var index = 0; index < reviewScores.Count; index++)
            {
                var customerUser = TestEntityFactory.CreateUserAccount($"{Guid.NewGuid():N}@customer.example");
                var customer = TestEntityFactory.CreateCustomer(customerUser.Id);
                var address = TestEntityFactory.CreateAddress();
                var order = TestEntityFactory.CreateOrder(customer.Id, address.Id, $"ORDER-{Guid.NewGuid():N}", DateTimeOffset.UtcNow.AddDays(-index - 1));

                dbContext.UserAccounts.Add(customerUser);
                dbContext.Customers.Add(customer);
                dbContext.Addresses.Add(address);
                dbContext.Orders.Add(order);
                dbContext.OrderReviews.Add(new OrderReview
                {
                    OrderId = order.Id,
                    CustomerId = customer.Id,
                    ProductId = product.Id,
                    ReviewScore = reviewScores[index],
                    ReviewCreationDateUtc = DateTimeOffset.UtcNow.AddDays(-index)
                });
            }
        }

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return (product.Id, listing.Id);
    }

    private async Task<SharedListingSeed> SeedSharedProductListingAsync()
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var sellerUser = TestEntityFactory.CreateUserAccount($"{Guid.NewGuid():N}@seller.example");
        var seller = TestEntityFactory.CreateVerifiedSeller(sellerUser.Id);
        seller.VerificationStatus = DomainVerificationStatus.Verified;
        var otherSellerUser = TestEntityFactory.CreateUserAccount($"{Guid.NewGuid():N}@seller.example");
        var otherSeller = TestEntityFactory.CreateVerifiedSeller(otherSellerUser.Id);
        otherSeller.VerificationStatus = DomainVerificationStatus.Verified;
        var category = TestEntityFactory.CreateCategory("shared_category", "Shared category");
        var product = TestEntityFactory.CreateProduct(category.Id, "Shared product");
        var sellerListing = TestEntityFactory.CreateListing(seller.Id, product.Id, $"SELLER-{Guid.NewGuid():N}", 25m);
        var otherSellerListing = TestEntityFactory.CreateListing(otherSeller.Id, product.Id, $"OTHER-{Guid.NewGuid():N}", 30m);

        dbContext.UserAccounts.AddRange(sellerUser, otherSellerUser);
        dbContext.Sellers.AddRange(seller, otherSeller);
        dbContext.ProductCategories.Add(category);
        dbContext.Products.Add(product);
        dbContext.ProductListings.AddRange(sellerListing, otherSellerListing);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return new SharedListingSeed(
            sellerUser.Id,
            category.Id,
            product.Id,
            sellerListing.Id,
            otherSellerListing.Id);
    }

    private async Task<ProductDetailsResponse> CreateProductAndFetchDetailsAsync(string currency, string return_currency, decimal price)
    {
        var seed = await SeedVerifiedSellerAndCategoryAsync();
        AuthenticateAs(seed.SellerUserId);

        var createRequest = new CreateProductRequest
        {
            ProductName = $"Created product {Guid.NewGuid():N}",
            CategoryId = seed.CategoryId,
            Description = "Created by integration test.",
            Price = price,
            InventoryQuantity = 10,
            ProductPhotosQty = 1,
            ProductWeightG = 100,
            ProductLengthCm = 10,
            ProductHeightCm = 10,
            ProductWidthCm = 10
        };

        var createResponse = await _client.PostAsJsonAsync($"/api/products?currency={currency}", createRequest, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductResponse>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);
        Assert.NotNull(createdProduct);

        await PublishCreatedListingAsync(createdProduct.Id);

        var detailsResponse = await _client.GetAsync($"/api/products/{createdProduct.Id}?currency={return_currency}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, detailsResponse.StatusCode);

        var details = await detailsResponse.Content.ReadFromJsonAsync<ProductDetailsResponse>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);
        Assert.NotNull(details);

        return details;
    }

    private async Task<(Guid SellerUserId, Guid CategoryId)> SeedVerifiedSellerAndCategoryAsync()
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var sellerUser = TestEntityFactory.CreateUserAccount($"{Guid.NewGuid():N}@seller.example");
        var seller = TestEntityFactory.CreateVerifiedSeller(sellerUser.Id);
        var category = TestEntityFactory.CreateCategory("create_category", "Create category");

        dbContext.UserAccounts.Add(sellerUser);
        dbContext.Sellers.Add(seller);
        dbContext.ProductCategories.Add(category);

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return (sellerUser.Id, category.Id);
    }

    private async Task PublishCreatedListingAsync(Guid productId)
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var listing = await dbContext.ProductListings.SingleAsync(l => l.ProductId == productId, TestContext.Current.CancellationToken);

        listing.VisibilityStatus = ListingVisibilityStatus.Published;
        listing.PublishedAtUtc = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private async Task<Guid> GetListingIdByProductIdAsync(Guid productId)
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await dbContext.ProductListings
            .Where(listing => listing.ProductId == productId)
            .Select(listing => listing.Id)
            .SingleAsync(TestContext.Current.CancellationToken);
    }

    private void AuthenticateAs(Guid userId)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            IntegrationTestAuth.CreateBearerToken(userId));
    }

    private sealed record SharedListingSeed(
        Guid SellerUserId,
        Guid CategoryId,
        Guid SharedProductId,
        Guid SellerListingId,
        Guid OtherSellerListingId);
}
