using Backend.Domain.Entities.Orders;

namespace Backend.Application.Abstractions.Repositories;

public interface IPaymentRepository
{
    Task<OrderPayment?> GetByIdAsync(Guid orderId, int paymentSequential, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderPayment>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task AddAsync(OrderPayment payment, CancellationToken cancellationToken = default);
    Task UpdateAsync(OrderPayment payment, CancellationToken cancellationToken = default);
    Task DeleteAsync(OrderPayment payment, CancellationToken cancellationToken = default);
}
