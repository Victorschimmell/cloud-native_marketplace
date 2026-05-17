namespace Backend.Application.DTOs;

public sealed record AddressDto(
    Guid Id,
    string PostalCode,
    string City,
    string State,
    string? AddressLine1,
    string? AddressLine2,
    string CountryCode);

public sealed record CreateAddressRequest(
    string PostalCode,
    string City,
    string State,
    string? AddressLine1,
    string? AddressLine2,
    string CountryCode,
    bool MakeDefault);
