using Backend.Domain.Base;
using Backend.Domain.Enums;

namespace Backend.Domain.Entities.Operations;

public sealed class AdminIssue : AggregateRoot<Guid>
{
    public AdminIssue()
    {
        Id = Guid.NewGuid();
    }

    public required string Title { get; set; }
    public required string Description { get; set; }
    public IssueType Type { get; set; }
    public IssuePriority Priority { get; set; }
    public IssueStatus Status { get; set; }
    public Guid ReportedByUserId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid? ResolvedByUserId { get; set; }
    public DateTimeOffset? ResolvedAtUtc { get; set; }
    public string? Resolution { get; set; }

    public IdentityAccess.UserAccount? ReportedByUser { get; set; }
    public IdentityAccess.UserAccount? AssignedToUser { get; set; }
    public IdentityAccess.UserAccount? ResolvedByUser { get; set; }
}
