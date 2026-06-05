using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Backend.Api;
using Backend.Api.Contracts.Catalog.Products;
using Backend.Api.Contracts.Common;
using Backend.Domain.Entities.Orders;
using Backend.Infrastructure.Persistence;
using Backend.IntegrationTests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using DomainVerificationStatus = Backend.Domain.Enums.VerificationStatus;

namespace Backend.IntegrationTests;

[Collection("PostgresDocker")]
public class ProductsEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;
    private readonly MarketplaceApiFactory _factory;

    public ProductsEndpointsTests(MarketplaceApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
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
        var response = await _client.PostAsJsonAsync("/api/products", createRequest, TestContext.Current.CancellationToken);

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
        var response = await _client.PutAsJsonAsync($"/api/products/listings/{productId}", updateRequest, TestContext.Current.CancellationToken);

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
            $"/api/products/listings/{seed.SellerListingId}",
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
            $"/api/products/listings/{seed.SellerListingId}",
            updateRequest,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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
