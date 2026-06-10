using Backend.Api.Contracts.Operation.Issues;
using App = Backend.Application.DTOs;

namespace Backend.Api.Mappings.Operation.Issues;

public static class IssueMappingExtensions
{
    public static App.CreateAdminIssueRequest ToApplicationRequest(this CreateIssueRequest request) =>
        new(
            request.Title,
            request.Description,
            request.Type,
            request.Priority);

    public static App.ResolveAdminIssueRequest ToApplicationRequest(this ResolveIssueRequest request, Guid issueId) =>
        new(issueId, request.Resolution);

    public static App.GetAdminIssuesRequest ToApplicationRequest(this GetIssuesRequest request, int page, int pageSize) =>
        new(
            request.Status,
            request.Priority,
            request.UnresolvedOnly,
            page,
            pageSize);

    public static IssueResponse ToResponse(this App.AdminIssueDto dto) =>
        new()
        {
            Id = dto.Id,
            Title = dto.Title,
            Description = dto.Description,
            Type = dto.Type,
            Priority = dto.Priority,
            Status = dto.Status,
            ReportedByUserId = dto.ReportedByUserId,
            ReportedByDisplay = dto.ReportedByDisplay,
            AssignedToUserId = dto.AssignedToUserId,
            AssignedToDisplay = dto.AssignedToDisplay,
            ResolvedByUserId = dto.ResolvedByUserId,
            ResolvedAtUtc = dto.ResolvedAtUtc,
            Resolution = dto.Resolution,
            CreatedAtUtc = dto.CreatedAtUtc,
            UpdatedAtUtc = dto.UpdatedAtUtc
        };
}
