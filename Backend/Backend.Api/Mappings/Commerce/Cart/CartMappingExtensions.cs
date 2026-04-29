using Backend.Api.Contracts.Commerce.Cart;
using App = Backend.Application.DTOs;

namespace Backend.Api.Mappings.Commerce.Cart;

public static class CartMappingExtensions
{
    public static App.AddCartItemRequest ToApplicationRequest(this AddCartItemRequest request) =>
        new(request.CartId, request.UserId, request.SessionId, request.ListingId, request.Quantity);

    public static CartResponse ToResponse(this App.CartDto cart) =>
        new()
        {
            Id = cart.Id,
            UserId = cart.UserId,
            SessionId = cart.SessionId,
            Status = cart.Status,
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
            AddedAtUtc = item.AddedAtUtc,
            UpdatedAtUtc = item.UpdatedAtUtc
        };
}
