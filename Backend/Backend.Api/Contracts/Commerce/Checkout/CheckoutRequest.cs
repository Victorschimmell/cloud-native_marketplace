using Backend.Api.Attributes;
using Backend.Api.Contracts.Commerce.Payments;
using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Contracts.Commerce.Checkout;

public sealed record CheckoutShippingAddressRequest
{
    [Required]
    public required string PostalCode { get; init; }

    [Required]
    public required string City { get; init; }

    [Required]
    public required string State { get; init; }

    [Required]
    public required string AddressLine1 { get; init; }

    public string? AddressLine2 { get; init; }

    [Required]
    public required string CountryCode { get; init; }
}

public sealed record CheckoutRequest
{
    [NotEmptyGuid]
    public Guid? CartId { get; init; }

    [Required]
    public required CheckoutShippingAddressRequest ShippingAddress { get; init; }

    public required IReadOnlyList<RecordPaymentRequest> Payments { get; init; }
}
