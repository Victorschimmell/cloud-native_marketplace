using Backend.Api.Attributes;

namespace Backend.Api.Contracts.Commerce.Cart;

public sealed record UpdateCartItemRequest
{
    [NotEmptyGuid]
    public Guid? CartId { get; init; }

    public required int Quantity { get; init; }
}
