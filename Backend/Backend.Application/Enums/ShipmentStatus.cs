namespace Backend.Application.Enums;

public enum ShipmentStatus
{
    Pending = 1,
    ReadyForPickup = 2,
    InTransit = 3,
    Delivered = 4,
    Returned = 5,
    Lost = 6,
    Cancelled = 7
}