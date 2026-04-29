using System.ComponentModel.DataAnnotations;
using Backend.Api.Attributes;

namespace Backend.Api.Contracts.User.Registration;

public sealed record RegisterCustomerRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    [MinLength(8)]
    public required string Password { get; init; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string FirstName { get; init; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string LastName { get; init; }

    [Required]
    [Phone]
    public required string Phone { get; init; }

    [NotEmptyGuid]
    public Guid? DefaultAddressId { get; init; }
}
