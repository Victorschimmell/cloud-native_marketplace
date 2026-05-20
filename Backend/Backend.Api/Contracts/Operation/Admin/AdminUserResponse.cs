namespace Backend.Api.Contracts.Operation.Admin;

public sealed record AdminUserResponse
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required string Name { get; init; }
    public required string Role { get; init; }
    public required string Status { get; init; }
    public string? Company { get; init; }
    public Guid? SellerId { get; init; }
    public Guid? PendingVerificationRequestId { get; init; }
    public required DateTimeOffset RegisteredOn { get; init; }
    public DateTimeOffset? LastLoginAtUtc { get; init; }
}
