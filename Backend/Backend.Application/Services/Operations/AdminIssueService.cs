using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Entities.Operations;
using Backend.Domain.Enums;
namespace Backend.Application.Services;

public sealed class AdminIssueService : IAdminIssueService
{
    private readonly IAdminIssueRepository _issueRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuditLogService _auditLogService;
    private readonly IUnitOfWork _unitOfWork;

    public AdminIssueService(
        IAdminIssueRepository issueRepository,
        ICurrentUserProvider currentUserProvider,
        IDateTimeProvider dateTimeProvider,
        IAuditLogService auditLogService,
        IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(issueRepository);
        ArgumentNullException.ThrowIfNull(currentUserProvider);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        ArgumentNullException.ThrowIfNull(auditLogService);
        ArgumentNullException.ThrowIfNull(unitOfWork);

        _issueRepository = issueRepository;
        _currentUserProvider = currentUserProvider;
        _dateTimeProvider = dateTimeProvider;
        _auditLogService = auditLogService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AdminIssueDto>> CreateAsync(CreateAdminIssueRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUserProvider.IsAdmin || _currentUserProvider.UserId is null)
        {
            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: AuditActionType.Created,
                TargetEntityType: nameof(AdminIssue),
                TargetEntityId: "new",
                Outcome: AuditOutcome.Forbidden,
                Details: "Non-admin attempted to create an issue."
            ), cancellationToken);

