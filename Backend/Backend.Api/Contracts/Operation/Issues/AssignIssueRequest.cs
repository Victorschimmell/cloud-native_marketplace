using Backend.Api.Attributes;

namespace Backend.Api.Contracts.Operation.Issues;

public sealed record AssignIssueRequest
{
    [NotEmptyGuid]
    public required Guid AssigneeUserId { get; init; }
}
