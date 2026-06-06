using Backend.Api.Auth;
using Backend.Api.Contracts.Common;
using Backend.Api.Contracts.Operation.AuditLog;
using Backend.Api.Mappings.Operation.AuditLog;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers.Operations;

[Route("api/admin/audit-logs")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class AuditLogController : ApiControllerBase
{
    private readonly IAdminService _adminService;

    public AuditLogController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<ActionResult<PageResponse<AuditLogEntryResponse>>> GetAuditLogsAsync([FromQuery] GetAuditLogsRequest request, [FromQuery] PageRequest pageRequest, CancellationToken cancellationToken)
    {
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
