namespace Backend.Api.Contracts.Operation.Issues;

public sealed record IssueResponse
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required IssueType Type { get; init; }
    public required IssuePriority Priority { get; init; }
    public required IssueStatus Status { get; init; }
    public required Guid ReportedByUserId { get; init; }
    public string? ReportedByDisplay { get; init; }
    public Guid? AssignedToUserId { get; init; }
    public string? AssignedToDisplay { get; init; }
    public Guid? ResolvedByUserId { get; init; }
    public DateTimeOffset? ResolvedAtUtc { get; init; }
    public string? Resolution { get; init; }
    public required DateTimeOffset CreatedAtUtc { get; init; }
    public required DateTimeOffset UpdatedAtUtc { get; init; }
}
