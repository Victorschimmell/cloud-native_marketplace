namespace Backend.Api.Contracts.Operation.Admin;

public sealed record AdminUnblockUserRequest
{
    public string? Reason { get; init; }
}
