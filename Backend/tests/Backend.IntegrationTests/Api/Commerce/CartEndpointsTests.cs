using System.Net;
using System.Net.Http.Json;
using Backend.Api;
using Backend.Api.Contracts.Commerce.Cart;

namespace Backend.IntegrationTests;

[Collection("PostgresDocker")]
public class CartEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;

    public CartEndpointsTests(MarketplaceApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AddCartItem_ReturnsNotImplemented()
    {
        // Arrange
        var addItemRequest = new AddCartItemRequest
        {
            UserId = Guid.NewGuid(),
            ListingId = Guid.NewGuid(),
            Quantity = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/cart", addItemRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
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
}
