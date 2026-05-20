using Backend.Api.Attributes;
using Backend.Api.Contracts.Common;
using Backend.Api.Contracts.Operation.Admin;
using Backend.Api.Contracts.User.SellerVerification;
using Backend.Api.Mappings.User.SellerVerification;
using Backend.Application.Common.Abstractions;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using App = Backend.Application.DTOs;
using ApplicationRepositories = Backend.Application.Abstractions.Repositories;

namespace Backend.Api.Controllers.Operations;

[Route("api/admin")]
[Authorize]
public class AdminController : ApiControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ISellerVerificationService _sellerVerificationService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public AdminController(
        IAdminService adminService,
        ISellerVerificationService sellerVerificationService,
        ICurrentUserProvider currentUserProvider)
    {
        _adminService = adminService;
        _sellerVerificationService = sellerVerificationService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpPost("users/{userId:guid}/block")]
    public async Task<ActionResult<AdminOperationResponse>> BlockUser(
        [FromRoute][NotEmptyGuid(ErrorMessage = "UserId cannot be an empty GUID.")] Guid userId,
        [FromBody] AdminBlockUserRequest request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserProvider.IsAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { Error = "Only admins can block users." });
        }

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
        if (!_currentUserProvider.IsAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { Error = "Only admins can unblock users." });
        }

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
        if (!_currentUserProvider.IsAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { Error = "Only admins can view seller verifications." });
        }

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
        if (!_currentUserProvider.IsAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { Error = "Only admins can verify sellers." });
        }

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
        if (!_currentUserProvider.IsAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { Error = "Only admins can list users." });
        }

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
        if (!_currentUserProvider.IsAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { Error = "Only admins can list payments." });
        }

        var applicationRequest = new App.GetAdminPaymentsRequest(
            pageRequest.Page,
            pageRequest.PageSize,
            currency,
            ParsePaymentStatusFilter(filters.Status));
        var result = await _adminService.GetPaymentsAsync(applicationRequest, cancellationToken);
        return HandleResult(result, page => new PageResponse<AdminPaymentResponse>
        {
            Items = page.Items.Select(ToPaymentResponse).ToArray(),
            Page = page.Page,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount
        });
    }

    private static ApplicationRepositories.AdminPaymentStatusFilter ParsePaymentStatusFilter(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return ApplicationRepositories.AdminPaymentStatusFilter.Any;
        }

        return status.Trim().ToLowerInvariant() switch
        {
            "all" or "any" => ApplicationRepositories.AdminPaymentStatusFilter.Any,
            "completed" => ApplicationRepositories.AdminPaymentStatusFilter.Completed,
            "pending" => ApplicationRepositories.AdminPaymentStatusFilter.Pending,
            "failed" => ApplicationRepositories.AdminPaymentStatusFilter.Failed,
            _ => ApplicationRepositories.AdminPaymentStatusFilter.Any
        };
    }

    [HttpGet("dashboard/stats")]
    public async Task<ActionResult<DashboardStatsResponse>> GetDashboardStats(
        [FromQuery] string? currency,
        CancellationToken cancellationToken)
    {
        if (!_currentUserProvider.IsAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { Error = "Only admins can view dashboard stats." });
        }

        var applicationRequest = new App.GetDashboardStatsRequest(currency);
        var result = await _adminService.GetDashboardStatsAsync(applicationRequest, cancellationToken);
        return HandleResult(result, dto => new DashboardStatsResponse
        {
            ActiveUsers = dto.ActiveUsers,
            OrdersInLast24Hours = dto.OrdersInLast24Hours,
            TotalRevenue = dto.TotalRevenue,
            CurrencyCode = dto.CurrencyCode,
            OpenIssues = dto.OpenIssues,
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
