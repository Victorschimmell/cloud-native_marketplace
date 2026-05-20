using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Contracts.Operation.Issues;

public sealed record CreateIssueRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public required string Title { get; init; }

    [Required]
    [StringLength(4000, MinimumLength = 1)]
    public required string Description { get; init; }

    public required IssueType Type { get; init; }
    public required IssuePriority Priority { get; init; }
}
