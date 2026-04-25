using System.Net;
using System.Net.Http.Json;
using Backend.Api;
using Backend.Api.Contracts.Commerce.Shipments;

namespace Backend.IntegrationTests;

[Collection("PostgresDocker")]
public class ShipmentsEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;

    public ShipmentsEndpointsTests(MarketplaceApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UpdateShipmentStatus_ReturnsNotImplemented()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var updateRequest = new UpdateShipmentStatusRequest
        {
            Status = ShipmentStatus.ReadyForPickup
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/shipments/{shipmentId}/status", updateRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }
}
