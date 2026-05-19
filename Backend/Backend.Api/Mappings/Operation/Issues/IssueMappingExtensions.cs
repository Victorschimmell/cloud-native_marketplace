using Backend.Api.Contracts.Operation.Issues;
using App = Backend.Application.DTOs;

namespace Backend.Api.Mappings.Operation.Issues;

public static class IssueMappingExtensions
{
    public static App.CreateAdminIssueRequest ToApplicationRequest(this CreateIssueRequest request) =>
        new(
            request.Title,
            request.Description,
            (Backend.Domain.Enums.IssueType)request.Type,
            (Backend.Domain.Enums.IssuePriority)request.Priority);

    public static App.ResolveAdminIssueRequest ToApplicationRequest(this ResolveIssueRequest request, Guid issueId) =>
        new(issueId, request.Resolution);

    public static App.GetAdminIssuesRequest ToApplicationRequest(this GetIssuesRequest request, int page, int pageSize) =>
        new(
            request.Status.HasValue ? (Backend.Domain.Enums.IssueStatus)request.Status.Value : null,
            request.Priority.HasValue ? (Backend.Domain.Enums.IssuePriority)request.Priority.Value : null,
            page,
            pageSize);

    public static IssueResponse ToResponse(this App.AdminIssueDto dto) =>
        new()
        {
            Id = dto.Id,
            Title = dto.Title,
            Description = dto.Description,
            Type = (IssueType)dto.Type,
            Priority = (IssuePriority)dto.Priority,
            Status = (IssueStatus)dto.Status,
            ReportedByUserId = dto.ReportedByUserId,
            ReportedByDisplay = dto.ReportedByDisplay,
            AssignedToUserId = dto.AssignedToUserId,
            ResolvedByUserId = dto.ResolvedByUserId,
            ResolvedAtUtc = dto.ResolvedAtUtc,
            Resolution = dto.Resolution,
            CreatedAtUtc = dto.CreatedAtUtc,
            UpdatedAtUtc = dto.UpdatedAtUtc
        };
}
