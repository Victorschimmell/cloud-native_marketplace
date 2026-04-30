using Backend.Api.Contracts.User.Registration;
using Backend.Api.Contracts.User.SellerVerification;
using Backend.Api.Mappings.User.Auth;
using App = Backend.Application.DTOs;

namespace Backend.Api.Mappings.User.Registration;

public static class RegistrationMappingExtensions
{
    public static App.RegisterCustomerRequest ToDto(this RegisterCustomerRequest request) =>
        new(request.Email, request.Password, request.FirstName, request.LastName, request.Phone, request.DefaultAddressId);

    public static App.RegisterSellerRequest ToDto(this RegisterSellerRequest request) =>
        new(request.Email, request.Password, request.BusinessName, request.RegistrationNumber, request.PayoutInformation, request.DefaultAddressId);

    public static RegistrationResponse ToResponse(this App.RegistrationResponse response) =>
        new()
        {
            User = response.User.ToModel(),
            Customer = response.Customer?.ToModel(),
            Seller = response.Seller?.ToModel(),
            Token = response.Token?.ToModel()
        };

    public static CustomerResponse ToResponse(this App.CustomerDto customer) =>
        new()
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

    private static CustomerModel ToModel(this App.CustomerDto customer) =>
        new()
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

    private static SellerModel ToModel(this App.SellerDto seller) =>
        new()
        {
            Id = seller.Id,
            UserId = seller.UserId,
            BusinessName = seller.BusinessName,
            RegistrationNumber = seller.RegistrationNumber,
            PayoutInformation = seller.PayoutInformation,
            DefaultAddressId = seller.DefaultAddressId,
            VerificationStatus = (VerificationStatus)seller.VerificationStatus,
            VerifiedAtUtc = seller.VerifiedAtUtc,
            OlistSellerId = seller.OlistSellerId
        };
}
