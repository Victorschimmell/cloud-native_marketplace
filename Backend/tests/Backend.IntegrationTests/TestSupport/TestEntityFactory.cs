using Backend.Domain.Entities.Catalog;
using Backend.Domain.Entities.IdentityAccess;
using Backend.Domain.Entities.Location;
using Backend.Domain.Entities.Operations;
using Backend.Domain.Entities.Orders;
using Backend.Domain.Enums;
using Backend.Domain.ValueObjects;

namespace Backend.IntegrationTests.TestSupport;

internal static class TestEntityFactory
{
    public static UserAccount CreateUserAccount(string email) =>
        new()
        {
            Email = new EmailAddress(email),
            PasswordHash = "hashed-password",
            AccountStatus = AccountStatus.Active
        };

    public static Customer CreateCustomer(Guid userId, Guid? defaultAddressId = null) =>
        new()
        {
            UserId = userId,
            FirstName = "Test",
            LastName = "Customer",
            Phone = "+4512345678",
            DefaultAddressId = defaultAddressId
        };

    public static Seller CreateSeller(Guid userId, Guid? defaultAddressId = null) =>
        new()
        {
            UserId = userId,
            BusinessName = "MarketplaceTraders",
            RegistrationNumber = $"REG-{Guid.NewGuid():N}",
            PayoutInformation = "iban:dk5000400440116243",
            DefaultAddressId = defaultAddressId,
            VerificationStatus = VerificationStatus.Pending
        };

    public static Address CreateAddress() =>
        new()
        {
            PostalCode = "2100",
            City = "Copenhagen",
            State = "Capital Region",
            CountryCode = "DK"
        };

    public static ProductCategory CreateCategory(string namePt, string? nameEn = null) =>
        new()
        {
            CategoryNamePt = namePt,
            CategoryNameEn = nameEn
        };

    public static Product CreateProduct(Guid categoryId, string name) =>
        new()
        {
            CategoryId = categoryId,
            ProductName = name,
            Description = $"{name} description",
            ProductNameLength = name.Length,
            ProductDescriptionLength = $"{name} description".Length
        };

    public static ProductListing CreateListing(Guid sellerId, Guid productId, string sku, decimal price) =>
        new()
        {
            SellerId = sellerId,
            ProductId = productId,
            Sku = sku,
            ListingPrice = price,
            InventoryQuantity = 10,
            VisibilityStatus = ListingVisibilityStatus.Published,
            PublishedAtUtc = DateTimeOffset.UtcNow
        };

    public static Currency CreateCurrency(string code, string name) =>
        new()
        {
            Code = code,
            Name = name,
            Symbol = code
        };

    public static Order CreateOrder(Guid customerId, Guid shippingAddressId, string orderNumber, DateTimeOffset purchasedAt) =>
        new()
        {
            CustomerId = customerId,
            ShippingAddressId = shippingAddressId,
            OrderNumber = orderNumber,
            OrderStatus = OrderStatus.Pending,
            OrderPurchaseTimestampUtc = purchasedAt,
            SubtotalAmount = 100m,
            FreightAmount = 15m,
            TotalAmount = 115m
        };

    public static AuditLog CreateAuditLog(
        Guid? actorUserId = null,
        string? actorIpAddress = "127.0.0.1",
        AuditActionType actionType = AuditActionType.Created,
        string targetEntityType = "Product",
        string targetEntityId = "test-entity-id",
        AuditOutcome outcome = AuditOutcome.Succeeded,
        string details = "Test audit log",
        DateTimeOffset? createdAtUtc = null) =>
        new()
        {
            ActorUserId = actorUserId,
            ActorIpAddress = actorIpAddress,
            ActionType = actionType,
            TargetEntityType = targetEntityType,
            TargetEntityId = targetEntityId,
            Outcome = outcome,
            Details = details,
            CreatedAtUtc = createdAtUtc ?? DateTimeOffset.UtcNow
        };
}
