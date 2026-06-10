using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Backend.Api.Contracts.Common;
using Backend.Api.Contracts.Operation.Admin;
using Backend.Api.Contracts.User.SellerVerification;
using Backend.Infrastructure.Persistence;
using Backend.IntegrationTests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DomainEnums = Backend.Domain.Enums;
using DomainSellerVerificationRequest = Backend.Domain.Entities.IdentityAccess.SellerVerificationRequest;

namespace Backend.IntegrationTests;

[Collection("PostgresDocker")]
public class AdminEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;
    private readonly MarketplaceApiFactory _factory;

    public AdminEndpointsTests(MarketplaceApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task BlockUser_WhenAdminIsAuthenticated_BlocksUser()
    {
        // Arrange
        var adminId = await SeedUserAsync("admin-block-admin@example.com", isAdmin: true);
        var targetId = await SeedUserAsync("admin-block-target@example.com", isAdmin: false);
        AuthenticateAs(adminId, isAdmin: true);
        var blockRequest = new AdminBlockUserRequest { Reason = "violation" };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/admin/users/{targetId}/block", blockRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var blocked = await dbContext.UserAccounts.SingleAsync(u => u.Id == targetId, TestContext.Current.CancellationToken);
        Assert.True(blocked.IsBlocked);
        Assert.Equal(DomainEnums.AccountStatus.Suspended, blocked.AccountStatus);

        var auditEntry = await dbContext.AuditLogs.SingleAsync(
            a => a.TargetEntityType == "UserAccount"
                && a.TargetEntityId == targetId.ToString()
                && a.ActionType == DomainEnums.AuditActionType.Block
                && a.Outcome == DomainEnums.AuditOutcome.Succeeded,
            TestContext.Current.CancellationToken);
        Assert.Equal(adminId, auditEntry.ActorUserId);
        Assert.Contains("violation", auditEntry.Details);
    }

    [Fact]
    public async Task UnblockUser_WhenAdminIsAuthenticated_UnblocksUser()
    {
        // Arrange
        var adminId = await SeedUserAsync("admin-unblock-admin@example.com", isAdmin: true);
        var targetId = await SeedUserAsync("admin-unblock-target@example.com", isAdmin: false, isBlocked: true);
        AuthenticateAs(adminId, isAdmin: true);
        var unblockRequest = new AdminUnblockUserRequest { Reason = "appeal granted" };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/admin/users/{targetId}/unblock", unblockRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await dbContext.UserAccounts.SingleAsync(u => u.Id == targetId, TestContext.Current.CancellationToken);
        Assert.False(user.IsBlocked);
        Assert.Equal(DomainEnums.AccountStatus.Active, user.AccountStatus);

        var auditEntry = await dbContext.AuditLogs.SingleAsync(
            a => a.TargetEntityType == "UserAccount"
                && a.TargetEntityId == targetId.ToString()
                && a.ActionType == DomainEnums.AuditActionType.Unblock
                && a.Outcome == DomainEnums.AuditOutcome.Succeeded,
            TestContext.Current.CancellationToken);
        Assert.Equal(adminId, auditEntry.ActorUserId);
        Assert.Contains("appeal granted", auditEntry.Details);
    }

    [Fact]
    public async Task GetSellerVerificationRequests_WhenAdminIsAuthenticated_ReturnsRequests()
    {
        // Arrange
        var adminId = await SeedUserAsync("admin-list-verifications@example.com", isAdmin: true);
        var (sellerId, requestId) = await SeedSellerWithVerificationAsync("seller-list@example.com");
        AuthenticateAs(adminId, isAdmin: true);

        // Act
        var response = await _client.GetAsync("/api/admin/sellers/verifications?page=1&pageSize=50", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var page = await response.Content.ReadFromJsonAsync<PageResponse<SellerVerificationRequestDetailsResponse>>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);
        Assert.NotNull(page);
        Assert.Contains(page.Items, r => r.Id == requestId && r.SellerId == sellerId);
    }

    [Fact]
    public async Task VerifySeller_WhenAdminApprovesRequest_VerifiesSeller()
    {
        // Arrange
        var adminId = await SeedUserAsync("admin-verify@example.com", isAdmin: true);
        var (sellerId, requestId) = await SeedSellerWithVerificationAsync("seller-verify@example.com");
        AuthenticateAs(adminId, isAdmin: true);
        var verifyRequest = new VerifySellerRequest
        {
            VerificationRequestId = requestId,
            Approve = true,
            ReviewNotes = "Documents check out"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/admin/sellers/{sellerId}/verify", verifyRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var dbSeller = await dbContext.Sellers.SingleAsync(s => s.Id == sellerId, TestContext.Current.CancellationToken);
        Assert.Equal(DomainEnums.VerificationStatus.Verified, dbSeller.VerificationStatus);
        Assert.NotNull(dbSeller.VerifiedAtUtc);

        var dbRequest = await dbContext.SellerVerificationRequests.SingleAsync(r => r.Id == requestId, TestContext.Current.CancellationToken);
        Assert.Equal(DomainEnums.SellerVerificationRequestStatus.Approved, dbRequest.Status);
        Assert.Equal(adminId, dbRequest.ReviewedByUserId);

        var auditEntry = await dbContext.AuditLogs.SingleAsync(
            a => a.TargetEntityType == "SellerVerificationRequest"
                && a.TargetEntityId == requestId.ToString()
                && a.ActionType == DomainEnums.AuditActionType.Approve
                && a.Outcome == DomainEnums.AuditOutcome.Succeeded,
            TestContext.Current.CancellationToken);
        Assert.Equal(adminId, auditEntry.ActorUserId);
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private async Task<Guid> SeedUserAsync(string email, bool isAdmin, bool isBlocked = false)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = TestEntityFactory.CreateUserAccount(email);
        user.IsAdmin = isAdmin;
        user.IsBlocked = isBlocked;

        dbContext.UserAccounts.Add(user);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return user.Id;
    }

    private async Task<(Guid SellerId, Guid RequestId)> SeedSellerWithVerificationAsync(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = TestEntityFactory.CreateUserAccount(email);
        var seller = TestEntityFactory.CreatePendingSeller(user.Id);
        var request = new DomainSellerVerificationRequest
        {
            SellerId = seller.Id,
            SubmittedAtUtc = DateTimeOffset.UtcNow,
            Status = DomainEnums.SellerVerificationRequestStatus.Submitted,
            BusinessNameSnapshot = seller.BusinessName,
            RegistrationNumberSnapshot = seller.RegistrationNumber,
            SubmittedDetails = "Seed"
        };

        dbContext.UserAccounts.Add(user);
        dbContext.Sellers.Add(seller);
        dbContext.SellerVerificationRequests.Add(request);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return (seller.Id, request.Id);
    }

    private void AuthenticateAs(Guid userId, bool isAdmin)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            IntegrationTestAuth.CreateBearerToken(userId, isAdmin));
    }
}
