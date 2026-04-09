using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Entities.Operations;
using Backend.Domain.Entities.Orders;
using Backend.Domain.Enums;

namespace Backend.Application.Services;

public sealed class AnalyticsService : IAnalyticsService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AnalyticsService(IOrderRepository orderRepository, IDateTimeProvider dateTimeProvider)
    {
        ArgumentNullException.ThrowIfNull(orderRepository);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        _orderRepository = orderRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<SalesStatisticsDto>> GetSalesStatisticsAsync(GetSalesStatisticsRequest request, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetAllAsync(request.Page, request.PageSize, cancellationToken);
        var filteredOrders = ApplyDateFilter(orders, request.FromUtc, request.ToUtc);
        var totalSales = filteredOrders.Sum(order => order.TotalAmount);
        var count = filteredOrders.Count;

        var statistics = new SalesStatisticsDto(
            count,
            totalSales,
            count == 0 ? 0m : totalSales / count,
            _dateTimeProvider.UtcNow);

        return Result<SalesStatisticsDto>.Success(statistics);
    }

    public async Task<Result<OrderStatisticsDto>> GetOrderStatisticsAsync(GetOrderStatisticsRequest request, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetAllAsync(request.Page, request.PageSize, cancellationToken);
        var filteredOrders = ApplyDateFilter(orders, request.FromUtc, request.ToUtc);

        var statistics = new OrderStatisticsDto(
            filteredOrders.Count,
            filteredOrders.Count(order => order.OrderStatus == OrderStatus.Cancelled),
            filteredOrders.Count(order => order.OrderStatus == OrderStatus.Delivered),
            _dateTimeProvider.UtcNow);

        return Result<OrderStatisticsDto>.Success(statistics);
    }

    private static List<Order> ApplyDateFilter(IReadOnlyList<Order> orders, DateTimeOffset? fromUtc, DateTimeOffset? toUtc)
    {
        return orders
            .Where(order => !fromUtc.HasValue || order.OrderPurchaseTimestampUtc >= fromUtc.Value)
            .Where(order => !toUtc.HasValue || order.OrderPurchaseTimestampUtc <= toUtc.Value)
            .ToList();
    }
}

public sealed class AdminService : IAdminService
{
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AdminService(IUserAccountRepository userAccountRepository, IAuditLogRepository auditLogRepository, IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(userAccountRepository);
        ArgumentNullException.ThrowIfNull(auditLogRepository);
        ArgumentNullException.ThrowIfNull(unitOfWork);

        _userAccountRepository = userAccountRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AdminOperationResponse>> BlockUserAsync(AdminBlockUserRequest request, CancellationToken cancellationToken = default)
    {
        if (request.UserId == Guid.Empty)
        {
            return Result<AdminOperationResponse>.Failure("User id is required.");
        }

        var user = await _userAccountRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result<AdminOperationResponse>.Failure("User was not found.");
        }

        user.IsBlocked = true;
        user.AccountStatus = AccountStatus.Suspended;
        await _userAccountRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AdminOperationResponse>.Success(new AdminOperationResponse(user.Id, "BlockUser", true, request.Reason));
    }

    public async Task<Result<AdminOperationResponse>> UnblockUserAsync(AdminUnblockUserRequest request, CancellationToken cancellationToken = default)
    {
        if (request.UserId == Guid.Empty)
        {
            return Result<AdminOperationResponse>.Failure("User id is required.");
        }

        var user = await _userAccountRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result<AdminOperationResponse>.Failure("User was not found.");
        }

        user.IsBlocked = false;
        user.AccountStatus = AccountStatus.Active;
        await _userAccountRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AdminOperationResponse>.Success(new AdminOperationResponse(user.Id, "UnblockUser", true, request.Reason));
    }

    public async Task<Result<PagedResult<AuditLogEntryDto>>> GetAuditLogsAsync(GetAuditLogsRequest request, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<AuditLog> logs;

        if (request.ActorUserId.HasValue && request.ActorUserId.Value != Guid.Empty)
        {
            logs = await _auditLogRepository.GetByActorUserIdAsync(request.ActorUserId.Value, request.Page, request.PageSize, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(request.TargetEntityType) && !string.IsNullOrWhiteSpace(request.TargetEntityId))
        {
            logs = await _auditLogRepository.GetByTargetEntityAsync(request.TargetEntityType.Trim(), request.TargetEntityId.Trim(), cancellationToken);
        }
        else
        {
            logs = await _auditLogRepository.GetAllAsync(request.Page, request.PageSize, cancellationToken);
        }

        var items = logs.Select(static log => log.ToDto()).ToArray();
        return Result<PagedResult<AuditLogEntryDto>>.Success(new PagedResult<AuditLogEntryDto>(items, request.Page, request.PageSize, items.Length));
    }
}

public sealed class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AuditLogService(IAuditLogRepository auditLogRepository, IDateTimeProvider dateTimeProvider)
    {
        ArgumentNullException.ThrowIfNull(auditLogRepository);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        _auditLogRepository = auditLogRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<PagedResult<AuditLogEntryDto>>> GetByActorUserAsync(Guid actorUserId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        if (actorUserId == Guid.Empty)
        {
            return Result<PagedResult<AuditLogEntryDto>>.Failure("Actor user id is required.");
        }

        var logs = await _auditLogRepository.GetByActorUserIdAsync(actorUserId, request.Page, request.PageSize, cancellationToken);
        var items = logs.Select(static log => log.ToDto()).ToArray();
        return Result<PagedResult<AuditLogEntryDto>>.Success(new PagedResult<AuditLogEntryDto>(items, request.Page, request.PageSize, items.Length));
    }

    public async Task<Result<IReadOnlyList<AuditLogEntryDto>>> GetByTargetEntityAsync(string entityType, string entityId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(entityId))
        {
            return Result<IReadOnlyList<AuditLogEntryDto>>.Failure("Entity type and entity id are required.");
        }

        var logs = await _auditLogRepository.GetByTargetEntityAsync(entityType.Trim(), entityId.Trim(), cancellationToken);
        return Result<IReadOnlyList<AuditLogEntryDto>>.Success(logs.Select(static log => log.ToDto()).ToArray());
    }

    public async Task<Result<AuditLogEntryDto>> WriteEntryAsync(WriteAuditLogEntryRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.TargetEntityType) || string.IsNullOrWhiteSpace(request.TargetEntityId) || string.IsNullOrWhiteSpace(request.Details))
        {
            return Result<AuditLogEntryDto>.Failure("Target entity and details are required.");
        }

        var log = new AuditLog
        {
            ActorUserId = request.ActorUserId,
            ActorIpAddress = request.ActorIpAddress,
            ActionType = request.ActionType,
            TargetEntityType = request.TargetEntityType.Trim(),
            TargetEntityId = request.TargetEntityId.Trim(),
            Outcome = request.Outcome,
            Details = request.Details.Trim(),
            CreatedAtUtc = _dateTimeProvider.UtcNow
        };

        await _auditLogRepository.AddAsync(log, cancellationToken);
        return Result<AuditLogEntryDto>.Success(log.ToDto());
    }
}
