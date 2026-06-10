namespace Backend.Api.Contracts.User.Auth;

public sealed record UserAccountModel
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required bool IsAdmin { get; init; }
    public required bool IsBlocked { get; init; }
    public required AccountStatus AccountStatus { get; init; }
    public DateTimeOffset? LastLoginAtUtc { get; init; }
}