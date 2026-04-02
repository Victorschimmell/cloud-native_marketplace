namespace Backend.Infrastructure.Persistence.Import.Models;

public sealed record OrderRow(
    string OrderId,
    string CustomerId,
    string OrderStatus,
    DateTimeOffset OrderPurchaseTimestampUtc,
    DateTimeOffset? OrderApprovedAtUtc,
    DateTimeOffset? OrderDeliveredCarrierDateUtc,
    DateTimeOffset? OrderDeliveredCustomerDateUtc,
    DateTimeOffset? OrderEstimatedDeliveryDateUtc);
