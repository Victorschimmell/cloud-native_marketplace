using System.ComponentModel.DataAnnotations;
using Backend.Api.Attributes;

namespace Backend.Api.Contracts.User.Registration;

public sealed record RegisterSellerRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    [MinLength(8)]
    public required string Password { get; init; }

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public required string BusinessName { get; init; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string RegistrationNumber { get; init; }

    [Required]
    [StringLength(500, MinimumLength = 1)]
    public required string PayoutInformation { get; init; }

    [NotEmptyGuid]
    public Guid? DefaultAddressId { get; init; }
}
