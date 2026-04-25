using System.ComponentModel.DataAnnotations;
using Backend.Api.Attributes;

namespace Backend.Api.Contracts.User.Registration;

public sealed record RegisterSellerRequest
{
    [EmailAddress]
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string BusinessName { get; init; }
    public required string RegistrationNumber { get; init; }
    public required string PayoutInformation { get; init; }

    [NotEmptyGuid]
    public Guid? DefaultAddressId { get; init; }
}
