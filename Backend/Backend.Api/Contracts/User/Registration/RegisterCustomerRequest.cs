using System.ComponentModel.DataAnnotations;
using Backend.Api.Attributes;

namespace Backend.Api.Contracts.User.Registration;

public sealed record RegisterCustomerRequest
{
    [EmailAddress]
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }

    [Phone]
    public required string Phone { get; init; }

    [NotEmptyGuid]
    public Guid? DefaultAddressId { get; init; }
}
