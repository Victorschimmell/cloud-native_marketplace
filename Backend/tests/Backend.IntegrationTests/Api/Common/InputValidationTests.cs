using System.Net;
using System.Net.Http.Json;
using Backend.Api;
using Backend.Api.Contracts.Catalog.Products;
using Backend.Api.Contracts.Commerce.Cart;
using Backend.Api.Contracts.Commerce.Payments;
using Backend.Api.Contracts.Commerce.Reviews;
using Backend.Api.Contracts.User.Registration;

namespace Backend.IntegrationTests;

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
        var response = await _client.GetAsync($"/api/orders/{invalidGuidFormat}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // NotEmptyGuid Validation Test
    // Example: RecordPaymentRequest.OrderId must not be empty
    [Fact]
    public async Task RecordPayment_WithEmptyOrderId_ReturnsBadRequest()
    {
        // Arrange
        var recordRequest = new RecordPaymentRequest
        {
            OrderId = Guid.Empty, // Invalid: empty GUID
            CurrencyId = Guid.NewGuid(),
            PaymentType = PaymentType.CreditCard,
            PaymentInstallments = 1,
            PaymentValue = 100m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/payments", recordRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("OrderId", content);
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
        var response = await _client.PostAsJsonAsync("/api/registration/seller", registerRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Email", content);
    }

    // Range Validation Test
    // Example: RecordReviewRequest.ReviewScore must be between 1 and 5
    [Fact]
    public async Task RecordReview_WithReviewScoreBelowRange_ReturnsBadRequest()
    {
        // Arrange
        var recordRequest = new RecordReviewRequest
        {
            OrderId = Guid.NewGuid(),
            ReviewScore = 0 // Invalid: must be between 1 and 5
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/reviews", recordRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("ReviewScore", content);
    }

    // MaxLength/StringLength Validation Test
    // Example: CreateProductRequest.ProductName must not exceed max length
    [Fact]
    public async Task CreateProduct_WithProductNameExceedingMaxLength_ReturnsBadRequest()
    {
        // Arrange
        var productName = new string('a', 501); // Invalid: exceeds MaxLength(500)
        var createRequest = new CreateProductRequest
        {
            ProductName = productName,
            CategoryId = Guid.NewGuid(),
            Description = "Test Description",
            ProductPhotosQty = 1,
            ProductWeightG = 100,
            ProductLengthCm = 10,
            ProductHeightCm = 10,
            ProductWidthCm = 10
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", createRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("ProductName", content);
    }

    // Cross-Field Validation Test (IValidatableObject)
    // Example: AddCartItemRequest requires at least one of CartId, UserId, or SessionId
    [Fact]
    public async Task AddCartItem_WithoutUserIdCartIdOrSessionId_ReturnsBadRequest()
    {
        // Arrange
        var addItemRequest = new AddCartItemRequest
        {
            // All identifier fields are null - invalid
            ListingId = Guid.NewGuid(),
            Quantity = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/cart", addItemRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        // Should indicate that at least one of the three fields is required
        Assert.True(content.Contains("UserId") || content.Contains("CartId") || content.Contains("SessionId"));
    }



    // Valid Data Test
    // Ensure valid data still receives NotImplemented (endpoint exists but not implemented)
    [Fact]
    public async Task RecordPayment_WithValidData_ReturnsNotImplemented()
    {
        // Arrange
        var recordRequest = new RecordPaymentRequest
        {
            OrderId = Guid.NewGuid(),
            CurrencyId = Guid.NewGuid(),
            PaymentType = PaymentType.CreditCard,
            PaymentInstallments = 1,
            PaymentValue = 100m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/payments", recordRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }
}
