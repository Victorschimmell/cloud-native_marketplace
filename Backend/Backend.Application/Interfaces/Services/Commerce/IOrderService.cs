using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Domain.Enums;
using Backend.Application.Abstractions.Repositories;

namespace Backend.Application.Interfaces.Services;

public interface IOrderService
{
    Task<Result<OrderDto>> GetByIdForCustomerAsync(Guid orderId, Guid authenticatedUserId, string? currency, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<OrderDto>>> GetByCustomerUserAsync(Guid authenticatedUserId, PagedRequest request, string? currency, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<OrderSummaryDto>>> GetSummaryByCustomerUserAsync(Guid authenticatedUserId, PagedRequest request, string? currency, OrderStatus? status = null, CustomerOrderSort sort = CustomerOrderSort.Newest, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<SellerOrderSummaryDto>>> GetSummaryBySellerUserAsync(Guid authenticatedUserId, PagedRequest request, string? currency, OrderStatus? status = null, SellerOrderSort sort = SellerOrderSort.Newest, CancellationToken cancellationToken = default);
    Task<Result<SellerOrderSummaryDto>> GetByIdForSellerUserAsync(Guid orderId, Guid authenticatedUserId, string? currency, CancellationToken cancellationToken = default);
    Task<Result<SellerOrderStatsDto>> GetStatsBySellerUserAsync(Guid authenticatedUserId, string? currency, CancellationToken cancellationToken = default);
    Task<Result<SellerOrderSummaryDto>> UpdateStatusForSellerUserAsync(UpdateOrderStatusRequest request, Guid authenticatedUserId, string? currency, CancellationToken cancellationToken = default);
    Task<Result<OrderDto>> CancelAsync(CancelOrderRequest request, Guid authenticatedUserId, string? currency, CancellationToken cancellationToken = default);
}
