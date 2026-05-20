using Backend.Domain.Enums;

namespace Backend.Application.DTOs;

public sealed record OrderItemDto(
    Guid OrderId,
    int OrderItemId,
    Guid ListingId,
    Guid ProductId,
    string ProductName,
    string? ImageUrl,
    int ProductPhotosQty,
    Guid SellerId,
    string SellerName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    decimal FreightValue,
    string CurrencyCode,
    DateTimeOffset? ShippingLimitDateUtc);

public sealed record PaymentDto(
    Guid OrderId,
    int PaymentSequential,
    Guid CurrencyId,
    PaymentType PaymentType,
    int PaymentInstallments,
    decimal PaymentValue,
    PaymentStatus PaymentStatus,
    string? ExternalPaymentReference,
    DateTimeOffset? PaidAtUtc);

public sealed record CurrencyDto(
    Guid Id,
    string Code,
    string Name,
    string? Symbol);

public sealed record ReviewDto(
    Guid Id,
    Guid OrderId,
    int? OrderItemId,
    Guid ProductId,
    string? ReviewerDisplayName,
    int ReviewScore,
    string? ReviewCommentTitle,
    string? ReviewCommentMessage,
    DateTimeOffset ReviewCreationDateUtc,
    DateTimeOffset? ReviewAnswerTimestampUtc);

public sealed record ShipmentDto(
    Guid Id,
    Guid OrderId,
    Guid SellerId,
    string CarrierName,
    string TrackingNumber,
    ShipmentStatus ShipmentStatus,
    DateTimeOffset? ShippedAtUtc,
    DateTimeOffset? DeliveredAtUtc,
    DateTimeOffset? ReturnedAtUtc);

public sealed record OrderDto(
    Guid Id,
    Guid CustomerId,
    Guid UserId,
    Guid ShippingAddressId,
    string OrderNumber,
    OrderStatus OrderStatus,
    string? OrderStatusDescription,
    DateTimeOffset OrderPurchaseTimestampUtc,
    DateTimeOffset? OrderApprovedAtUtc,
    DateTimeOffset? OrderDeliveredCarrierDateUtc,
    DateTimeOffset? OrderDeliveredCustomerDateUtc,
    DateTimeOffset? OrderEstimatedDeliveryDateUtc,
    decimal SubtotalAmount,
    decimal FreightAmount,
    decimal TotalAmount,
    string CurrencyCode,
    Guid? PlacedFromCartId,
    IReadOnlyList<OrderItemDto> Items,
    IReadOnlyList<PaymentDto> Payments,
    IReadOnlyList<ReviewDto> Reviews,
    IReadOnlyList<ShipmentDto> Shipments);

public sealed record OrderSummaryDto(
    Guid Id,
    Guid CustomerId,
    Guid UserId,
    Guid ShippingAddressId,
    string OrderNumber,
    OrderStatus OrderStatus,
    string? OrderStatusDescription,
    DateTimeOffset OrderPurchaseTimestampUtc,
    DateTimeOffset? OrderApprovedAtUtc,
    DateTimeOffset? OrderDeliveredCarrierDateUtc,
    DateTimeOffset? OrderDeliveredCustomerDateUtc,
    DateTimeOffset? OrderEstimatedDeliveryDateUtc,
    decimal SubtotalAmount,
    decimal FreightAmount,
    decimal TotalAmount,
    string CurrencyCode,
    Guid? PlacedFromCartId,
    IReadOnlyList<OrderItemDto> Items,
    IReadOnlyList<ShipmentDto> Shipments);

public sealed record SellerOrderSummaryDto(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string CustomerEmail,
    string OrderNumber,
    OrderStatus OrderStatus,
    string? OrderStatusDescription,
    DateTimeOffset OrderPurchaseTimestampUtc,
    DateTimeOffset? OrderApprovedAtUtc,
    DateTimeOffset? OrderDeliveredCarrierDateUtc,
    DateTimeOffset? OrderDeliveredCustomerDateUtc,
    DateTimeOffset? OrderEstimatedDeliveryDateUtc,
    decimal SubtotalAmount,
    decimal FreightAmount,
    decimal TotalAmount,
    string CurrencyCode,
    bool CanUpdateStatus,
    IReadOnlyList<OrderItemDto> Items,
    IReadOnlyList<ShipmentDto> Shipments);

public sealed record SellerOrderStatsDto(
    int TotalOrders,
    int ActiveOrders,
    decimal TotalRevenue,
    string CurrencyCode);

public sealed record UpdateOrderStatusRequest(Guid OrderId, OrderStatus Status);

public sealed record CancelOrderRequest(Guid OrderId, string? Reason);

public sealed record RecordPaymentDetails(
    Guid CurrencyId,
    PaymentType PaymentType,
    int PaymentInstallments,
    decimal PaymentValue,
    string? ExternalPaymentReference);

public sealed record RecordPaymentRequest(
    Guid OrderId,
    RecordPaymentDetails PaymentDetails);

public sealed record CreateReviewRequest(
    Guid OrderId,
    int OrderItemId,
    int ReviewScore,
    string? ReviewCommentTitle,
    string? ReviewCommentMessage);

public sealed record RecordShipmentRequest(
    Guid OrderId,
    Guid SellerId,
    string CarrierName,
    string TrackingNumber,
    ShipmentStatus ShipmentStatus);

public sealed record UpdateShipmentStatusRequest(Guid ShipmentId, ShipmentStatus ShipmentStatus);
