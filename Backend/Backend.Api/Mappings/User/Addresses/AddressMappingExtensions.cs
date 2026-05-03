using Backend.Api.Contracts.User.Addresses;
using App = Backend.Application.DTOs;

namespace Backend.Api.Mappings.User.Addresses;

public static class AddressMappingExtensions
{
    public static App.CreateAddressRequest ToApplicationRequest(this CreateAddressRequest request) =>
        new(
            request.PostalCode,
            request.City,
            request.State,
            request.AddressLine1,
            request.AddressLine2,
            request.CountryCode,
            request.MakeDefault);

    public static AddressResponse ToResponse(this App.AddressDto address) =>
        new()
        {
            Id = address.Id,
            PostalCode = address.PostalCode,
            City = address.City,
            State = address.State,
            AddressLine1 = address.AddressLine1,
            AddressLine2 = address.AddressLine2,
            CountryCode = address.CountryCode
        };
}
