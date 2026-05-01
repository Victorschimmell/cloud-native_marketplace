using Backend.Api.Attributes;
using Backend.Api.Contracts.Commerce.Payments;

namespace Backend.Api.Contracts.Commerce.Checkout;

public sealed record CheckoutRequest
{
    [NotEmptyGuid]
    public Guid? CartId { get; init; }

    [NotEmptyGuid]
    public Guid? UserId { get; init; }

    [NotEmptyGuid]
    public Guid? SessionId { get; init; }

    [NotEmptyGuid]
    public required Guid ShippingAddressId { get; init; }
    public required IReadOnlyList<RecordPaymentRequest> Payments { get; init; }
}
