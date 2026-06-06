using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Entities.Orders;
using Backend.Domain.Enums;
namespace Backend.Application.Services;

public sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ISellerRepository _sellerRepository;
    private readonly ICurrencyConversionService _currencyConversionService;
    private readonly IAuditLogService _auditLogService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public OrderService(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        ISellerRepository sellerRepository,
        ICurrencyConversionService currencyConversionService,
        IAuditLogService auditLogService,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        ArgumentNullException.ThrowIfNull(orderRepository);
        ArgumentNullException.ThrowIfNull(customerRepository);
        ArgumentNullException.ThrowIfNull(sellerRepository);
        ArgumentNullException.ThrowIfNull(currencyConversionService);
        ArgumentNullException.ThrowIfNull(auditLogService);
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);

        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _sellerRepository = sellerRepository;
        _currencyConversionService = currencyConversionService;
        _auditLogService = auditLogService;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<OrderDto>> GetByIdForCustomerAsync(Guid orderId, Guid authenticatedUserId, string? currency, CancellationToken cancellationToken = default)
    {
        if (!_currencyConversionService.TryGetPriceFromBaseConverter(currency, out var currencyCode, out var priceConverter))
        {
            return Result<OrderDto>.ValidationFailure("Currency must be one of BRL, USD, or DKK.");
        }

        var customer = await _customerRepository.GetByUserIdAsync(authenticatedUserId, cancellationToken);
        if (customer is null)
        {
            return Result<OrderDto>.NotFound("Customer profile was not found for the authenticated user.");
        }

        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId, cancellationToken);
        if (order is null || order.CustomerId != customer.Id)
        {
            return Result<OrderDto>.NotFound("Order was not found for the authenticated customer.");
        }

        return Result<OrderDto>.Success(order.ToOrderDto(authenticatedUserId, currencyCode, priceConverter));
    }

    public async Task<Result<PagedResult<OrderSummaryDto>>> GetSummaryByCustomerUserAsync(
        Guid authenticatedUserId,
        PagedRequest request,
        string? currency,
        OrderStatus? status = null,
        CustomerOrderSort sort = CustomerOrderSort.Newest,
        CancellationToken cancellationToken = default)
    {
        var paginationError = PaginationRules.Validate(request.Page, request.PageSize);
        if (paginationError is not null)
        {
            return Result<PagedResult<OrderSummaryDto>>.ValidationFailure(paginationError);
        }

        if (!_currencyConversionService.TryGetPriceFromBaseConverter(currency, out var currencyCode, out var priceConverter))
        {
            return Result<PagedResult<OrderSummaryDto>>.ValidationFailure("Currency must be one of BRL, USD, or DKK.");
        }

        var customer = await _customerRepository.GetByUserIdAsync(authenticatedUserId, cancellationToken);
        if (customer is null)
        {
            return Result<PagedResult<OrderSummaryDto>>.NotFound("Customer profile was not found for the authenticated user.");
        }

        var orders = await _orderRepository.GetByCustomerIdAsync(customer.Id, request.Page, request.PageSize, status, sort, cancellationToken);
        var mappedOrders = orders.Items
            .Select(order => order.ToOrderSummaryDto(authenticatedUserId, currencyCode, priceConverter))
            .ToArray();

        return Result<PagedResult<OrderSummaryDto>>.Success(
            new PagedResult<OrderSummaryDto>(mappedOrders, orders.Page, orders.PageSize, orders.TotalCount));
    }

    public async Task<Result<PagedResult<OrderDto>>> GetByCustomerUserAsync(Guid authenticatedUserId, PagedRequest request, string? currency, CancellationToken cancellationToken = default)
    {
        var paginationError = PaginationRules.Validate(request.Page, request.PageSize);
        if (paginationError is not null)
        {
            return Result<PagedResult<OrderDto>>.ValidationFailure(paginationError);
        }

        if (!_currencyConversionService.TryGetPriceFromBaseConverter(currency, out var currencyCode, out var priceConverter))
        {
            return Result<PagedResult<OrderDto>>.ValidationFailure("Currency must be one of BRL, USD, or DKK.");
        }

        var customer = await _customerRepository.GetByUserIdAsync(authenticatedUserId, cancellationToken);
        if (customer is null)
        {
            return Result<PagedResult<OrderDto>>.NotFound("Customer profile was not found for the authenticated user.");
        }

        var orders = await _orderRepository.GetByCustomerIdWithDetailsAsync(customer.Id, request.Page, request.PageSize, cancellationToken);
        var mappedOrders = orders.Items
            .Select(order => order.ToOrderDto(authenticatedUserId, currencyCode, priceConverter))
            .ToArray();

        return Result<PagedResult<OrderDto>>.Success(
            new PagedResult<OrderDto>(mappedOrders, orders.Page, orders.PageSize, orders.TotalCount));
    }

    public async Task<Result<PagedResult<SellerOrderSummaryDto>>> GetSummaryBySellerUserAsync(
        Guid authenticatedUserId,
        PagedRequest request,
        string? currency,
        OrderStatus? status = null,
        SellerOrderSort sort = SellerOrderSort.Newest,
        CancellationToken cancellationToken = default)
    {
        var paginationError = PaginationRules.Validate(request.Page, request.PageSize);
        if (paginationError is not null)
        {
            return Result<PagedResult<SellerOrderSummaryDto>>.ValidationFailure(paginationError);
        }

        if (!_currencyConversionService.TryGetPriceFromBaseConverter(currency, out var currencyCode, out var priceConverter))
        {
            return Result<PagedResult<SellerOrderSummaryDto>>.ValidationFailure("Currency must be one of BRL, USD, or DKK.");
        }

        var seller = await _sellerRepository.GetByUserIdAsync(authenticatedUserId, cancellationToken);
        if (seller is null)
        {
            return Result<PagedResult<SellerOrderSummaryDto>>.NotFound("Seller profile was not found for the authenticated user.");
        }
        if (seller.VerificationStatus != VerificationStatus.Verified)
        {
            return Result<PagedResult<SellerOrderSummaryDto>>.Forbidden("Seller must be verified to manage orders.");
        }

        var orders = await _orderRepository.GetBySellerIdAsync(seller.Id, request.Page, request.PageSize, status, sort, cancellationToken);
        var mappedOrders = orders.Items
            .Select(order => order.ToSellerOrderSummaryDto(seller.Id, currencyCode, priceConverter))
            .ToArray();

        return Result<PagedResult<SellerOrderSummaryDto>>.Success(
            new PagedResult<SellerOrderSummaryDto>(mappedOrders, orders.Page, orders.PageSize, orders.TotalCount));
    }

    public async Task<Result<SellerOrderSummaryDto>> GetByIdForSellerUserAsync(
        Guid orderId,
        Guid authenticatedUserId,
        string? currency,
        CancellationToken cancellationToken = default)
    {
        if (!_currencyConversionService.TryGetPriceFromBaseConverter(currency, out var currencyCode, out var priceConverter))
        {
            return Result<SellerOrderSummaryDto>.ValidationFailure("Currency must be one of BRL, USD, or DKK.");
        }

        var seller = await _sellerRepository.GetByUserIdAsync(authenticatedUserId, cancellationToken);
        if (seller is null)
        {
            return Result<SellerOrderSummaryDto>.NotFound("Seller profile was not found for the authenticated user.");
        }
        if (seller.VerificationStatus != VerificationStatus.Verified)
        {
            return Result<SellerOrderSummaryDto>.Forbidden("Seller must be verified to manage orders.");
        }

        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId, cancellationToken);
        if (order is null || !order.Items.Any(item => item.SellerId == seller.Id))
        {
            return Result<SellerOrderSummaryDto>.NotFound("Order was not found for the authenticated seller.");
        }

        return Result<SellerOrderSummaryDto>.Success(order.ToSellerOrderSummaryDto(seller.Id, currencyCode, priceConverter));
    }

    public async Task<Result<SellerOrderStatsDto>> GetStatsBySellerUserAsync(
        Guid authenticatedUserId,
        string? currency,
        CancellationToken cancellationToken = default)
    {
        if (!_currencyConversionService.TryGetPriceFromBaseConverter(currency, out var currencyCode, out var priceConverter))
        {
            return Result<SellerOrderStatsDto>.ValidationFailure("Currency must be one of BRL, USD, or DKK.");
        }

        var seller = await _sellerRepository.GetByUserIdAsync(authenticatedUserId, cancellationToken);
        if (seller is null)
        {
            return Result<SellerOrderStatsDto>.NotFound("Seller profile was not found for the authenticated user.");
        }
        if (seller.VerificationStatus != VerificationStatus.Verified)
        {
            return Result<SellerOrderStatsDto>.Forbidden("Seller must be verified to manage orders.");
        }

        var aggregate = await _orderRepository.GetSellerOrderAggregateAsync(seller.Id, cancellationToken);
        return Result<SellerOrderStatsDto>.Success(new SellerOrderStatsDto(
            aggregate.TotalOrders,
            aggregate.ActiveOrders,
            priceConverter(aggregate.TotalRevenue),
            currencyCode));
    }

    public async Task<Result<SellerOrderSummaryDto>> UpdateStatusForSellerUserAsync(
        UpdateOrderStatusRequest request,
        Guid authenticatedUserId,
        string? currency,
        CancellationToken cancellationToken = default)
    {
        if (!_currencyConversionService.TryGetPriceFromBaseConverter(currency, out var currencyCode, out var priceConverter))
        {
            return Result<SellerOrderSummaryDto>.ValidationFailure("Currency must be one of BRL, USD, or DKK.");
        }

        var seller = await _sellerRepository.GetByUserIdAsync(authenticatedUserId, cancellationToken);
        if (seller is null)
        {
            return Result<SellerOrderSummaryDto>.NotFound("Seller profile was not found for the authenticated user.");
        }
        if (seller.VerificationStatus != VerificationStatus.Verified)
        {
            return Result<SellerOrderSummaryDto>.Forbidden("Seller must be verified to manage orders.");
        }

        var order = await _orderRepository.GetByIdWithDetailsAsync(request.OrderId, cancellationToken);
        if (order is null || !order.Items.Any(item => item.SellerId == seller.Id))
        {
            return Result<SellerOrderSummaryDto>.NotFound("Order was not found for the authenticated seller.");
        }
        if (order.OrderStatus is OrderStatus.Cancelled or OrderStatus.Delivered or OrderStatus.Returned)
        {
            return Result<SellerOrderSummaryDto>.ValidationFailure($"Order cannot be updated in its current status of {order.OrderStatus}.");
        }

        var sellerItems = order.Items
            .Where(item => item.SellerId == seller.Id)
            .ToArray();
        var blockedItem = sellerItems.FirstOrDefault(item => !CanSellerMoveItemToStatus(item.FulfillmentStatus, request.Status));
        if (blockedItem is not null)
        {
            return Result<SellerOrderSummaryDto>.ValidationFailure($"Seller item {blockedItem.OrderItemId} cannot move from {blockedItem.FulfillmentStatus} to {request.Status}.");
        }

        var previousOrderStatus = order.OrderStatus;
        var now = _dateTimeProvider.UtcNow;
        foreach (var item in sellerItems)
        {
            ApplySellerItemStatus(item, request.Status, now);
        }

        ApplyDerivedOrderStatus(order, now);

        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
            ActionType: AuditActionType.Updated,
            TargetEntityType: nameof(Order),
            TargetEntityId: order.Id.ToString(),
            Outcome: AuditOutcome.Succeeded,
            Details: $"Seller {seller.Id} changed {sellerItems.Length} item(s) on order {order.Id} to {request.Status}. Order status moved from {previousOrderStatus} to {order.OrderStatus}."
        ), cancellationToken);

        var updatedOrder = await _orderRepository.GetByIdWithDetailsAsync(request.OrderId, cancellationToken);
        if (updatedOrder is null)
        {
            return Result<SellerOrderSummaryDto>.NotFound("Order was not found after update.");
        }

        return Result<SellerOrderSummaryDto>.Success(updatedOrder.ToSellerOrderSummaryDto(seller.Id, currencyCode, priceConverter));
    }

    public async Task<Result<OrderDto>> CancelAsync(CancelOrderRequest request, Guid authenticatedUserId, string? currency, CancellationToken cancellationToken = default)
    {
        if (!_currencyConversionService.TryGetPriceFromBaseConverter(currency, out var currencyCode, out var priceConverter))
        {
            return Result<OrderDto>.ValidationFailure("Currency must be one of BRL, USD, or DKK.");
        }

        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result<OrderDto>.NotFound("Order was not found.");
        }

        var customer = await _customerRepository.GetByUserIdAsync(authenticatedUserId, cancellationToken);
        if (customer is null)
        {
            return Result<OrderDto>.NotFound("Customer profile was not found for the authenticated user.");
        }

        if (order.CustomerId != customer.Id)
        {
            return Result<OrderDto>.Forbidden("The authenticated user is not the owner of the order.");
        }

        var allowed = new List<OrderStatus> { OrderStatus.Pending, OrderStatus.Approved, OrderStatus.Processing };
        if (!allowed.Contains(order.OrderStatus))
        {
            return Result<OrderDto>.ValidationFailure($"Order cannot be cancelled in its current status of {order.OrderStatus}.");
        }

        order.OrderStatus = OrderStatus.Cancelled;
        order.OrderStatusDescription = request.Reason;

        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
            ActionType: AuditActionType.Cancelled,
            TargetEntityType: nameof(Order),
            TargetEntityId: order.Id.ToString(),
            Outcome: AuditOutcome.Succeeded,
            Details: $"Order {order.Id} was cancelled by user {authenticatedUserId} for reason: \"{request.Reason}\""
        ), cancellationToken);

        var orderWithDetails = await _orderRepository.GetByIdWithDetailsAsync(request.OrderId, cancellationToken);
        if (orderWithDetails is null)
        {
            return Result<OrderDto>.NotFound("Order was not found after update.");
        }

        return Result<OrderDto>.Success(orderWithDetails.ToOrderDto(authenticatedUserId, currencyCode, priceConverter));
    }

    private static bool CanSellerMoveItemToStatus(OrderStatus currentStatus, OrderStatus nextStatus)
    {
        if (currentStatus == nextStatus)
        {
            return true;
        }

        return currentStatus switch
        {
            OrderStatus.Pending => nextStatus == OrderStatus.Approved,
            OrderStatus.Approved => nextStatus == OrderStatus.Processing,
            OrderStatus.Processing => nextStatus == OrderStatus.Shipped,
            OrderStatus.Shipped => nextStatus == OrderStatus.Delivered,
            _ => false
        };
    }

    private static void ApplySellerItemStatus(OrderItem item, OrderStatus status, DateTimeOffset now)
    {
        item.FulfillmentStatus = status;

        if (status is OrderStatus.Approved or OrderStatus.Processing or OrderStatus.Shipped or OrderStatus.Delivered)
        {
            item.FulfillmentApprovedAtUtc ??= now;
        }

        if (status is OrderStatus.Processing or OrderStatus.Shipped or OrderStatus.Delivered)
        {
            item.FulfillmentProcessingAtUtc ??= now;
        }

        if (status is OrderStatus.Shipped or OrderStatus.Delivered)
        {
            item.FulfillmentShippedAtUtc ??= now;
        }
    }

    private static void ApplyDerivedOrderStatus(Order order, DateTimeOffset now)
    {
        var derivedStatus = DeriveOrderStatusFromItems(order.Items);
        order.OrderStatus = derivedStatus;

        if (derivedStatus is OrderStatus.Approved or OrderStatus.Processing or OrderStatus.Shipped or OrderStatus.Delivered)
        {
            order.OrderApprovedAtUtc ??= now;
        }

        if (derivedStatus is OrderStatus.Shipped or OrderStatus.Delivered)
        {
            order.OrderDeliveredCarrierDateUtc ??= now;
        }

        if (derivedStatus == OrderStatus.Delivered)
        {
            order.OrderDeliveredCustomerDateUtc ??= now;
        }
    }

    private static OrderStatus DeriveOrderStatusFromItems(IEnumerable<OrderItem> items)
    {
        var itemStatuses = items.Select(item => item.FulfillmentStatus).ToArray();
        if (itemStatuses.Length == 0)
        {
            return OrderStatus.Pending;
        }

        if (itemStatuses.All(status => IsAtLeast(status, OrderStatus.Delivered)))
        {
            return OrderStatus.Delivered;
        }

        if (itemStatuses.All(status => IsAtLeast(status, OrderStatus.Shipped)))
        {
            return OrderStatus.Shipped;
        }

        if (itemStatuses.All(status => IsAtLeast(status, OrderStatus.Processing)))
        {
            return OrderStatus.Processing;
        }

        if (itemStatuses.All(status => IsAtLeast(status, OrderStatus.Approved)))
        {
            return OrderStatus.Approved;
        }

        return OrderStatus.Pending;
    }

    private static bool IsAtLeast(OrderStatus currentStatus, OrderStatus targetStatus) =>
        GetFulfillmentRank(currentStatus) >= GetFulfillmentRank(targetStatus);

    private static int GetFulfillmentRank(OrderStatus status) =>
        status switch
        {
            OrderStatus.Approved => 2,
            OrderStatus.Processing => 3,
            OrderStatus.Shipped => 4,
            OrderStatus.Delivered => 5,
            _ => 1
        };
}
