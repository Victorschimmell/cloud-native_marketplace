namespace Backend.Api.Contracts.User.Auth;

public sealed record AuthToken
{
    public required string AccessToken { get; init; }
    public required DateTimeOffset ExpiresAtUtc { get; init; }
    public string? RefreshToken { get; init; }
}
