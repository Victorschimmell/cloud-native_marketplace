using System.Net;
using System.Net.Http.Json;
using Backend.Api;
using Backend.Api.Contracts.User.SellerVerification;

namespace Backend.IntegrationTests;

public class SellerVerificationEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;

    public SellerVerificationEndpointsTests(MarketplaceApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SubmitSellerVerification_ReturnsNotImplemented()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var submitRequest = new SellerVerificationRequest
        {
            SubmittedDetails = "Verification Details"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/sellers/{sellerId}/verifications", submitRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task GetSellerVerifications_ReturnsNotImplemented()
    {
        // Act
        var sellerId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/sellers/{sellerId}/verifications");

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }
}
