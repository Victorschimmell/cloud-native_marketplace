using Backend.Api.Contracts.Common;
using Backend.Api.Contracts.Operation.AuditLog;
using Backend.Api.Mappings.Operation.AuditLog;
using Backend.Application.Common.Abstractions;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers.Operations;

[Route("api/admin/audit-logs")]
[Authorize]
public class AuditLogController : ApiControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public AuditLogController(IAdminService adminService, ICurrentUserProvider currentUserProvider)
    {
        _adminService = adminService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet]
    public async Task<ActionResult<PageResponse<AuditLogEntryResponse>>> GetAuditLogsAsync([FromQuery] GetAuditLogsRequest request, [FromQuery] PageRequest pageRequest, CancellationToken cancellationToken)
    {
        if (!_currentUserProvider.IsAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { Error = "Only admins can access audit logs." });
        }

        var result = await _adminService.GetAuditLogsAsync(request.ToApplicationRequest(pageRequest), cancellationToken);
        return HandleResult(
            result,
            page => new PageResponse<AuditLogEntryResponse>
            {
                Items = page.Items.Select(entry => entry.ToResponse()).ToArray(),
                Page = page.Page,
                PageSize = page.PageSize,
                TotalCount = page.TotalCount
            });
    }
}
