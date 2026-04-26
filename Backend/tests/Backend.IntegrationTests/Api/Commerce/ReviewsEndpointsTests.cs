using System.Net;
using System.Net.Http.Json;
using Backend.Api;
using Backend.Api.Contracts.Commerce.Reviews;

namespace Backend.IntegrationTests;

[Collection("PostgresDocker")]
public class ReviewsEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;

    public ReviewsEndpointsTests(MarketplaceApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RecordReview_ReturnsNotImplemented()
    {
        // Arrange
        var recordRequest = new RecordReviewRequest
        {
            OrderId = Guid.NewGuid(),
            ReviewScore = 5
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/reviews", recordRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }
}
