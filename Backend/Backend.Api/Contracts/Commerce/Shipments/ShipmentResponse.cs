using Backend.Domain.Enums;

namespace Backend.Api.Contracts.Commerce.Shipments;

public sealed record ShipmentResponse
{
    public required Guid Id { get; init; }
    public required Guid OrderId { get; init; }
    public required Guid SellerId { get; init; }
    public required string CarrierName { get; init; }
    public required string TrackingNumber { get; init; }
    public required ShipmentStatus ShipmentStatus { get; init; }
    public DateTimeOffset? ShippedAtUtc { get; init; }
    public DateTimeOffset? DeliveredAtUtc { get; init; }
    public DateTimeOffset? ReturnedAtUtc { get; init; }
}
