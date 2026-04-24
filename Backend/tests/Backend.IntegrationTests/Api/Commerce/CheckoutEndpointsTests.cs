using System.Net;
using System.Net.Http.Json;
using Backend.Api;
using Backend.Api.Contracts.Commerce.Checkout;
using Backend.Api.Contracts.Commerce.Payments;

namespace Backend.IntegrationTests;

public class CheckoutEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;

    public CheckoutEndpointsTests(MarketplaceApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Checkout_ReturnsNotImplemented()
    {
        // Arrange
        var checkoutRequest = new CheckoutRequest
        {
            UserId = Guid.NewGuid(),
            ShippingAddressId = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            Payments = new List<RecordPaymentRequest>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/checkout", checkoutRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task PreviewCheckout_ReturnsNotImplemented()
    {
        // Arrange
        var previewRequest = new CheckoutPreviewRequest
        {
            UserId = Guid.NewGuid()
        };

        // Act
        var response = await _client.GetAsync($"/api/checkout/preview?userId={previewRequest.UserId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }
}
