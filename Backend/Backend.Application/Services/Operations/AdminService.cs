using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Entities.IdentityAccess;
using Backend.Domain.Entities.Operations;
using Backend.Domain.Entities.Orders;
using Backend.Domain.Enums;
namespace Backend.Application.Services;

public sealed class AdminService : IAdminService
{
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IAdminIssueRepository _adminIssueRepository;
    private readonly ICurrencyConversionService _currencyConversionService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IUnitOfWork _unitOfWork;

    public AdminService(
        IUserAccountRepository userAccountRepository,
        IAuditLogRepository auditLogRepository,
        IAuditLogService auditLogService,
        IPaymentRepository paymentRepository,
        IOrderRepository orderRepository,
        IAdminIssueRepository adminIssueRepository,
        ICurrencyConversionService currencyConversionService,
        IDateTimeProvider dateTimeProvider,
        ICurrentUserProvider currentUserProvider,
        IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(userAccountRepository);
        ArgumentNullException.ThrowIfNull(auditLogRepository);
        ArgumentNullException.ThrowIfNull(auditLogService);
        ArgumentNullException.ThrowIfNull(paymentRepository);
        ArgumentNullException.ThrowIfNull(orderRepository);
        ArgumentNullException.ThrowIfNull(adminIssueRepository);
        ArgumentNullException.ThrowIfNull(currencyConversionService);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        ArgumentNullException.ThrowIfNull(currentUserProvider);
        ArgumentNullException.ThrowIfNull(unitOfWork);

        _userAccountRepository = userAccountRepository;
        _auditLogRepository = auditLogRepository;
        _auditLogService = auditLogService;
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _adminIssueRepository = adminIssueRepository;
        _currencyConversionService = currencyConversionService;
        _dateTimeProvider = dateTimeProvider;
        _currentUserProvider = currentUserProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AdminOperationResponse>> BlockUserAsync(AdminBlockUserRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUserProvider.IsAdmin)
        {
            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: AuditActionType.Block,
                TargetEntityType: nameof(UserAccount),
                TargetEntityId: request.UserId.ToString(),
                Outcome: AuditOutcome.Forbidden,
                Details: "Non-admin attempted to block user."
            ), cancellationToken);

