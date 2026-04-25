using Backend.Api.Attributes;
using Backend.Api.Contracts.Common;
using Backend.Api.Contracts.Operation.Admin;
using Backend.Api.Contracts.Operation.AuditLog;
using Backend.Api.Contracts.User.SellerVerification;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers.Operations;

[Route("api/admin")]
public class AdminController : ApiControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ISellerVerificationService _sellerVerificationService;

    public AdminController(IAdminService adminService, ISellerVerificationService sellerVerificationService)
    {
        _adminService = adminService;
        _sellerVerificationService = sellerVerificationService;
    }

    [HttpPost("users/{userId:guid}/block")]
    public async Task<ActionResult<AdminOperationResponse>> BlockUser([FromRoute][NotEmptyGuid(ErrorMessage = "UserId cannot be an empty GUID.")] Guid userId, [FromBody] AdminBlockUserRequest request, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }

    [HttpPost("users/{userId:guid}/unblock")]
    public async Task<ActionResult<AdminOperationResponse>> UnblockUser([FromRoute][NotEmptyGuid(ErrorMessage = "UserId cannot be an empty GUID.")] Guid userId, [FromBody] AdminUnblockUserRequest request, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }

    [HttpGet("audit-logs")]
    public async Task<ActionResult<PageResponse<AuditLogEntryResponse>>> GetAuditLogs([FromQuery] GetAuditLogsRequest request, [FromQuery] PageRequest pageRequest, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }

    [HttpGet("sellers/verifications")]
    // TODO: Return type
    public async Task<IActionResult> GetSellerVerifications([FromQuery] PageRequest pageRequest, CancellationToken cancellationToken)
    {
        // NOTE: Application layer ISellerVerificationService does not implement this function yet
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }

    [HttpPost("sellers/{sellerId:guid}/verify")]
    public async Task<ActionResult<SellerVerificationSubmissionResponse>> VerifySeller([NotEmptyGuid] Guid sellerId, [FromBody] VerifySellerRequest request, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }
}
