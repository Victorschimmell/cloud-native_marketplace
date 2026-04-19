using System.ComponentModel.DataAnnotations;
using Backend.Api.Attributes;

namespace Backend.Api.Contracts.Commerce.Cart;

public sealed record AddCartItemRequest : IValidatableObject
{
    [NotEmptyGuid]
    public Guid? CartId { get; init; }

    [NotEmptyGuid]
    public Guid? UserId { get; init; }

    [NotEmptyGuid]
    public Guid? SessionId { get; init; }

    [NotEmptyGuid]
    public required Guid ListingId { get; init; }

    [Range(1, int.MaxValue)]
    public required int Quantity { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!CartId.HasValue && !UserId.HasValue && !SessionId.HasValue)
        {
            yield return new ValidationResult(
                "At least one of CartId, UserId, or SessionId must be provided.",
                [nameof(CartId), nameof(UserId), nameof(SessionId)]);
        }
    }
}
