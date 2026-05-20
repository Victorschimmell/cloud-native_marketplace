using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Domain.Enums;
using Backend.Application.Abstractions.Repositories;

namespace Backend.Application.Interfaces.Services;

public interface IOrderService
{
    Task<Result<OrderDto>> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Result<OrderDto>> GetByIdForCustomerAsync(Guid orderId, Guid authenticatedUserId, string? currency, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<OrderDto>>> GetByCustomerAsync(Guid customerId, PagedRequest request, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<OrderDto>>> GetByCustomerUserAsync(Guid authenticatedUserId, PagedRequest request, string? currency, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<OrderSummaryDto>>> GetSummaryByCustomerUserAsync(Guid authenticatedUserId, PagedRequest request, string? currency, OrderStatus? status = null, CustomerOrderSort sort = CustomerOrderSort.Newest, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<OrderItemDto>>> GetOrderItemsAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Result<OrderDto>> UpdateStatusAsync(UpdateOrderStatusRequest request, CancellationToken cancellationToken = default);
    Task<Result<OrderDto>> CancelAsync(CancelOrderRequest request, Guid authenticatedUserId, string? currency, CancellationToken cancellationToken = default);
}
