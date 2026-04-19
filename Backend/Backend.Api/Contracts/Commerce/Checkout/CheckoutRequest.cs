using System.ComponentModel.DataAnnotations;
using Backend.Api.Attributes;
using Backend.Api.Contracts.Commerce.Payments;

namespace Backend.Api.Contracts.Commerce.Checkout;

public sealed record CheckoutRequest : IValidatableObject
{
    [NotEmptyGuid]
    public Guid? CartId { get; init; }

    [NotEmptyGuid]
    public Guid? UserId { get; init; }

    [NotEmptyGuid]
    public Guid? SessionId { get; init; }

    [NotEmptyGuid]
    public required Guid ShippingAddressId { get; init; }
    public required string OrderNumber { get; init; }
    public required IReadOnlyList<RecordPaymentRequest> Payments { get; init; }

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
