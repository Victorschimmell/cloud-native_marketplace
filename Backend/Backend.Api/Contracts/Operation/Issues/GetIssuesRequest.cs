namespace Backend.Api.Contracts.Operation.Issues;

public sealed record GetIssuesRequest
{
    public IssueStatus? Status { get; init; }
    public IssuePriority? Priority { get; init; }
}
