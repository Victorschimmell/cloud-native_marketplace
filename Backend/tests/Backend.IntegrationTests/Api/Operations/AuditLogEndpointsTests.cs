using System.Net;
using System.Net.Http.Json;
using Backend.Api;
using Backend.Api.Contracts.Operation.AuditLog;

namespace Backend.IntegrationTests;

public class AuditLogEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;

    public AuditLogEndpointsTests(MarketplaceApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAuditLogsByActor_ReturnsNotImplemented()
    {
        // Act
        var actorUserId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/admin/audit-logs/{actorUserId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task GetAuditLogsByEntity_ReturnsNotImplemented()
    {
        // Act
        var entityType = "Type";
        var entityId = "Id";
        var response = await _client.GetAsync($"/api/admin/audit-logs/{entityType}/{entityId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task WriteAuditLog_ReturnsNotImplemented()
    {
        // Arrange
        var writeRequest = new WriteAuditLogEntryRequest
        {
            ActionType = AuditActionType.Created,
            TargetEntityType = "User",
            TargetEntityId = Guid.NewGuid().ToString(),
            Outcome = AuditOutcome.Succeeded,
            Details = "Audit log details"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/audit-logs", writeRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }
}
