using Backend.Domain.Enums;

namespace Backend.Application.DTOs;

public sealed record AdminIssueDto(
    Guid Id,
    string Title,
    string Description,
    IssueType Type,
    IssuePriority Priority,
    IssueStatus Status,
    Guid ReportedByUserId,
    string? ReportedByDisplay,
    Guid? AssignedToUserId,
    string? AssignedToDisplay,
    Guid? ResolvedByUserId,
    DateTimeOffset? ResolvedAtUtc,
    string? Resolution,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record CreateAdminIssueRequest(
    string Title,
    string Description,
    IssueType Type,
    IssuePriority Priority);

public sealed record ResolveAdminIssueRequest(Guid IssueId, string? Resolution);

public sealed record AssignAdminIssueRequest(Guid IssueId);

public sealed record GetAdminIssuesRequest(
    IssueStatus? Status,
    IssuePriority? Priority,
    bool UnresolvedOnly = false,
    int Page = 1,
    int PageSize = 20);
