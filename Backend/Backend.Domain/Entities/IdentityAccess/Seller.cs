using Backend.Domain.Base;
using Backend.Domain.Entities.Location;
using Backend.Domain.ValueObjects;
using Backend.Domain.Enums;

namespace Backend.Domain.Entities.IdentityAccess;

public sealed class Seller : AggregateRoot<Guid>
{
    public Seller()
    {
        Id = Guid.NewGuid();
    }

    public Guid UserId { get; set; }
    public required string BusinessName { get; set; }
    public required string RegistrationNumber { get; set; }
    public required string PayoutInformation { get; set; }
    public Guid? DefaultAddressId { get; set; }
    public VerificationStatus VerificationStatus { get; set; }
    public DateTimeOffset? VerifiedAtUtc { get; set; }
    public string? OlistSellerId { get; set; }

    public UserAccount? UserAccount { get; set; }
    public Address? DefaultAddress { get; set; }
    public ICollection<SellerVerificationRequest> VerificationRequests { get; } = [];
    public ICollection<Catalog.ProductListing> Listings { get; } = [];
    public ICollection<Orders.OrderItem> OrderItems { get; } = [];
    public ICollection<Orders.Shipment> Shipments { get; } = [];
}


