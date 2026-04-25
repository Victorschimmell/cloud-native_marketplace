namespace Backend.Api.Contracts.Operation.Admin;

public sealed record AdminOperationResponse
{
    public required Guid UserId { get; init; }
    public required string Operation { get; init; }
    public string? Message { get; init; }
}
