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
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ISellerRepository _sellerRepository;
    private readonly ICurrencyConversionService _currencyConversionService;
    private readonly IAuditLogService _auditLogService;
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(
        IOrderRepository orderRepository,
        IOrderItemRepository orderItemRepository,
        ICustomerRepository customerRepository,
        ISellerRepository sellerRepository,
        ICurrencyConversionService currencyConversionService,
        IAuditLogService auditLogService,
        IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(orderRepository);
        ArgumentNullException.ThrowIfNull(orderItemRepository);
        ArgumentNullException.ThrowIfNull(customerRepository);
        ArgumentNullException.ThrowIfNull(sellerRepository);
        ArgumentNullException.ThrowIfNull(currencyConversionService);
        ArgumentNullException.ThrowIfNull(auditLogService);
        ArgumentNullException.ThrowIfNull(unitOfWork);

        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _customerRepository = customerRepository;
        _sellerRepository = sellerRepository;
        _currencyConversionService = currencyConversionService;
        _auditLogService = auditLogService;
        _unitOfWork = unitOfWork;
    }

    public Task<Result<OrderDto>> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<OrderDto>.NotImplemented());
    }

    public async Task<Result<OrderDto>> GetByIdForCustomerAsync(Guid orderId, Guid authenticatedUserId, string? currency, CancellationToken cancellationToken = default)
    {
        if (!_currencyConversionService.TryGetPriceConverter(currency, out var currencyCode, out var priceConverter))
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

    public Task<Result<PagedResult<OrderDto>>> GetByCustomerAsync(Guid customerId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<PagedResult<OrderDto>>.NotImplemented());
    }

    public async Task<Result<PagedResult<OrderSummaryDto>>> GetSummaryByCustomerUserAsync(
        Guid authenticatedUserId,
        PagedRequest request,
        string? currency,
        OrderStatus? status = null,
        CustomerOrderSort sort = CustomerOrderSort.Newest,
        CancellationToken cancellationToken = default)
    {
        if (request.Page < 1 || request.PageSize < 1)
        {
            return Result<PagedResult<OrderSummaryDto>>.ValidationFailure("Page and page size must be greater than zero.");
        }

        if (!_currencyConversionService.TryGetPriceConverter(currency, out var currencyCode, out var priceConverter))
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
        if (request.Page < 1 || request.PageSize < 1)
        {
            return Result<PagedResult<OrderDto>>.ValidationFailure("Page and page size must be greater than zero.");
        }

        if (!_currencyConversionService.TryGetPriceConverter(currency, out var currencyCode, out var priceConverter))
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
        if (request.Page < 1 || request.PageSize < 1)
        {
            return Result<PagedResult<SellerOrderSummaryDto>>.ValidationFailure("Page and page size must be greater than zero.");
        }

        if (!_currencyConversionService.TryGetPriceConverter(currency, out var currencyCode, out var priceConverter))
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
        if (!_currencyConversionService.TryGetPriceConverter(currency, out var currencyCode, out var priceConverter))
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
        if (!_currencyConversionService.TryGetPriceConverter(currency, out var currencyCode, out var priceConverter))
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

    public Task<Result<IReadOnlyList<OrderItemDto>>> GetOrderItemsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<IReadOnlyList<OrderItemDto>>.NotImplemented());
    }

    public Task<Result<OrderDto>> UpdateStatusAsync(UpdateOrderStatusRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<OrderDto>.NotImplemented());
    }

    public async Task<Result<SellerOrderSummaryDto>> UpdateStatusForSellerUserAsync(
        UpdateOrderStatusRequest request,
        Guid authenticatedUserId,
        string? currency,
        CancellationToken cancellationToken = default)
    {
        if (!_currencyConversionService.TryGetPriceConverter(currency, out var currencyCode, out var priceConverter))
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
        if (order.Items.Any(item => item.SellerId != seller.Id))
        {
            return Result<SellerOrderSummaryDto>.ValidationFailure("Order contains items from another seller and cannot be updated with order-level seller status.");
        }

        if (!CanSellerMoveOrderToStatus(order.OrderStatus, request.Status))
        {
            return Result<SellerOrderSummaryDto>.ValidationFailure($"Order cannot move from {order.OrderStatus} to {request.Status}.");
        }

        var previousStatus = order.OrderStatus;
        ApplySellerOrderStatus(order, request.Status);

        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
            ActionType: AuditActionType.Updated,
            TargetEntityType: nameof(Order),
            TargetEntityId: order.Id.ToString(),
            Outcome: AuditOutcome.Succeeded,
            Details: $"Seller {seller.Id} changed order {order.Id} status from {previousStatus} to {request.Status}."
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
        if (!_currencyConversionService.TryGetPriceConverter(currency, out var currencyCode, out var priceConverter))
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

    private static bool CanSellerMoveOrderToStatus(OrderStatus currentStatus, OrderStatus nextStatus)
    {
        if (currentStatus == nextStatus)
        {
            return true;
        }

        return currentStatus switch
        {
            OrderStatus.Pending => nextStatus == OrderStatus.Approved,
            OrderStatus.Approved => nextStatus is OrderStatus.Processing or OrderStatus.Shipped,
            OrderStatus.Processing => nextStatus == OrderStatus.Shipped,
            _ => false
        };
    }

    private static void ApplySellerOrderStatus(Order order, OrderStatus status)
    {
        order.OrderStatus = status;

        if (status is OrderStatus.Approved or OrderStatus.Processing or OrderStatus.Shipped)
        {
            order.OrderApprovedAtUtc ??= DateTimeOffset.UtcNow;
        }

        if (status == OrderStatus.Shipped)
        {
            order.OrderDeliveredCarrierDateUtc ??= DateTimeOffset.UtcNow;
        }
    }
}
