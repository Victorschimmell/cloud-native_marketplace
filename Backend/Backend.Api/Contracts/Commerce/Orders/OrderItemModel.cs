using Backend.Domain.Enums;

namespace Backend.Api.Contracts.Commerce.Orders;

public sealed record OrderItemModel
{
    public required Guid OrderId { get; init; }
    public required int OrderItemId { get; init; }
    public required Guid ListingId { get; init; }
    public required Guid ProductId { get; init; }
    public required string ProductName { get; init; }
    public string? ImageUrl { get; init; }
    public required int ProductPhotosQty { get; init; }
    public required Guid SellerId { get; init; }
    public required string SellerName { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
    public required decimal LineTotal { get; init; }
    public required decimal FreightValue { get; init; }
    public required string CurrencyCode { get; init; }
    public DateTimeOffset? ShippingLimitDateUtc { get; init; }
    public required OrderStatus FulfillmentStatus { get; init; }
    public DateTimeOffset? FulfillmentApprovedAtUtc { get; init; }
    public DateTimeOffset? FulfillmentProcessingAtUtc { get; init; }
    public DateTimeOffset? FulfillmentShippedAtUtc { get; init; }
}
