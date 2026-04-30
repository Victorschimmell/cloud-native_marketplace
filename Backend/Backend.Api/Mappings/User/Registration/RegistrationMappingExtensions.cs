using Backend.Api.Contracts.User.Registration;
using App = Backend.Application.DTOs;

namespace Backend.Api.Mappings.User.Registration;

public static class RegistrationMappingExtensions
{
    public static CustomerResponse ToResponse(this App.CustomerDto customer)
    {
        return new CustomerResponse
        {
            Id = customer.Id,
            UserId = customer.UserId,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Phone = customer.Phone,
            DefaultAddressId = customer.DefaultAddressId,
            OlistCustomerId = customer.OlistCustomerId,
            OlistCustomerUniqueId = customer.OlistCustomerUniqueId
        };
    }
}
