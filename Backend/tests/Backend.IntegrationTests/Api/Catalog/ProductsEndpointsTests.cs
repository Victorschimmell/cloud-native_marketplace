using System.Net;
using System.Net.Http.Json;
using Backend.Api;
using Backend.Api.Contracts.Catalog.Products;
using Backend.Api.Contracts.Common;
using Backend.Infrastructure.Persistence;
using Backend.IntegrationTests.TestSupport;
using Microsoft.Extensions.DependencyInjection;

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
        // Act
        var response = await _client.GetAsync("/api/products", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var products = await response.Content.ReadFromJsonAsync<PageResponse<BrowseProductResponse>>(TestContext.Current.CancellationToken);
        Assert.NotNull(products);
        Assert.Equal(1, products.Page);
        Assert.Equal(20, products.PageSize);
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

        var product = await response.Content.ReadFromJsonAsync<ProductDetailsResponse>(TestContext.Current.CancellationToken);
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

        var product = await response.Content.ReadFromJsonAsync<ProductDetailsResponse>(TestContext.Current.CancellationToken);
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
    public async Task CreateProduct_ReturnsNotImplemented()
    {
        // Arrange
        var createRequest = new CreateProductRequest
        {
            ProductName = "Test Product",
            CategoryId = Guid.NewGuid(),
            Description = "Test Description",
            ProductPhotosQty = 1,
            ProductWeightG = 100,
            ProductLengthCm = 10,
            ProductHeightCm = 10,
            ProductWidthCm = 10
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", createRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProduct_ReturnsNotImplemented()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var updateRequest = new UpdateProductRequest
        {
            ProductName = "Updated Product",
            CategoryId = Guid.NewGuid(),
            Description = "Updated Description",
            ProductPhotosQty = 1,
            ProductWeightG = 100,
            ProductLengthCm = 10,
            ProductHeightCm = 10,
            ProductWidthCm = 10
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/products/{productId}", updateRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_ReturnsNotImplemented()
    {
        // Act
        var productId = Guid.NewGuid();
        var response = await _client.DeleteAsync($"/api/products/{productId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task GetProductReviews_ReturnsNotImplemented()
    {
        // Act
        var productId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/products/{productId}/reviews", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    private async Task<(Guid ProductId, Guid ListingId)> SeedProductListingAsync(string productName, string sku, decimal price)
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var sellerUser = TestEntityFactory.CreateUserAccount($"{Guid.NewGuid():N}@seller.example");
        var seller = TestEntityFactory.CreateSeller(sellerUser.Id);
        var category = TestEntityFactory.CreateCategory("test_category", "Test category");
        var product = TestEntityFactory.CreateProduct(category.Id, productName);
        var listing = TestEntityFactory.CreateListing(seller.Id, product.Id, sku, price);

        dbContext.UserAccounts.Add(sellerUser);
        dbContext.Sellers.Add(seller);
        dbContext.ProductCategories.Add(category);
        dbContext.Products.Add(product);
        dbContext.ProductListings.Add(listing);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return (product.Id, listing.Id);
    }
}