            return Result<AdminIssueDto>.Forbidden("Only admins can create issues.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: AuditActionType.Created,
                TargetEntityType: nameof(AdminIssue),
                TargetEntityId: "new",
                Outcome: AuditOutcome.Failed,
                Details: "Issue creation failed: title is required."
            ), cancellationToken);

            return Result<AdminIssueDto>.ValidationFailure("Title is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: AuditActionType.Created,
                TargetEntityType: nameof(AdminIssue),
                TargetEntityId: "new",
                Outcome: AuditOutcome.Failed,
                Details: "Issue creation failed: description is required."
            ), cancellationToken);

            return Result<AdminIssueDto>.ValidationFailure("Description is required.");
        }

        var issue = new AdminIssue
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Type = request.Type,
            Priority = request.Priority,
            Status = IssueStatus.Open,
            ReportedByUserId = _currentUserProvider.UserId.Value
        };

        await _issueRepository.AddAsync(issue, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
            ActionType: AuditActionType.Created,
            TargetEntityType: nameof(AdminIssue),
            TargetEntityId: issue.Id.ToString(),
            Outcome: AuditOutcome.Succeeded,
            Details: $"Issue '{issue.Title}' created with priority {issue.Priority}."
        ), cancellationToken);

        return Result<AdminIssueDto>.Success(issue.ToAdminIssueDto());
    }

    public async Task<Result<AdminIssueDto>> ResolveAsync(ResolveAdminIssueRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUserProvider.IsAdmin || _currentUserProvider.UserId is null)
        {
            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: AuditActionType.Updated,
                TargetEntityType: nameof(AdminIssue),
                TargetEntityId: request.IssueId.ToString(),
                Outcome: AuditOutcome.Forbidden,
                Details: "Non-admin attempted to resolve an issue."
            ), cancellationToken);

            return Result<AdminIssueDto>.Forbidden("Only admins can resolve issues.");
        }

        var issue = await _issueRepository.GetByIdAsync(request.IssueId, cancellationToken);
        if (issue is null)
        {
            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: AuditActionType.Updated,
                TargetEntityType: nameof(AdminIssue),
                TargetEntityId: request.IssueId.ToString(),
                Outcome: AuditOutcome.Failed,
                Details: "Resolve failed: issue not found."
            ), cancellationToken);

            return Result<AdminIssueDto>.NotFound("Issue not found.");
        }

        if (issue.Status == IssueStatus.Resolved)
        {
            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: AuditActionType.Updated,
                TargetEntityType: nameof(AdminIssue),
                TargetEntityId: issue.Id.ToString(),
                Outcome: AuditOutcome.Failed,
                Details: "Resolve failed: issue is already resolved."
            ), cancellationToken);

            return Result<AdminIssueDto>.Conflict("Issue is already resolved.");
        }

        issue.Status = IssueStatus.Resolved;
        issue.ResolvedByUserId = _currentUserProvider.UserId.Value;
        issue.ResolvedAtUtc = _dateTimeProvider.UtcNow;
        issue.Resolution = string.IsNullOrWhiteSpace(request.Resolution) ? null : request.Resolution.Trim();

        await _issueRepository.UpdateAsync(issue, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
            ActionType: AuditActionType.Updated,
            TargetEntityType: nameof(AdminIssue),
            TargetEntityId: issue.Id.ToString(),
            Outcome: AuditOutcome.Succeeded,
            Details: $"Issue '{issue.Title}' resolved."
        ), cancellationToken);

        return Result<AdminIssueDto>.Success(issue.ToAdminIssueDto());
    }

    public async Task<Result<AdminIssueDto>> AssignAsync(AssignAdminIssueRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUserProvider.IsAdmin)
        {
            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: AuditActionType.Updated,
                TargetEntityType: nameof(AdminIssue),
                TargetEntityId: request.IssueId.ToString(),
                Outcome: AuditOutcome.Forbidden,
                Details: "Non-admin attempted to assign an issue."
            ), cancellationToken);

            return Result<AdminIssueDto>.Forbidden("Only admins can assign issues.");
        }

        var issue = await _issueRepository.GetByIdAsync(request.IssueId, cancellationToken);
        if (issue is null)
        {
            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: AuditActionType.Updated,
                TargetEntityType: nameof(AdminIssue),
                TargetEntityId: request.IssueId.ToString(),
                Outcome: AuditOutcome.Failed,
                Details: "Assign failed: issue not found."
            ), cancellationToken);

            return Result<AdminIssueDto>.NotFound("Issue not found.");
        }

        if (issue.Status == IssueStatus.Resolved)
        {
            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: AuditActionType.Updated,
                TargetEntityType: nameof(AdminIssue),
                TargetEntityId: issue.Id.ToString(),
                Outcome: AuditOutcome.Failed,
                Details: "Assign failed: resolved issues cannot be reassigned."
            ), cancellationToken);

            return Result<AdminIssueDto>.Conflict("Resolved issues cannot be reassigned.");
        }

        issue.AssignedToUserId = request.AssigneeUserId;
        issue.Status = IssueStatus.InProgress;

        await _issueRepository.UpdateAsync(issue, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
            ActionType: AuditActionType.Updated,
            TargetEntityType: nameof(AdminIssue),
            TargetEntityId: issue.Id.ToString(),
            Outcome: AuditOutcome.Succeeded,
            Details: $"Issue '{issue.Title}' assigned to user {request.AssigneeUserId}."
        ), cancellationToken);

        return Result<AdminIssueDto>.Success(issue.ToAdminIssueDto());
    }

    public async Task<Result<AdminIssueDto>> GetByIdAsync(Guid issueId, CancellationToken cancellationToken = default)
    {
        if (!_currentUserProvider.IsAdmin)
        {
            return Result<AdminIssueDto>.Forbidden("Only admins can view issues.");
        }

        var issue = await _issueRepository.GetByIdAsync(issueId, cancellationToken);
        if (issue is null)
        {
            return Result<AdminIssueDto>.NotFound("Issue not found.");
        }

        return Result<AdminIssueDto>.Success(issue.ToAdminIssueDto());
    }

    public async Task<Result<PagedResult<AdminIssueDto>>> GetIssuesAsync(GetAdminIssuesRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUserProvider.IsAdmin)
        {
            return Result<PagedResult<AdminIssueDto>>.Forbidden("Only admins can view issues.");
        }

        if (request.Page <= 0 || request.PageSize <= 0)
        {
            return Result<PagedResult<AdminIssueDto>>.ValidationFailure("Page and PageSize must be greater than 0.");
        }

        var page = await _issueRepository.GetByFilterAsync(request.Status, request.Priority, request.Page, request.PageSize, cancellationToken);
        var items = page.Items.Select(issue => issue.ToAdminIssueDto()).ToList();
        return Result<PagedResult<AdminIssueDto>>.Success(new PagedResult<AdminIssueDto>(items, page.Page, page.PageSize, page.TotalCount));
    }
}
