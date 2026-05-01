using Backend.Api.Contracts.Commerce.Checkout;
using Backend.Api.Mappings.Commerce.Cart;
using Backend.Api.Mappings.Commerce.Orders;
using Backend.Api.Mappings.Commerce.Payments;
using App = Backend.Application.DTOs;

namespace Backend.Api.Mappings.Commerce.Checkout;

public static class CheckoutMappingExtensions
{
    public static App.GetCheckoutPreviewRequest ToApplicationRequest(this CheckoutPreviewRequest request, Guid authenticatedUserId) =>
        new(request.CartId, authenticatedUserId, null);

    public static CheckoutPreviewLineResponse ToResponse(this App.CheckoutLineDto line) =>
        new()
        {
            ListingId = line.ListingId,
            Quantity = line.Quantity,
            UnitPrice = line.UnitPrice,
            LineTotal = line.LineTotal,
            CurrencyCode = line.CurrencyCode
        };

    public static App.CheckoutRequest ToApplicationRequest(this CheckoutRequest request, Guid authenticatedUserId) =>
        new(
            request.CartId,
            authenticatedUserId,
            null,
            request.ShippingAddressId,
            request.Payments.Select(p => p.ToApplicationRequest()).ToArray());

    public static CheckoutResponse ToResponse(this App.CheckoutResponse response) =>
        new()
        {
            Order = response.Order.ToModel(),
            Cart = response.Cart.ToModel(),
            Payments = response.Payments.Select(p => p.ToModel()).ToArray(),
            TotalAmount = response.TotalAmount,
        };
}
