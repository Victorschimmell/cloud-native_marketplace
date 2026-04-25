using System.Net;
using System.Net.Http.Json;
using Backend.Api;
using Backend.Api.Contracts.Operation.Admin;
using Backend.Api.Contracts.User.SellerVerification;

namespace Backend.IntegrationTests;

public class AdminEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;

    public AdminEndpointsTests(MarketplaceApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task BlockUser_ReturnsNotImplemented()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var blockRequest = new AdminBlockUserRequest();

        // Act
        var response = await _client.PostAsJsonAsync($"/api/admin/users/{userId}/block", blockRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task UnblockUser_ReturnsNotImplemented()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var unblockRequest = new AdminUnblockUserRequest();

        // Act
        var response = await _client.PostAsJsonAsync($"/api/admin/users/{userId}/unblock", unblockRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task GetAuditLogs_ReturnsNotImplemented()
    {
        // Arrange
        var getLogsRequest = new GetAuditLogsRequest
        {
            ActorUserId = Guid.NewGuid()
        };

        // Act
        var response = await _client.GetAsync($"/api/admin/audit-logs?actorUserId={getLogsRequest.ActorUserId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task GetSellerVerificationRequests_ReturnsNotImplemented()
    {
        // Act
        var response = await _client.GetAsync("/api/admin/sellers/verifications", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task VerifySeller_ReturnsNotImplemented()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var verifyRequest = new VerifySellerRequest
        {
            VerificationRequestId = Guid.NewGuid(),
            Approve = true
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/admin/sellers/{sellerId}/verify", verifyRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }
}