            return Result<AdminOperationResponse>.Forbidden("Only admins can block users.");
        }

        var user = await _userAccountRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: AuditActionType.Block,
                TargetEntityType: nameof(UserAccount),
                TargetEntityId: request.UserId.ToString(),
                Outcome: AuditOutcome.Failed,
                Details: "Block failed: user not found."
            ), cancellationToken);

            return Result<AdminOperationResponse>.NotFound("User not found.");
        }

        if (user.IsBlocked)
        {
            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: AuditActionType.Block,
                TargetEntityType: nameof(UserAccount),
                TargetEntityId: user.Id.ToString(),
                Outcome: AuditOutcome.Failed,
                Details: "Block failed: user is already blocked."
            ), cancellationToken);

            return Result<AdminOperationResponse>.Conflict("User is already blocked.");
        }

        user.IsBlocked = true;
        user.AccountStatus = AccountStatus.Suspended;

        await _userAccountRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var details = string.IsNullOrWhiteSpace(request.Reason)
            ? $"User {user.Id} blocked."
            : $"User {user.Id} blocked. Reason: {request.Reason}";

        await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
            ActionType: AuditActionType.Block,
            TargetEntityType: nameof(UserAccount),
            TargetEntityId: user.Id.ToString(),
            Outcome: AuditOutcome.Succeeded,
            Details: details
        ), cancellationToken);

        return Result<AdminOperationResponse>.Success(new AdminOperationResponse(
            user.Id,
            Operation: "Block",
            Message: "User has been blocked."));
    }

    public async Task<Result<AdminOperationResponse>> UnblockUserAsync(AdminUnblockUserRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUserProvider.IsAdmin)
        {
            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: AuditActionType.Unblock,
                TargetEntityType: nameof(UserAccount),
                TargetEntityId: request.UserId.ToString(),
                Outcome: AuditOutcome.Forbidden,
                Details: "Non-admin attempted to unblock user."
            ), cancellationToken);

            return Result<AdminOperationResponse>.Forbidden("Only admins can unblock users.");
        }

        var user = await _userAccountRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: AuditActionType.Unblock,
                TargetEntityType: nameof(UserAccount),
                TargetEntityId: request.UserId.ToString(),
                Outcome: AuditOutcome.Failed,
                Details: "Unblock failed: user not found."
            ), cancellationToken);

            return Result<AdminOperationResponse>.NotFound("User not found.");
        }

        if (!user.IsBlocked)
        {
            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: AuditActionType.Unblock,
                TargetEntityType: nameof(UserAccount),
                TargetEntityId: user.Id.ToString(),
                Outcome: AuditOutcome.Failed,
                Details: "Unblock failed: user is not currently blocked."
            ), cancellationToken);

            return Result<AdminOperationResponse>.Conflict("User is not currently blocked.");
        }

        user.IsBlocked = false;
        user.AccountStatus = AccountStatus.Active;
        user.FailedLoginAttempts = 0;
        user.LockedUntilUtc = null;

        await _userAccountRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var details = string.IsNullOrWhiteSpace(request.Reason)
            ? $"User {user.Id} unblocked."
            : $"User {user.Id} unblocked. Reason: {request.Reason}";

        await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
            ActionType: AuditActionType.Unblock,
            TargetEntityType: nameof(UserAccount),
            TargetEntityId: user.Id.ToString(),
            Outcome: AuditOutcome.Succeeded,
            Details: details
        ), cancellationToken);

        return Result<AdminOperationResponse>.Success(new AdminOperationResponse(
            user.Id,
            Operation: "Unblock",
            Message: "User has been unblocked."));
    }

    public async Task<Result<PagedResult<AuditLogEntryDto>>> GetAuditLogsAsync(GetAuditLogsRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Page <= 0 || request.PageSize <= 0)
        {
            return Result<PagedResult<AuditLogEntryDto>>.ValidationFailure("Page and PageSize must be greater than 0.");
        }

        if (request.TargetEntityId is not null && request.TargetEntityType is null)
        {
            return Result<PagedResult<AuditLogEntryDto>>.ValidationFailure("TargetEntityType must be provided when TargetEntityId is specified.");
        }

        var result = await _auditLogRepository.GetByFilterAsync(request.ActorUserId, request.TargetEntityType, request.TargetEntityId, request.Page, request.PageSize, cancellationToken);
        var auditLogs = result.Items.Select(a => a.ToAuditLogEntryDto()).ToList();
        return Result<PagedResult<AuditLogEntryDto>>.Success(new PagedResult<AuditLogEntryDto>(auditLogs, result.Page, result.PageSize, result.TotalCount));
    }

    public async Task<Result<PagedResult<AdminUserDto>>> GetUsersAsync(GetAdminUsersRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUserProvider.IsAdmin)
        {
            return Result<PagedResult<AdminUserDto>>.Forbidden("Only admins can list users.");
        }

        if (request.Page <= 0 || request.PageSize <= 0)
        {
            return Result<PagedResult<AdminUserDto>>.ValidationFailure("Page and PageSize must be greater than 0.");
        }

        var page = await _userAccountRepository.GetByFilterAsync(request.Role, request.Status, request.Page, request.PageSize, cancellationToken);
        var items = page.Items.Select(ToAdminUserDto).ToList();
        return Result<PagedResult<AdminUserDto>>.Success(new PagedResult<AdminUserDto>(items, page.Page, page.PageSize, page.TotalCount));
    }

    public async Task<Result<PagedResult<AdminPaymentDto>>> GetPaymentsAsync(GetAdminPaymentsRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUserProvider.IsAdmin)
        {
            return Result<PagedResult<AdminPaymentDto>>.Forbidden("Only admins can list payments.");
        }

        if (request.Page <= 0 || request.PageSize <= 0)
        {
            return Result<PagedResult<AdminPaymentDto>>.ValidationFailure("Page and PageSize must be greater than 0.");
        }

        if (!_currencyConversionService.TryGetPriceConverter(request.Currency, out var currencyCode, out var convert))
        {
            return Result<PagedResult<AdminPaymentDto>>.ValidationFailure($"Unsupported currency '{request.Currency}'.");
        }

        var page = await _paymentRepository.GetRecentAsync(request.Page, request.PageSize, cancellationToken);
        var items = page.Items.Select(p => ToAdminPaymentDto(p, currencyCode, convert)).ToList();
        return Result<PagedResult<AdminPaymentDto>>.Success(new PagedResult<AdminPaymentDto>(items, page.Page, page.PageSize, page.TotalCount));
    }

    public async Task<Result<DashboardStatsDto>> GetDashboardStatsAsync(GetDashboardStatsRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUserProvider.IsAdmin)
        {
            return Result<DashboardStatsDto>.Forbidden("Only admins can view dashboard stats.");
        }

        if (!_currencyConversionService.TryGetPriceConverter(request.Currency, out var currencyCode, out var convert))
        {
            return Result<DashboardStatsDto>.ValidationFailure($"Unsupported currency '{request.Currency}'.");
        }

        var now = _dateTimeProvider.UtcNow;
        var dayAgo = now.AddDays(-1);

        var activeUsers = await _userAccountRepository.CountActiveAsync(cancellationToken);
        var orderStats = await _orderRepository.GetOrderStatusAggregateAsync(dayAgo, now, cancellationToken);
        var salesAllTime = await _orderRepository.GetSalesAggregateAsync(null, null, cancellationToken);
        var openIssues = await _adminIssueRepository.CountByStatusAsync(IssueStatus.Open, cancellationToken);

        var totalRevenue = Math.Round(convert(salesAllTime.TotalSalesAmount), 2);

        return Result<DashboardStatsDto>.Success(new DashboardStatsDto(
            ActiveUsers: activeUsers,
            OrdersInLast24Hours: orderStats.TotalOrders,
            TotalRevenue: totalRevenue,
            CurrencyCode: currencyCode,
            OpenIssues: openIssues,
            GeneratedAtUtc: now));
    }

    private static AdminUserDto ToAdminUserDto(UserAccount user)
    {
        string role;
        string? company = null;
        string displayName;

        if (user.IsAdmin)
        {
            role = "Admin";
            displayName = user.Email.Value;
        }
        else if (user.SellerProfile is not null)
        {
            role = "Seller";
            company = user.SellerProfile.BusinessName;
            displayName = user.SellerProfile.BusinessName;
        }
        else if (user.CustomerProfile is not null)
        {
            role = "Customer";
            displayName = $"{user.CustomerProfile.FirstName} {user.CustomerProfile.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = user.Email.Value;
            }
        }
        else
        {
            role = "Customer";
            displayName = user.Email.Value;
        }

        string status;
        if (user.IsBlocked || user.AccountStatus == AccountStatus.Suspended)
        {
            status = "blocked";
        }
        else if (user.SellerProfile is not null && user.SellerProfile.VerificationStatus == VerificationStatus.Pending)
        {
            status = "pending verification";
        }
        else
        {
            status = "active";
        }

        return new AdminUserDto(
            user.Id,
            user.Email.Value,
            displayName,
            role,
            status,
            company,
            user.CreatedAtUtc,
            user.LastLoginAtUtc);
    }

    private static AdminPaymentDto ToAdminPaymentDto(OrderPayment payment, string currencyCode, Func<decimal, decimal> convert)
    {
        var customerName = "Unknown";
        if (payment.Order?.Customer is { } customer)
        {
            customerName = $"{customer.FirstName} {customer.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(customerName))
            {
                customerName = "Customer";
            }
        }

        var date = payment.PaidAtUtc ?? payment.Order?.OrderPurchaseTimestampUtc ?? DateTimeOffset.UtcNow;
        var displayId = $"{payment.Order?.OrderNumber ?? payment.OrderId.ToString("N")[..8]}-{payment.PaymentSequential}";
        var status = payment.PaymentStatus switch
        {
            PaymentStatus.Paid => "completed",
            PaymentStatus.Refunded => "completed",
            PaymentStatus.Pending => "pending",
            PaymentStatus.Authorized => "pending",
            PaymentStatus.Failed => "failed",
            PaymentStatus.Cancelled => "failed",
            _ => "pending"
        };

        return new AdminPaymentDto(
            payment.OrderId,
            payment.PaymentSequential,
            displayId,
            customerName,
            date,
            Math.Round(convert(payment.PaymentValue), 2),
            currencyCode,
            status);
    }
}
