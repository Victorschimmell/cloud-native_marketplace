using Backend.Domain.Entities.IdentityAccess;
using Backend.Domain.Enums;
using Backend.Domain.ValueObjects;

namespace Backend.UnitTests.Entities;

public class IdentityAccessEntitiesTests
{
    [Fact]
    public void UserAccount_InitializesIdentityAndCollections()
    {
        var entity = new UserAccount
        {
            Email = new EmailAddress("user@example.com"),
            PasswordHash = "hash"
        };

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Empty(entity.Sessions);
        Assert.Empty(entity.Blocks);
        Assert.Empty(entity.ShoppingCarts);
        Assert.Empty(entity.Notifications);
        Assert.Empty(entity.AuditLogs);
    }

    [Fact]
    public void Customer_StoresNameAsValueObject()
    {
        var entity = new Customer
        {
            Name = new PersonName("Victor", "LastName"),
            Phone = "+4512345678"
        };

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal("Victor LastName", entity.Name.FullName);
        Assert.Empty(entity.Orders);
    }

    [Fact]
    public void Seller_InitializesVerificationAndCollections()
    {
        var entity = new Seller
        {
            BusinessName = "MarketplaceTraders",
            RegistrationNumber = "REG-1",
            PayoutInformation = "bank-account",
            VerificationStatus = VerificationStatus.Pending
        };

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(VerificationStatus.Pending, entity.VerificationStatus);
        Assert.Empty(entity.VerificationRequests);
        Assert.Empty(entity.Listings);
        Assert.Empty(entity.OrderItems);
        Assert.Empty(entity.Shipments);
    }

    [Fact]
    public void Admin_InitializesCollections()
    {
        var entity = new Admin
        {
            DisplayName = "Platform Admin"
        };

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Empty(entity.AppliedBlocks);
        Assert.Empty(entity.ReviewedVerificationRequests);
    }

    [Fact]
    public void UserSession_InitializesIdentityAndGuestCarts()
    {
        var entity = new UserSession
        {
            IpAddress = "127.0.0.1",
            UserAgent = "xunit"
        };

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Empty(entity.GuestShoppingCarts);
    }

    [Fact]
    public void UserBlock_StoresAuditFriendlyFields()
    {
        var entity = new UserBlock
        {
            Reason = "Too many failed logins"
        };

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal("Too many failed logins", entity.Reason);
    }

    [Fact]
    public void SellerVerificationRequest_StoresSnapshots()
    {
        var entity = new SellerVerificationRequest
        {
            BusinessNameSnapshot = "MarketplaceTraders",
            RegistrationNumberSnapshot = "REG-1",
            SubmittedDetails = "Submitted documents",
            Status = SellerVerificationRequestStatus.Submitted
        };

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(SellerVerificationRequestStatus.Submitted, entity.Status);
    }
}
