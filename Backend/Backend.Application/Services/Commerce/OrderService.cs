using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Enums;
namespace Backend.Application.Services;

public sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ICurrencyConversionService _currencyConversionService;

    public OrderService(
        IOrderRepository orderRepository,
        IOrderItemRepository orderItemRepository,
        ICustomerRepository customerRepository,
        ICurrencyConversionService currencyConversionService)
    {
        ArgumentNullException.ThrowIfNull(orderRepository);
        ArgumentNullException.ThrowIfNull(orderItemRepository);
        ArgumentNullException.ThrowIfNull(customerRepository);
        ArgumentNullException.ThrowIfNull(currencyConversionService);

        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _customerRepository = customerRepository;
        _currencyConversionService = currencyConversionService;
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

    public async Task<Result<PagedResult<OrderSummaryDto>>> GetSummaryByCustomerUserAsync(Guid authenticatedUserId, PagedRequest request, string? currency, CancellationToken cancellationToken = default)
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

        var orders = await _orderRepository.GetByCustomerIdAsync(customer.Id, request.Page, request.PageSize, cancellationToken);
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

    public Task<Result<IReadOnlyList<OrderItemDto>>> GetOrderItemsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<IReadOnlyList<OrderItemDto>>.NotImplemented());
    }

    public Task<Result<OrderDto>> UpdateStatusAsync(UpdateOrderStatusRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<OrderDto>.NotImplemented());
    }

    public async Task<Result<OrderDto>> CancelAsync(CancelOrderRequest request, Guid authenticatedUserId, string? currency, CancellationToken cancellationToken = default)
    {
        if (!_currencyConversionService.TryGetPriceConverter(currency, out var currencyCode, out var priceConverter))
        {
            return Result<OrderDto>.ValidationFailure("Currency must be one of BRL, USD, or DKK.");
        }

        var orders = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (orders is null)
        {
            return Result<OrderDto>.NotFound("Order was not found.");
        }

        var customer = await _customerRepository.GetByUserIdAsync(authenticatedUserId, cancellationToken);
        if (customer is null)
        {
            return Result<OrderDto>.NotFound("Customer profile was not found for the authenticated user.");
        }

        if (orders.CustomerId != customer.Id)
        {
            return Result<OrderDto>.Forbidden("The authenticated user is not the owner of the order.");
        }

        var allowed = new List<OrderStatus> { OrderStatus.Pending, OrderStatus.Approved, OrderStatus.Processing };
        if (!allowed.Contains(orders.OrderStatus))
        {
            return Result<OrderDto>.ValidationFailure($"Order cannot be cancelled in its current status of {orders.OrderStatus}.");
        }

        orders.OrderStatus = OrderStatus.Cancelled;
        orders.OrderStatusDescription = request.Reason;

        await _orderRepository.UpdateAsync(orders, cancellationToken);

        return Result<OrderDto>.Success(orders.ToOrderDto(authenticatedUserId, currencyCode, priceConverter));
    }
}
