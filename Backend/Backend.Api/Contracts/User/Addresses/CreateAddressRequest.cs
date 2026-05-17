using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Contracts.User.Addresses;

public sealed record CreateAddressRequest
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

    public bool MakeDefault { get; init; }
}
