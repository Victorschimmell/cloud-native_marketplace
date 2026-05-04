namespace Backend.Api.Contracts.User.Addresses;

public sealed record AddressResponse
{
    public required Guid Id { get; init; }
    public required string PostalCode { get; init; }
    public required string City { get; init; }
    public required string State { get; init; }
    public string? AddressLine1 { get; init; }
    public string? AddressLine2 { get; init; }
    public required string CountryCode { get; init; }
}
