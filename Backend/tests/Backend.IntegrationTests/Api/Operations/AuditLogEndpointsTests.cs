using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Backend.Api.Contracts.Common;
using Backend.Api.Contracts.Commerce.Checkout;
using Backend.Api.Contracts.Commerce.Payments;
using Backend.Api.Contracts.Operation.AuditLog;
using Backend.Api.Contracts.User.Auth;
using Backend.Api.Contracts.User.Registration;
using Backend.Domain.Entities.Carts;
using Backend.Domain.Entities.Catalog;
using Backend.Domain.Entities.IdentityAccess;
using Backend.Domain.Entities.Orders;
using Backend.Infrastructure.Persistence;
using Backend.IntegrationTests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using DomainEnums = Backend.Domain.Enums;

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

    [Fact]
    public async Task GetAuditLogs_WithPageLessThanOne_ReturnsBadRequest()
    {
        // Arrange
        var adminUserId = await SeedAdminAsync("auditlog-invalid-page-admin@example.com");
        AuthenticateAs(adminUserId, isAdmin: true);

        // Act
        var response = await _client.GetAsync("/api/admin/audit-logs?page=0&pageSize=10", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAuditLogs_WithPageSizeLessThanOne_ReturnsBadRequest()
    {
        // Arrange
        var adminUserId = await SeedAdminAsync("auditlog-invalid-pagesize-admin@example.com");
        AuthenticateAs(adminUserId, isAdmin: true);

        // Act
        var response = await _client.GetAsync("/api/admin/audit-logs?page=1&pageSize=0", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAuditLogs_WithNegativePage_ReturnsBadRequest()
    {
        // Arrange
        var adminUserId = await SeedAdminAsync("auditlog-negative-page-admin@example.com");
        AuthenticateAs(adminUserId, isAdmin: true);

        // Act
        var response = await _client.GetAsync("/api/admin/audit-logs?page=-1&pageSize=10", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAuditLogs_FilterByActorUserId_ReturnsOnlyLogsFromSpecificActor()
    {
        // Arrange
        var admin1Id = await SeedAdminAsync("auditlog-filter-admin1@example.com");
        var admin2Id = await SeedAdminAsync("auditlog-filter-admin2@example.com");
        var entityType = "FilterActorUserId";

        // Seed audit logs from different actors
        await SeedAuditLogAsync(actorUserId: admin1Id, targetEntityType: entityType, targetEntityId: "entity-1");
        await SeedAuditLogAsync(actorUserId: admin1Id, targetEntityType: entityType, targetEntityId: "entity-2");
        await SeedAuditLogAsync(actorUserId: admin2Id, targetEntityType: entityType, targetEntityId: "entity-3");

        AuthenticateAs(admin1Id, isAdmin: true);

        // Act
        var response = await _client.GetAsync($"/api/admin/audit-logs?actorUserId={admin1Id}&entityType={entityType}", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadFromJsonAsync<PageResponse<AuditLogEntryResponse>>(IntegrationTestJson.Options, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(content);
        Assert.Equal(2, content.Items.Count);
        Assert.All(content.Items, item => Assert.Equal(admin1Id, item.ActorUserId));
    }

    [Fact]
    public async Task GetAuditLogs_FilterByTargetEntity_ReturnsOnlyLogsForSpecificEntity()
    {
        // Arrange
        var adminUserId = await SeedAdminAsync("auditlog-filter-entity-admin@example.com");
        var entityType = "FilterTargetEntity";

        // Seed audit logs for different entities
        await SeedAuditLogAsync(actorUserId: adminUserId, targetEntityType: entityType, targetEntityId: "prod-1");
        await SeedAuditLogAsync(actorUserId: adminUserId, targetEntityType: entityType, targetEntityId: "prod-2");
        await SeedAuditLogAsync(actorUserId: adminUserId, targetEntityType: "FilterTargetEntityOther", targetEntityId: "order-1");

        AuthenticateAs(adminUserId, isAdmin: true);

        // Act
        var response = await _client.GetAsync($"/api/admin/audit-logs?entityType={entityType}", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadFromJsonAsync<PageResponse<AuditLogEntryResponse>>(IntegrationTestJson.Options, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(content);
        Assert.Equal(2, content.Items.Count);
        Assert.All(content.Items, item => Assert.Equal(entityType, item.TargetEntityType));
    }

    [Fact]
    public async Task GetAuditLogs_FilterByTargetEntityAndId_ReturnsOnlySpecificEntityLogs()
    {
        // Arrange
        var adminUserId = await SeedAdminAsync("auditlog-filter-entity-id-admin@example.com");
        var entityType = "FilterTargetEntityAndId";

        // Seed audit logs
        await SeedAuditLogAsync(actorUserId: adminUserId, targetEntityType: entityType, targetEntityId: "prod-1");
        await SeedAuditLogAsync(actorUserId: adminUserId, targetEntityType: entityType, targetEntityId: "prod-1");
        await SeedAuditLogAsync(actorUserId: adminUserId, targetEntityType: entityType, targetEntityId: "prod-2");

        AuthenticateAs(adminUserId, isAdmin: true);

        // Act
        var response = await _client.GetAsync($"/api/admin/audit-logs?entityType={entityType}&entityId=prod-1", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadFromJsonAsync<PageResponse<AuditLogEntryResponse>>(IntegrationTestJson.Options, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(content);
        Assert.Equal(2, content.Items.Count);
        Assert.All(content.Items, item =>
        {
            Assert.Equal(entityType, item.TargetEntityType);
            Assert.Equal("prod-1", item.TargetEntityId);
        });
    }

    [Fact]
    public async Task GetAuditLogs_WithPaginationParameters_ReturnsPaginatedResults()
    {
        // Arrange
        var adminUserId = await SeedAdminAsync("auditlog-pagination-admin@example.com");
        var entityType = "Pagination";

        // Seed 5 audit logs
        for (int i = 1; i <= 5; i++)
        {
            await SeedAuditLogAsync(actorUserId: adminUserId, targetEntityType: entityType, targetEntityId: $"entity-{i}");
        }

        AuthenticateAs(adminUserId, isAdmin: true);

        // Act - Get page 2 with page size 2
        var response = await _client.GetAsync($"/api/admin/audit-logs?entityType={entityType}&page=2&pageSize=2", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadFromJsonAsync<PageResponse<AuditLogEntryResponse>>(IntegrationTestJson.Options, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(content);
        Assert.Equal(2, content.Items.Count);
        Assert.Equal(2, content.Page);
        Assert.Equal(2, content.PageSize);
        Assert.Equal(5, content.TotalCount);
    }

    [Fact]
    public async Task GetAuditLogs_FirstPage_ReturnsFirstSetOfResults()
    {
        // Arrange
        var adminUserId = await SeedAdminAsync("auditlog-first-page-admin@example.com");
        var entityType = "FirstPage";

        // Seed 7 audit logs
        for (int i = 1; i <= 7; i++)
        {
            await SeedAuditLogAsync(actorUserId: adminUserId, targetEntityType: entityType, targetEntityId: $"entity-{i}");
        }

        AuthenticateAs(adminUserId, isAdmin: true);

        // Act
        var response = await _client.GetAsync($"/api/admin/audit-logs?entityType={entityType}&page=1&pageSize=3", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadFromJsonAsync<PageResponse<AuditLogEntryResponse>>(IntegrationTestJson.Options, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(content);
        Assert.Equal(3, content.Items.Count);
        Assert.Equal(1, content.Page);
        Assert.Equal(3, content.PageSize);
        Assert.Equal(7, content.TotalCount);
    }

    [Fact]
    public async Task GetAuditLogs_LastPageWithRemainder_ReturnsCorrectNumberOfItems()
    {
        // Arrange
        var adminUserId = await SeedAdminAsync("auditlog-last-page-admin@example.com");
        var entityType = "LastPage";

        // Seed 5 audit logs
        for (int i = 1; i <= 5; i++)
        {
            await SeedAuditLogAsync(actorUserId: adminUserId, targetEntityType: entityType, targetEntityId: $"entity-{i}");
        }

        AuthenticateAs(adminUserId, isAdmin: true);

        // Act - Get last page with page size 2
        var response = await _client.GetAsync($"/api/admin/audit-logs?entityType={entityType}&page=3&pageSize=2", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadFromJsonAsync<PageResponse<AuditLogEntryResponse>>(IntegrationTestJson.Options, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(content);
        Assert.Single(content.Items); // Last page has only 1 item (5 % 2 = 1)
        Assert.Equal(3, content.Page);
        Assert.Equal(2, content.PageSize);
        Assert.Equal(5, content.TotalCount);
    }

    [Fact]
    public async Task GetAuditLogs_ResponseContainsAllRequiredFields()
    {
        // Arrange
        var adminUserId = await SeedAdminAsync("auditlog-structure-admin@example.com");
        var entityType = "ResponseFields";

        var auditLogId = await SeedAuditLogAsync(
            actorUserId: adminUserId,
            actionType: DomainEnums.AuditActionType.Updated,
            targetEntityType: entityType,
            targetEntityId: "order-123",
            outcome: DomainEnums.AuditOutcome.Failed,
            details: "Order update failed");

        AuthenticateAs(adminUserId, isAdmin: true);

        // Act
        var response = await _client.GetAsync($"/api/admin/audit-logs?entityType={entityType}", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadFromJsonAsync<PageResponse<AuditLogEntryResponse>>(IntegrationTestJson.Options, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(content);
        Assert.NotEmpty(content.Items);

        var auditLog = content.Items.FirstOrDefault(a => a.Id == auditLogId);
        Assert.NotNull(auditLog);
        Assert.Equal(auditLogId, auditLog.Id);
        Assert.Equal(adminUserId, auditLog.ActorUserId);
        Assert.Equal(AuditActionType.Updated, auditLog.ActionType);
        Assert.Equal(entityType, auditLog.TargetEntityType);
        Assert.Equal("order-123", auditLog.TargetEntityId);
        Assert.Equal(AuditOutcome.Failed, auditLog.Outcome);
        Assert.Equal("Order update failed", auditLog.Details);
        Assert.True(auditLog.CreatedAtUtc > DateTimeOffset.UtcNow.AddSeconds(-5));
    }

    [Fact]
    public async Task GetAuditLogs_MultipleFiltersApplyTogether()
    {
        // Arrange
        var admin1Id = await SeedAdminAsync("auditlog-multi-filter-admin1@example.com");
        var admin2Id = await SeedAdminAsync("auditlog-multi-filter-admin2@example.com");
        var entityType = "MultiFilter";

        // Seed audit logs with different combinations
        await SeedAuditLogAsync(actorUserId: admin1Id, targetEntityType: entityType, targetEntityId: "prod-1");
        await SeedAuditLogAsync(actorUserId: admin1Id, targetEntityType: entityType, targetEntityId: "prod-2");
        await SeedAuditLogAsync(actorUserId: admin1Id, targetEntityType: "MultiFilterOther", targetEntityId: "order-1");
        await SeedAuditLogAsync(actorUserId: admin2Id, targetEntityType: entityType, targetEntityId: "prod-1");

        AuthenticateAs(admin1Id, isAdmin: true);

        // Act - Filter by ActorUserId AND TargetEntityType
        var response = await _client.GetAsync($"/api/admin/audit-logs?actorUserId={admin1Id}&entityType={entityType}", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadFromJsonAsync<PageResponse<AuditLogEntryResponse>>(IntegrationTestJson.Options, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(content);
        Assert.Equal(2, content.Items.Count);
        Assert.All(content.Items, item =>
        {
            Assert.Equal(admin1Id, item.ActorUserId);
            Assert.Equal(entityType, item.TargetEntityType);
        });
    }

    [Fact]
    public async Task RegisterCustomer_WritesAuditLogEntry()
    {
        // Arrange
        ClearAuthentication();
        var email = $"auditlog-register-{Guid.NewGuid():N}@example.com";
        var request = new RegisterCustomerRequest
        {
            Email = email,
            Password = "Password123!",
            FirstName = "Audit",
            LastName = "Customer",
            Phone = "+4512345678"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/registration/customer", request, TestContext.Current.CancellationToken);
        var registration = await response.Content.ReadFromJsonAsync<RegistrationResponse>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(registration);
        Assert.NotNull(registration.Customer);

        var adminUserId = await SeedAdminAsync("auditlog-register-admin@example.com");
        AuthenticateAs(adminUserId, isAdmin: true);

        var auditResponse = await _client.GetAsync(
            $"/api/admin/audit-logs?entityType={nameof(UserAccount)}&entityId={registration.Customer.Id}",
            TestContext.Current.CancellationToken);
        var auditLogs = await auditResponse.Content.ReadFromJsonAsync<PageResponse<AuditLogEntryResponse>>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, auditResponse.StatusCode);
        Assert.NotNull(auditLogs);
        Assert.Contains(auditLogs.Items, entry =>
            entry.ActionType == AuditActionType.Created &&
            entry.Outcome == AuditOutcome.Succeeded &&
            entry.TargetEntityId == registration.Customer.Id.ToString());
    }

    [Fact]
    public async Task Login_WritesAuditLogEntry()
    {
        // Arrange
        ClearAuthentication();
        var email = $"auditlog-login-{Guid.NewGuid():N}@example.com";
        var password = "Password123!";
        var registrationRequest = new RegisterCustomerRequest
        {
            Email = email,
            Password = password,
            FirstName = "Audit",
            LastName = "Login",
            Phone = "+4512345678"
        };

        var registrationResponse = await _client.PostAsJsonAsync(
            "/api/registration/customer",
            registrationRequest,
            TestContext.Current.CancellationToken);
        var registration = await registrationResponse.Content.ReadFromJsonAsync<RegistrationResponse>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, registrationResponse.StatusCode);
        Assert.NotNull(registration);

        ClearAuthentication();
        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = password
        };

        // Act
        var loginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            loginRequest,
            TestContext.Current.CancellationToken);
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        Assert.NotNull(login);

        var adminUserId = await SeedAdminAsync("auditlog-login-admin@example.com");
        AuthenticateAs(adminUserId, isAdmin: true);

        var auditResponse = await _client.GetAsync(
            $"/api/admin/audit-logs?entityType={nameof(UserAccount)}&entityId={login.User.Id}",
            TestContext.Current.CancellationToken);
        var auditLogs = await auditResponse.Content.ReadFromJsonAsync<PageResponse<AuditLogEntryResponse>>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, auditResponse.StatusCode);
        Assert.NotNull(auditLogs);
        Assert.Contains(auditLogs.Items, entry =>
            entry.ActionType == AuditActionType.Login &&
            entry.Outcome == AuditOutcome.Succeeded &&
            entry.TargetEntityId == login.User.Id.ToString());
    }

    [Fact]
    public async Task Checkout_WritesAuditLogEntry()
    {
        // Arrange
        var (userId, cartId, currencyId, expectedTotal) = await SeedCheckoutCartAsync();
        AuthenticateAs(userId, isAdmin: false);

        var checkoutRequest = new CheckoutRequest
        {
            CartId = cartId,
            SaveShippingAddressAsDefault = false,
            ShippingAddress = new CheckoutShippingAddressRequest
            {
                PostalCode = "2100",
                City = "Copenhagen",
                State = "Capital Region",
                AddressLine1 = "Test Street 1",
                CountryCode = "DK"
            },
            Payments = new List<RecordPaymentRequest>
            {
                new()
                {
                    CurrencyId = currencyId,
                    PaymentType = PaymentType.CreditCard,
                    PaymentInstallments = 1,
                    PaymentValue = expectedTotal
                }
            }
        };

        // Act
        var checkoutResponse = await _client.PostAsJsonAsync(
            "/api/checkout?currency=BRL",
            checkoutRequest,
            TestContext.Current.CancellationToken);
        var checkout = await checkoutResponse.Content.ReadFromJsonAsync<CheckoutResponse>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, checkoutResponse.StatusCode);
        Assert.NotNull(checkout);

        var adminUserId = await SeedAdminAsync("auditlog-checkout-admin@example.com");
        AuthenticateAs(adminUserId, isAdmin: true);

        var auditResponse = await _client.GetAsync(
            $"/api/admin/audit-logs?entityType={nameof(Order)}&entityId={checkout.Order.Id}",
            TestContext.Current.CancellationToken);
        var auditLogs = await auditResponse.Content.ReadFromJsonAsync<PageResponse<AuditLogEntryResponse>>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, auditResponse.StatusCode);
        Assert.NotNull(auditLogs);
        Assert.Contains(auditLogs.Items, entry =>
            entry.ActionType == AuditActionType.Created &&
            entry.Outcome == AuditOutcome.Succeeded &&
            entry.TargetEntityId == checkout.Order.Id.ToString());
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

    private void ClearAuthentication()
    {
        _client.DefaultRequestHeaders.Authorization = null;
    }

    private async Task<Guid> SeedAuditLogAsync(
        Guid? actorUserId = null,
        DomainEnums.AuditActionType actionType = DomainEnums.AuditActionType.Created,
        string targetEntityType = "Product",
        string targetEntityId = "test-entity-id",
        DomainEnums.AuditOutcome outcome = DomainEnums.AuditOutcome.Succeeded,
        string details = "Test audit log")
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var auditLog = TestEntityFactory.CreateAuditLog(
            actorUserId: actorUserId,
            actionType: actionType,
            targetEntityType: targetEntityType,
            targetEntityId: targetEntityId,
            outcome: outcome,
            details: details);

        dbContext.AuditLogs.Add(auditLog);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return auditLog.Id;
    }

    private async Task<(Guid UserId, Guid CartId, Guid CurrencyId, decimal ExpectedTotal)> SeedCheckoutCartAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var customerUser = TestEntityFactory.CreateUserAccount($"checkout-customer-{Guid.NewGuid():N}@example.com");
        var customer = TestEntityFactory.CreateCustomer(customerUser.Id);
        var sellerUser = TestEntityFactory.CreateUserAccount($"checkout-seller-{Guid.NewGuid():N}@example.com");
        var seller = TestEntityFactory.CreateSeller(sellerUser.Id);
        var category = TestEntityFactory.CreateCategory("checkout-category", "Checkout category");
        var product = TestEntityFactory.CreateProduct(category.Id, "Checkout product");
        var listing = TestEntityFactory.CreateListing(seller.Id, product.Id, $"SKU-{Guid.NewGuid():N}", 50m);
        listing.InventoryQuantity = 10;

        var cart = new ShoppingCart
        {
            UserId = customerUser.Id,
            Status = DomainEnums.CartStatus.Active,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(1)
        };

        var cartItem = new CartItem
        {
            CartId = cart.Id,
            ListingId = listing.Id,
            Quantity = 2,
            UnitPriceAtAddition = listing.ListingPrice,
            AddedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        var currency = TestEntityFactory.CreateCurrency("BRL", "Brazilian Real");

        dbContext.UserAccounts.AddRange(customerUser, sellerUser);
        dbContext.Customers.Add(customer);
        dbContext.Sellers.Add(seller);
        dbContext.ProductCategories.Add(category);
        dbContext.Products.Add(product);
        dbContext.ProductListings.Add(listing);
        dbContext.ShoppingCarts.Add(cart);
        dbContext.CartItems.Add(cartItem);
        dbContext.Currencies.Add(currency);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var subtotal = cartItem.UnitPriceAtAddition * cartItem.Quantity;
        var freight = 100m;
        var expectedTotal = subtotal + freight;

        return (customerUser.Id, cart.Id, currency.Id, expectedTotal);
    }
}
