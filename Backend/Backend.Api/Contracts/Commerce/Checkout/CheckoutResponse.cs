using Backend.Api.Contracts.Commerce.Cart;
using Backend.Api.Contracts.Commerce.Orders;
using Backend.Api.Contracts.Commerce.Payments;

namespace Backend.Api.Contracts.Commerce.Checkout;

public sealed record CheckoutResponse
{
    public required OrderModel Order { get; init; }
    public required CartModel Cart { get; init; }
    public required IReadOnlyList<PaymentModel> Payments { get; init; }
    public required decimal TotalAmount { get; init; }
}
