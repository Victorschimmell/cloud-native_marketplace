namespace Backend.Api.Contracts.Operation.Admin;

public sealed record AdminBlockUserRequest
{
    public string? Reason { get; init; }
}
