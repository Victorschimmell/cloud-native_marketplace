using Backend.Api.Contracts.Common;
using Backend.Api.Contracts.Operation.AuditLog;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Backend.Api.Attributes;

namespace Backend.Api.Controllers.Operations;

[Route("api/admin/audit-logs")]
public class AuditLogController : ApiControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IAuditLogService _auditLogService;

    public AuditLogController(IAdminService adminService, IAuditLogService auditLogService)
    {
        _adminService = adminService;
        _auditLogService = auditLogService;
    }

    [HttpGet("{actorUserId:guid}")]
    public async Task<ActionResult<PageResponse<AuditLogEntryResponse>>> GetByActorUserAsync([NotEmptyGuid] Guid actorUserId, [FromQuery] PageRequest pageRequest, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }

    [HttpGet("{entityType}/{entityId}")]
    public async Task<ActionResult<IReadOnlyList<AuditLogEntryResponse>>> GetByTargetEntityAsync(string entityType, string entityId, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }

    [HttpPost]
    public async Task<ActionResult<AuditLogEntryResponse>> WriteEntryAsync([FromBody] WriteAuditLogEntryRequest request, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }
}
