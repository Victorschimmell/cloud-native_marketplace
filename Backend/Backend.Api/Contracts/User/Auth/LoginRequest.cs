using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Contracts.User.Auth;

public sealed record LoginRequest
{
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public required string Email { get; init; }
    public required string Password { get; init; }
}
