using System.ComponentModel.DataAnnotations;
using Backend.Api.Attributes;

namespace Backend.Api.Contracts.Commerce.Cart;

public sealed record AddCartItemRequest
{
    [NotEmptyGuid]
    public Guid? CartId { get; init; }

    [NotEmptyGuid]
    public required Guid ListingId { get; init; }

    [Range(1, int.MaxValue)]
    public required int Quantity { get; init; }
}
