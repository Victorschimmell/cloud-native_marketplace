using System.Net;
using System.Net.Http.Json;
using Backend.Api;
using Backend.Api.Contracts.Commerce.Checkout;
using Backend.Api.Contracts.Commerce.Payments;

namespace Backend.IntegrationTests;

[Collection("PostgresDocker")]
public class CheckoutEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;

    public CheckoutEndpointsTests(MarketplaceApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PreviewCheckout_ReturnsNotFound()
    {
        // Arrange
        var previewRequest = new CheckoutPreviewRequest
        {
            UserId = Guid.NewGuid()
        };

        // Act
        var response = await _client.GetAsync($"/api/checkout/preview?userId={previewRequest.UserId}&currency=USD", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
