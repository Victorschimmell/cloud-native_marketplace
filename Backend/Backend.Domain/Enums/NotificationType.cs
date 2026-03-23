namespace Backend.Domain.Enums;

public enum NotificationType
{
    OrderStatusChanged = 1,
    ShipmentUpdated = 2,
    PaymentReceived = 3,
    VerificationReviewed = 4,
    SecurityAlert = 5,
    SystemAlert = 6
}
