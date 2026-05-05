using System.Net;
using System.Net.Http.Headers;
using Backend.Infrastructure.Persistence;
using Backend.IntegrationTests.TestSupport;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.IntegrationTests;

[Collection("PostgresDocker")]
public class AuditLogEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;
    private readonly MarketplaceApiFactory _factory;

    public AuditLogEndpointsTests(MarketplaceApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAuditLogsByActor_ReturnsOK()
    {
        // Arrange
        var adminUserId = await SeedAdminAsync("auditlog-all-admin@example.com");
        AuthenticateAs(adminUserId, isAdmin: true);

        // Act
        var response = await _client.GetAsync($"/api/admin/audit-logs", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAuditLogsByEntity_ReturnsOK()
    {
        var adminUserId = await SeedAdminAsync("auditlog-entity-admin@example.com");
        AuthenticateAs(adminUserId, isAdmin: true);

        // Act
        var entityType = "Type";
        var entityId = "Id";
        var response = await _client.GetAsync($"/api/admin/audit-logs?entityType={entityType}&entityId={entityId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAuditLogsByActorUserId_ReturnsOK()
    {
        // Arrange
        var adminUserId = await SeedAdminAsync("auditlog-actor-user-id-admin@example.com");
        AuthenticateAs(adminUserId, isAdmin: true);


        // Act
        var response = await _client.GetAsync($"/api/admin/audit-logs?actorUserId={adminUserId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAuditLogs_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/admin/audit-logs", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAuditLogs_WhenNonAdmin_ReturnsForbidden()
    {
        // Arrange
        var userId = await SeedCustomerAsync("auditlog-nonadmin@example.com");
        AuthenticateAs(userId, isAdmin: false);

        // Act
        var response = await _client.GetAsync("/api/admin/audit-logs", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAuditLogs_ProvidesEntityIdButNotEntityType_ReturnsBadRequest()
    {
        // Arrange
        var adminUserId = await SeedAdminAsync("auditlog-entity-id-only-admin@example.com");
        AuthenticateAs(adminUserId, isAdmin: true);

        // Act
        var entityId = "Id";
        var response = await _client.GetAsync($"/api/admin/audit-logs?entityId={entityId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<Guid> SeedCustomerAsync(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var customerUser = TestEntityFactory.CreateUserAccount(email);
        var customer = TestEntityFactory.CreateCustomer(customerUser.Id);

        dbContext.UserAccounts.Add(customerUser);
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return customerUser.Id;
    }

    private async Task<Guid> SeedAdminAsync(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var adminUser = TestEntityFactory.CreateUserAccount(email);
        adminUser.IsAdmin = true;

        dbContext.UserAccounts.Add(adminUser);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return adminUser.Id;
    }

    private void AuthenticateAs(Guid userId, bool isAdmin = false)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            IntegrationTestAuth.CreateBearerToken(userId, isAdmin));
    }
}
