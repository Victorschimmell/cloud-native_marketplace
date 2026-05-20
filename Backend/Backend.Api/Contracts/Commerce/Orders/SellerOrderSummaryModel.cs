using Backend.Api.Contracts.Commerce.Shipments;

namespace Backend.Api.Contracts.Commerce.Orders;

public sealed record SellerOrderSummaryModel
{
    public required Guid Id { get; init; }
    public required Guid CustomerId { get; init; }
    public required string CustomerName { get; init; }
    public required string CustomerEmail { get; init; }
    public required string OrderNumber { get; init; }
    public required OrderStatus OrderStatus { get; init; }
    public required string? OrderStatusDescription { get; init; }
    public required DateTimeOffset OrderPurchaseTimestampUtc { get; init; }
    public required DateTimeOffset? OrderApprovedAtUtc { get; init; }
    public required DateTimeOffset? OrderDeliveredCarrierDateUtc { get; init; }
    public required DateTimeOffset? OrderDeliveredCustomerDateUtc { get; init; }
    public required DateTimeOffset? OrderEstimatedDeliveryDateUtc { get; init; }
    public required decimal SubtotalAmount { get; init; }
    public required decimal FreightAmount { get; init; }
    public required decimal TotalAmount { get; init; }
    public required string CurrencyCode { get; init; }
    public required IReadOnlyList<OrderItemModel> Items { get; init; }
    public required IReadOnlyList<ShipmentModel> Shipments { get; init; }
}

public sealed record SellerOrderStatsModel
{
    public required int TotalOrders { get; init; }
    public required int ActiveOrders { get; init; }
    public required decimal TotalRevenue { get; init; }
    public required string CurrencyCode { get; init; }
}
