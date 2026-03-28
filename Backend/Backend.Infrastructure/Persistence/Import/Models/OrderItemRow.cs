namespace Backend.Infrastructure.Persistence.Import.Models;

public sealed record OrderItemRow(
    string OrderId,
    int OrderItemId,
    string ProductId,
    string SellerId,
    DateTimeOffset? ShippingLimitDateUtc,
    decimal Price,
    decimal FreightValue);
