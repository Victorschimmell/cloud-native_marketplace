using Backend.Api.Attributes;
using Backend.Api.Contracts.Common;
using Backend.Api.Contracts.Operation.Issues;
using Backend.Api.Mappings.Operation.Issues;
using Backend.Application.Common.Abstractions;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using App = Backend.Application.DTOs;

namespace Backend.Api.Controllers.Operations;

[Route("api/admin/issues")]
[Authorize]
public class AdminIssuesController : ApiControllerBase
{
    private readonly IAdminIssueService _issueService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public AdminIssuesController(IAdminIssueService issueService, ICurrentUserProvider currentUserProvider)
    {
        _issueService = issueService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet]
    public async Task<ActionResult<PageResponse<IssueResponse>>> GetIssues(
        [FromQuery] GetIssuesRequest request,
        [FromQuery] PageRequest pageRequest,
        CancellationToken cancellationToken)
    {
        if (!_currentUserProvider.IsAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { Error = "Only admins can view issues." });
        }

        var applicationRequest = request.ToApplicationRequest(pageRequest.Page, pageRequest.PageSize);
        var result = await _issueService.GetIssuesAsync(applicationRequest, cancellationToken);
        return HandleResult(result, page => new PageResponse<IssueResponse>
        {
            Items = page.Items.Select(item => item.ToResponse()).ToArray(),
            Page = page.Page,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount
        });
    }

    [HttpGet("{issueId:guid}")]
    public async Task<ActionResult<IssueResponse>> GetIssue(
        [FromRoute][NotEmptyGuid] Guid issueId,
        CancellationToken cancellationToken)
    {
        if (!_currentUserProvider.IsAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { Error = "Only admins can view issues." });
        }

        var result = await _issueService.GetByIdAsync(issueId, cancellationToken);
        return HandleResult(result, dto => dto.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<IssueResponse>> CreateIssue(
        [FromBody] CreateIssueRequest request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserProvider.IsAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { Error = "Only admins can create issues." });
        }

        var applicationRequest = request.ToApplicationRequest();
        var result = await _issueService.CreateAsync(applicationRequest, cancellationToken);
        return HandleResult(result, dto => dto.ToResponse());
    }

    [HttpPost("{issueId:guid}/resolve")]
    public async Task<ActionResult<IssueResponse>> ResolveIssue(
        [FromRoute][NotEmptyGuid] Guid issueId,
        [FromBody] ResolveIssueRequest request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserProvider.IsAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { Error = "Only admins can resolve issues." });
        }

        var applicationRequest = request.ToApplicationRequest(issueId);
        var result = await _issueService.ResolveAsync(applicationRequest, cancellationToken);
        return HandleResult(result, dto => dto.ToResponse());
    }

    [HttpPost("{issueId:guid}/assign")]
    public async Task<ActionResult<IssueResponse>> AssignIssue(
        [FromRoute][NotEmptyGuid] Guid issueId,
        CancellationToken cancellationToken)
    {
        if (!_currentUserProvider.IsAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { Error = "Only admins can assign issues." });
        }

        var applicationRequest = new App.AssignAdminIssueRequest(issueId);
        var result = await _issueService.AssignAsync(applicationRequest, cancellationToken);
        return HandleResult(result, dto => dto.ToResponse());
    }
}
