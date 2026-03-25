using Backend.Domain.Base;
using Backend.Domain.Enums;

namespace Backend.Domain.Entities.Orders;

public sealed class Shipment : AuditableEntity<Guid>
{
    public Shipment()
    {
        Id = Guid.NewGuid();
    }

    public Guid OrderId { get; set; }
    public Guid SellerId { get; set; }
    public required string CarrierName { get; set; }
    public required string TrackingNumber { get; set; }
    public ShipmentStatus ShipmentStatus { get; set; }
    public DateTimeOffset? ShippedAtUtc { get; set; }
    public DateTimeOffset? DeliveredAtUtc { get; set; }
    public DateTimeOffset? ReturnedAtUtc { get; set; }

    public Order? Order { get; set; }
    public IdentityAccess.Seller? Seller { get; set; }
}
