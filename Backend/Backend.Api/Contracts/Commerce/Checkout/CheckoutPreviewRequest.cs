using Backend.Api.Attributes;

namespace Backend.Api.Contracts.Commerce.Checkout;

public sealed record CheckoutPreviewRequest
{
    [NotEmptyGuid]
    public Guid? CartId { get; init; }

    [NotEmptyGuid]
    public Guid? UserId { get; init; }

    [NotEmptyGuid]
    public Guid? SessionId { get; init; }
}
