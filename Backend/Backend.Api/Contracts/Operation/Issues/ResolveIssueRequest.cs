using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Contracts.Operation.Issues;

public sealed record ResolveIssueRequest
{
    [StringLength(4000)]
    public string? Resolution { get; init; }
}
