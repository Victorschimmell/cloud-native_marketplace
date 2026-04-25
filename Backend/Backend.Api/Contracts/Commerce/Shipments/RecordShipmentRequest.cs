using Backend.Api.Attributes;

namespace Backend.Api.Contracts.Commerce.Shipments;

public sealed record RecordShipmentRequest
{
    [NotEmptyGuid]
    public required Guid SellerId { get; init; }
    public required string CarrierName { get; init; }
    public required string TrackingNumber { get; init; }
    public required ShipmentStatus ShipmentStatus { get; init; }
}
