using Backend.Domain.Enums;

namespace Backend.Application.DTOs;

public sealed record CartItemDto(
    Guid Id,
    Guid CartId,
    Guid ListingId,
    Guid ProductId,
    string ProductName,
    string? ImageUrl,
    int Quantity,
    decimal UnitPriceAtAddition,
    decimal LineTotal,
    string CurrencyCode,
    DateTimeOffset AddedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record CartDto(
    Guid Id,
    Guid? UserId,
    Guid? SessionId,
    CartStatus Status,
    DateTimeOffset ExpiresAtUtc,
    IReadOnlyList<CartItemDto> Items);

public sealed record GetCartRequest(Guid? CartId, Guid? UserId, Guid? SessionId);

public sealed record AddCartItemRequest(Guid? CartId, Guid? UserId, Guid? SessionId, Guid ListingId, int Quantity);

public sealed record UpdateCartItemRequest(Guid? CartId, Guid? UserId, Guid? SessionId, Guid ListingId, int Quantity);

public sealed record RemoveCartItemRequest(Guid? CartId, Guid? UserId, Guid? SessionId, Guid ListingId);

public sealed record CheckoutLineDto(Guid ListingId, Guid ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal LineTotal, string CurrencyCode);

public sealed record CheckoutPreviewDto(
    IReadOnlyList<CheckoutLineDto> Lines,
    decimal SubtotalAmount,
    decimal FreightAmount,
    decimal TotalAmount,
    string CurrencyCode);

public sealed record GetCheckoutPreviewRequest(Guid? CartId, Guid? UserId, Guid? SessionId);

public sealed record CheckoutShippingAddressDto(
    string PostalCode,
    string City,
    string State,
    string? AddressLine1,
    string? AddressLine2,
    string CountryCode);

public sealed record CheckoutRequest(
    Guid? CartId,
    Guid? UserId,
    Guid? SessionId,
    CheckoutShippingAddressDto ShippingAddress,
    bool SaveShippingAddressAsDefault,
    IReadOnlyList<RecordPaymentDetails> Payments);

public sealed record CheckoutResponse(
    OrderDto Order,
    CartDto Cart,
    IReadOnlyList<PaymentDto> Payments,
    decimal TotalAmount,
    string CurrencyCode);
