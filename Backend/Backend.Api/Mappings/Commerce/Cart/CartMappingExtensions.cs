using Backend.Api.Contracts.Commerce.Cart;
using App = Backend.Application.DTOs;

namespace Backend.Api.Mappings.Commerce.Cart;

public static class CartMappingExtensions
{
    public static App.AddCartItemRequest ToApplicationRequest(this AddCartItemRequest request, Guid authenticatedUserId) =>
        new(request.CartId, authenticatedUserId, null, request.ListingId, request.Quantity);

    public static App.UpdateCartItemRequest ToApplicationRequest(this UpdateCartItemRequest request, Guid listingId, Guid authenticatedUserId) =>
        new(request.CartId, authenticatedUserId, null, listingId, request.Quantity);

    public static CartResponse ToResponse(this App.CartDto cart) =>
        new()
        {
            Id = cart.Id,
            UserId = cart.UserId,
            SessionId = cart.SessionId,
            Status = (CartStatus)cart.Status,
            ExpiresAtUtc = cart.ExpiresAtUtc,
            Items = cart.Items.Select(item => item.ToModel()).ToArray()
        };

    public static CartModel ToModel(this App.CartDto cart) =>
        new()
        {
            Id = cart.Id,
            UserId = cart.UserId,
            SessionId = cart.SessionId,
            Status = (CartStatus)cart.Status,
            ExpiresAtUtc = cart.ExpiresAtUtc,
            Items = cart.Items.Select(item => item.ToModel()).ToArray()
        };

    private static CartItemModel ToModel(this App.CartItemDto item) =>
        new()
        {
            Id = item.Id,
            CartId = item.CartId,
            ListingId = item.ListingId,
            Quantity = item.Quantity,
            UnitPriceAtAddition = item.UnitPriceAtAddition,
            CurrencyCode = item.CurrencyCode,
            AddedAtUtc = item.AddedAtUtc,
            UpdatedAtUtc = item.UpdatedAtUtc
        };
}
