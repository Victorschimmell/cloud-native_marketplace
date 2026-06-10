namespace Backend.Api.Contracts.Commerce.Shipments;

public sealed record UpdateShipmentStatusRequest
{
    public required ShipmentStatus Status { get; init; }
}
