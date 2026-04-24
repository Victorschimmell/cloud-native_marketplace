using Backend.Application.Abstractions.Repositories;
using Backend.Domain.Entities.Orders;
using Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.Repositories;

internal sealed class PaymentRepository(ApplicationDbContext dbContext) : IPaymentRepository
{
    public async Task<OrderPayment?> GetByIdAsync(Guid orderId, int paymentSequential, CancellationToken cancellationToken = default)
    {
        return await dbContext.OrderPayments
            .FirstOrDefaultAsync(
                payment => payment.OrderId == orderId && payment.PaymentSequential == paymentSequential,
                cancellationToken);
    }

    public async Task<IReadOnlyList<OrderPayment>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await dbContext.OrderPayments
            .Where(payment => payment.OrderId == orderId)
            .OrderBy(payment => payment.PaymentSequential)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(OrderPayment payment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payment);

        if (payment.PaymentSequential <= 0)
        {
            payment.PaymentSequential = await GetNextPaymentSequentialAsync(payment.OrderId, cancellationToken);
        }

        SimulateGatewayProcessing(payment);

        await dbContext.OrderPayments.AddAsync(payment, cancellationToken);
    }

    public Task UpdateAsync(OrderPayment payment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payment);

        dbContext.OrderPayments.Update(payment);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(OrderPayment payment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payment);

        dbContext.OrderPayments.Remove(payment);
        return Task.CompletedTask;
    }

    private async Task<int> GetNextPaymentSequentialAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var persistedMax = await dbContext.OrderPayments
            .Where(payment => payment.OrderId == orderId)
            .Select(payment => (int?)payment.PaymentSequential)
            .MaxAsync(cancellationToken);

        var pendingMax = dbContext.ChangeTracker.Entries<OrderPayment>()
            .Where(entry => entry.Entity.OrderId == orderId && entry.State != EntityState.Deleted)
            .Select(entry => (int?)entry.Entity.PaymentSequential)
            .Max();

        var currentMax = Math.Max(persistedMax ?? 0, pendingMax ?? 0);

        return currentMax + 1;
    }

    private void SimulateGatewayProcessing(OrderPayment payment)
    {
        if (string.IsNullOrWhiteSpace(payment.ExternalPaymentReference))
        {
            payment.ExternalPaymentReference = $"sim_{payment.OrderId:N}_{payment.PaymentSequential}_{Guid.NewGuid():N}";
        }

        // This repository intentionally simulates a successful payment provider interaction.
        payment.PaymentStatus = PaymentStatus.Paid;
        payment.PaidAtUtc ??= DateTimeOffset.UtcNow;
    }
}
