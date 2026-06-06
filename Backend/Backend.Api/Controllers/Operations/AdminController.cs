using Backend.Api.Attributes;
using Backend.Api.Auth;
using Backend.Api.Contracts.Common;
using Backend.Api.Contracts.Operation.Admin;
using Backend.Api.Contracts.User.SellerVerification;
using Backend.Api.Mappings.User.SellerVerification;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using App = Backend.Application.DTOs;
using ApplicationRepositories = Backend.Application.Abstractions.Repositories;

namespace Backend.Api.Controllers.Operations;

[Route("api/admin")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class AdminController : ApiControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IAdminDashboardService _adminDashboardService;
    private readonly ISellerVerificationService _sellerVerificationService;

    public AdminController(
        IAdminService adminService,
        IAdminDashboardService adminDashboardService,
        ISellerVerificationService sellerVerificationService)
    {
        _adminService = adminService;
        _adminDashboardService = adminDashboardService;
        _sellerVerificationService = sellerVerificationService;
    }

    [HttpPost("users/{userId:guid}/block")]
    public async Task<ActionResult<AdminOperationResponse>> BlockUser(
        [FromRoute][NotEmptyGuid(ErrorMessage = "UserId cannot be an empty GUID.")] Guid userId,
        [FromBody] AdminBlockUserRequest request,
        CancellationToken cancellationToken)
    {
        var applicationRequest = new App.AdminBlockUserRequest(userId, request.Reason);
        var result = await _adminService.BlockUserAsync(applicationRequest, cancellationToken);
        return HandleResult(result, dto => new AdminOperationResponse
        {
            UserId = dto.UserId,
            Operation = dto.Operation,
            Message = dto.Message
        });
    }

    [HttpPost("users/{userId:guid}/unblock")]
    public async Task<ActionResult<AdminOperationResponse>> UnblockUser(
        [FromRoute][NotEmptyGuid(ErrorMessage = "UserId cannot be an empty GUID.")] Guid userId,
        [FromBody] AdminUnblockUserRequest request,
        CancellationToken cancellationToken)
    {
        var applicationRequest = new App.AdminUnblockUserRequest(userId, request.Reason);
        var result = await _adminService.UnblockUserAsync(applicationRequest, cancellationToken);
        return HandleResult(result, dto => new AdminOperationResponse
        {
            UserId = dto.UserId,
            Operation = dto.Operation,
            Message = dto.Message
        });
    }

    [HttpGet("sellers/verifications")]
    public async Task<ActionResult<PageResponse<SellerVerificationRequestDetailsResponse>>> GetSellerVerifications(
        [FromQuery] PageRequest pageRequest,
        CancellationToken cancellationToken)
    {
        var result = await _sellerVerificationService.GetAllRequestsAsync(pageRequest.Page, pageRequest.PageSize, cancellationToken);
        return HandleResult(result, page => new PageResponse<SellerVerificationRequestDetailsResponse>
        {
            Items = page.Items.Select(item => item.ToDetailsResponse()).ToArray(),
            Page = page.Page,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount
        });
    }

    [HttpPost("sellers/{sellerId:guid}/verify")]
    public async Task<ActionResult<SellerVerificationSubmissionResponse>> VerifySeller(
        [NotEmptyGuid] Guid sellerId,
        [FromBody] VerifySellerRequest request,
        CancellationToken cancellationToken)
    {
        var applicationRequest = request.ToApplicationRequest(sellerId);
        var result = await _sellerVerificationService.VerifySellerAsync(applicationRequest, cancellationToken);
        return HandleResult(result, response => response.ToSubmissionResponse());
    }

    [HttpGet("users")]
    public async Task<ActionResult<PageResponse<AdminUserResponse>>> GetUsers(
        [FromQuery] GetAdminUsersRequest filters,
        [FromQuery] PageRequest pageRequest,
        CancellationToken cancellationToken)
    {
        var applicationRequest = new App.GetAdminUsersRequest(
            (Application.Abstractions.Repositories.AdminUserRoleFilter)filters.Role,
            (Application.Abstractions.Repositories.AdminUserStatusFilter)filters.Status,
            pageRequest.Page,
            pageRequest.PageSize);
        var result = await _adminService.GetUsersAsync(applicationRequest, cancellationToken);
        return HandleResult(result, page => new PageResponse<AdminUserResponse>
        {
            Items = page.Items.Select(ToUserResponse).ToArray(),
            Page = page.Page,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount
        });
    }

    [HttpGet("payments")]
    public async Task<ActionResult<PageResponse<AdminPaymentResponse>>> GetPayments(
        [FromQuery] GetAdminPaymentsRequest filters,
        [FromQuery] PageRequest pageRequest,
        [FromQuery] string? currency,
        CancellationToken cancellationToken)
    {
        if (!TryParsePaymentStatusFilter(filters.Status, out var statusFilter))
        {
            return BadRequest(new { Error = $"Unsupported payment status filter '{filters.Status}'." });
        }

        var applicationRequest = new App.GetAdminPaymentsRequest(
            pageRequest.Page,
            pageRequest.PageSize,
            currency,
            statusFilter);
        var result = await _adminService.GetPaymentsAsync(applicationRequest, cancellationToken);
        return HandleResult(result, page => new PageResponse<AdminPaymentResponse>
        {
            Items = page.Items.Select(ToPaymentResponse).ToArray(),
            Page = page.Page,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount
        });
    }

    private static bool TryParsePaymentStatusFilter(string? status, out ApplicationRepositories.AdminPaymentStatusFilter filter)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            filter = ApplicationRepositories.AdminPaymentStatusFilter.Any;
            return true;
        }

        var normalizedStatus = status.Trim().ToLowerInvariant();
        filter = normalizedStatus switch
        {
            "all" or "any" => ApplicationRepositories.AdminPaymentStatusFilter.Any,
            "completed" => ApplicationRepositories.AdminPaymentStatusFilter.Completed,
            "pending" => ApplicationRepositories.AdminPaymentStatusFilter.Pending,
            "failed" => ApplicationRepositories.AdminPaymentStatusFilter.Failed,
            _ => ApplicationRepositories.AdminPaymentStatusFilter.Any
        };

        return normalizedStatus is "all" or "any" or "completed" or "pending" or "failed";
    }

    [HttpGet("dashboard/stats")]
    public async Task<ActionResult<DashboardStatsResponse>> GetDashboardStats(
        [FromQuery] string? currency,
        CancellationToken cancellationToken)
    {
        var applicationRequest = new App.GetDashboardStatsRequest(currency);
        var result = await _adminDashboardService.GetDashboardStatsAsync(applicationRequest, cancellationToken);
        return HandleResult(result, dto => new DashboardStatsResponse
        {
            ActiveUsers = dto.ActiveUsers,
            OrdersInLast24Hours = dto.OrdersInLast24Hours,
            TotalRevenue = dto.TotalRevenue,
            CurrencyCode = dto.CurrencyCode,
            UnresolvedIssues = dto.UnresolvedIssues,
            GeneratedAtUtc = dto.GeneratedAtUtc
        });
    }

    private static AdminUserResponse ToUserResponse(App.AdminUserDto dto) => new()
    {
        Id = dto.Id,
        Email = dto.Email,
        Name = dto.DisplayName,
        Role = dto.Role,
        Status = dto.Status,
        Company = dto.Company,
        SellerId = dto.SellerId,
        PendingVerificationRequestId = dto.PendingVerificationRequestId,
        RegisteredOn = dto.RegisteredAtUtc,
        LastLoginAtUtc = dto.LastLoginAtUtc
    };

    private static AdminPaymentResponse ToPaymentResponse(App.AdminPaymentDto dto) => new()
    {
        Id = dto.DisplayId,
        OrderId = dto.OrderId,
        PaymentSequential = dto.PaymentSequential,
        CustomerName = dto.CustomerName,
        Date = dto.Date,
        Amount = dto.Amount,
        CurrencyCode = dto.CurrencyCode,
        Status = dto.Status
    };
}
