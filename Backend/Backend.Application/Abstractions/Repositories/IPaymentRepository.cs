using Backend.Application.Common.Models;
using Backend.Domain.Entities.Orders;

namespace Backend.Application.Abstractions.Repositories;

public enum AdminPaymentStatusFilter
{
    Any,
    Completed,
    Pending,
    Failed
}

public interface IPaymentRepository
{
    Task<OrderPayment?> GetByIdAsync(Guid orderId, int paymentSequential, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderPayment>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<PagedResult<OrderPayment>> GetRecentAsync(AdminPaymentStatusFilter status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(OrderPayment payment, CancellationToken cancellationToken = default);
    Task UpdateAsync(OrderPayment payment, CancellationToken cancellationToken = default);
    Task DeleteAsync(OrderPayment payment, CancellationToken cancellationToken = default);
}
