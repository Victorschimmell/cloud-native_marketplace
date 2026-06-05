using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Backend.Api;
using Backend.Api.Contracts.User.SellerVerification;
using Backend.Infrastructure.Persistence;
using Backend.IntegrationTests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DomainEnums = Backend.Domain.Enums;
using DomainSellerVerificationRequest = Backend.Domain.Entities.IdentityAccess.SellerVerificationRequest;

namespace Backend.IntegrationTests;

[Collection("PostgresDocker")]
public class SellerVerificationEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;
    private readonly MarketplaceApiFactory _factory;

    public SellerVerificationEndpointsTests(MarketplaceApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SubmitSellerVerification_WhenRequestIsValid_CreatesSubmission()
    {
        // Arrange
        var sellerId = await SeedSellerAsync("seller-submit@example.com", DomainEnums.VerificationStatus.Unverified);
        var actorId = await SeedUserAsync("seller-submit-actor@example.com");
        AuthenticateAs(actorId);
        var submitRequest = new SellerVerificationRequest
        {
            SubmittedDetails = "Verification Details"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/sellers/{sellerId}/verifications", submitRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var dbSeller = await dbContext.Sellers.SingleAsync(s => s.Id == sellerId, TestContext.Current.CancellationToken);
        Assert.Equal(DomainEnums.VerificationStatus.Pending, dbSeller.VerificationStatus);

        var dbRequest = await dbContext.SellerVerificationRequests.SingleAsync(r => r.SellerId == sellerId, TestContext.Current.CancellationToken);
        Assert.Equal(DomainEnums.SellerVerificationRequestStatus.Submitted, dbRequest.Status);
        Assert.Equal("Verification Details", dbRequest.SubmittedDetails);
    }

    [Fact]
    public async Task GetSellerVerifications_WhenRequestsExist_ReturnsSellerRequests()
    {
        // Arrange
        var sellerId = await SeedSellerAsync("seller-get@example.com", DomainEnums.VerificationStatus.Pending);
        var requestId = await SeedVerificationRequestAsync(sellerId, "First submission");
        var actorId = await SeedUserAsync("seller-get-actor@example.com");
        AuthenticateAs(actorId);

        // Act
        var response = await _client.GetAsync($"/api/sellers/{sellerId}/verifications", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<IReadOnlyList<SellerVerificationRequestDetailsResponse>>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);
        Assert.NotNull(items);
        Assert.Contains(items, r => r.Id == requestId && r.SubmittedDetails == "First submission");
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private async Task<Guid> SeedUserAsync(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = TestEntityFactory.CreateUserAccount(email);
        dbContext.UserAccounts.Add(user);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return user.Id;
    }

    private async Task<Guid> SeedSellerAsync(string email, DomainEnums.VerificationStatus verificationStatus)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = TestEntityFactory.CreateUserAccount(email);
        var seller = TestEntityFactory.CreatePendingSeller(user.Id);
        seller.VerificationStatus = verificationStatus;

        dbContext.UserAccounts.Add(user);
        dbContext.Sellers.Add(seller);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return seller.Id;
    }

    private async Task<Guid> SeedVerificationRequestAsync(Guid sellerId, string details)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var seller = await dbContext.Sellers.SingleAsync(s => s.Id == sellerId, TestContext.Current.CancellationToken);
        var request = new DomainSellerVerificationRequest
        {
            SellerId = sellerId,
            SubmittedAtUtc = DateTimeOffset.UtcNow,
            Status = DomainEnums.SellerVerificationRequestStatus.Submitted,
            BusinessNameSnapshot = seller.BusinessName,
            RegistrationNumberSnapshot = seller.RegistrationNumber,
            SubmittedDetails = details
        };

        dbContext.SellerVerificationRequests.Add(request);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return request.Id;
    }

    private void AuthenticateAs(Guid userId)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            IntegrationTestAuth.CreateBearerToken(userId, isAdmin: false));
    }
}
