namespace Backend.Api.Contracts.User.Auth;

public sealed record LoginResponse
{
    public required UserAccountModel User { get; init; }
    public required AuthToken Token { get; init; }
}
