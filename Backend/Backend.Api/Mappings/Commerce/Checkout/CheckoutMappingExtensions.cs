using Backend.Api.Contracts.Commerce.Checkout;
using App = Backend.Application.DTOs;

namespace Backend.Api.Mappings.Commerce.Checkout;

public static class CheckoutMappingExtensions
{
    public static App.GetCheckoutPreviewRequest ToApplicationRequest(this CheckoutPreviewRequest request) =>
        new(request.CartId, request.UserId, request.SessionId);

    public static CheckoutPreviewLineResponse ToResponse(this App.CheckoutLineDto line) =>
        new()
        {
            ListingId = line.ListingId,
            Quantity = line.Quantity,
            UnitPrice = line.UnitPrice,
            LineTotal = line.LineTotal,
            CurrencyCode = line.CurrencyCode
        };
}
