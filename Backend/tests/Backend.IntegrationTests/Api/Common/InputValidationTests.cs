using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Backend.Api.Contracts.Catalog.Products;
using Backend.Api.Contracts.Commerce.Cart;
using Backend.Api.Contracts.Commerce.Reviews;
using Backend.Api.Contracts.User.Registration;

namespace Backend.IntegrationTests;

[Collection("PostgresDocker")]
public class InputValidationTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;

    public InputValidationTests(MarketplaceApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    // Route Constraint Validation Test (GUID format)
    // When GUID format is invalid in route, returns 404 (not found) because route doesn't match
    [Fact]
    public async Task GetOrderById_WithInvalidGuidFormat_Returns404()
    {
        // Arrange
        var invalidGuidFormat = "invalid-guid";

        // Act
        var response = await _client.GetAsync($"/api/orders/{invalidGuidFormat}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // NotEmptyGuid Validation Test
    [Fact]
    public async Task RecordPayment_WithEmptyOrderId_ReturnsBadRequest()
    {
        // Arrange
        var productId = Guid.Empty; // Invalid: empty GUID

        // Act
        var response = await _client.GetAsync($"/api/products/{productId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("productId", content);
    }

    // EmailAddress Validation Test
    // Example: RegisterSellerRequest.Email must be valid email format
    [Fact]
    public async Task RegisterSeller_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var registerRequest = new RegisterSellerRequest
        {
            Email = "not-a-valid-email", // Invalid: not a valid email format
            Password = "Password123!",
            BusinessName = "My Business",
            RegistrationNumber = "+4512345678",
            PayoutInformation = "Bank Account Info"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/registration/seller", registerRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("Email", content);
    }

    // Range Validation Test
    // Example: RecordReviewRequest.ReviewScore must be between 1 and 5
    [Fact]
    public async Task RecordReview_WithReviewScoreBelowRange_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsRegisteredCustomerAsync();
        var recordRequest = new RecordReviewRequest
        {
            OrderId = Guid.NewGuid(),
            OrderItemId = 1,
            ReviewScore = 0 // Invalid: must be between 1 and 5
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/reviews", recordRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("ReviewScore", content);
    }

    // MaxLength/StringLength Validation Test
    // Example: CreateProductRequest.ProductName must not exceed max length
    [Fact]
    public async Task CreateProduct_WithProductNameExceedingMaxLength_ReturnsBadRequest()
    {
        // Arrange
        var productName = new string('a', 501); // Invalid: exceeds MaxLength(500)
        await AuthenticateAsRegisteredCustomerAsync();
        var createRequest = new CreateProductRequest
        {
            ProductName = productName,
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
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("ProductName", content);
    }

    // NotEmptyGuid Validation Test
    // Example: AddCartItemRequest.ListingId must not be empty
    [Fact]
    public async Task AddCartItem_WithEmptyListingId_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsRegisteredCustomerAsync();
        var addItemRequest = new AddCartItemRequest
        {
            ListingId = Guid.Empty,
            Quantity = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/cart/items", addItemRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("ListingId", content);
    }

    private async Task AuthenticateAsRegisteredCustomerAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/registration/customer",
            new RegisterCustomerRequest
            {
                Email = $"{Guid.NewGuid():N}@validation.example.com",
                Password = "Password123!",
                FirstName = "Validation",
                LastName = "Customer",
                Phone = "+4512345678"
            },
            TestContext.Current.CancellationToken);

        var registration = await response.Content.ReadFromJsonAsync<RegistrationResponse>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);

        Assert.NotNull(registration?.Token);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            registration.Token.AccessToken);
    }
}
