namespace Backend.Application.DTOs;

public sealed record CartItemDto(
    Guid Id,
    Guid CartId,
    Guid ListingId,
    int Quantity,
    decimal UnitPriceAtAddition,
    string CurrencyCode,
    DateTimeOffset AddedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record CartDto(
    Guid Id,
    Guid? UserId,
    Guid? SessionId,
    string Status,
    DateTimeOffset ExpiresAtUtc,
    IReadOnlyList<CartItemDto> Items);

public sealed record GetCartRequest(Guid? CartId, Guid? UserId, Guid? SessionId);

public sealed record AddCartItemRequest(Guid? CartId, Guid? UserId, Guid? SessionId, Guid ListingId, int Quantity);

public sealed record RemoveCartItemRequest(Guid? CartId, Guid? UserId, Guid? SessionId, Guid ListingId, int Quantity);

public sealed record CheckoutLineDto(Guid ListingId, int Quantity, decimal UnitPrice, decimal LineTotal, string CurrencyCode);

public sealed record GetCheckoutPreviewRequest(Guid? CartId, Guid? UserId, Guid? SessionId);

public sealed record CheckoutRequest(
    Guid? CartId,
    Guid? UserId,
    Guid? SessionId,
    Guid ShippingAddressId,
    string OrderNumber,
    IReadOnlyList<RecordPaymentRequest> Payments);

public sealed record CheckoutResponse(
    OrderDto Order,
    CartDto Cart,
    IReadOnlyList<PaymentDto> Payments,
    decimal TotalAmount);
