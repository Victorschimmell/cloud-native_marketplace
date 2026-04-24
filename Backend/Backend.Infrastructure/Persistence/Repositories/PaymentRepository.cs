using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Domain.Entities.Orders;
using Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.Repositories;

internal sealed class PaymentRepository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider) : IPaymentRepository
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
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(OrderPayment payment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payment);

        dbContext.OrderPayments.Update(payment);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(OrderPayment payment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payment);

        dbContext.OrderPayments.Remove(payment);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<int> GetNextPaymentSequentialAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var currentMax = await dbContext.OrderPayments
            .Where(payment => payment.OrderId == orderId)
            .Select(payment => (int?)payment.PaymentSequential)
            .MaxAsync(cancellationToken);

        return (currentMax ?? 0) + 1;
    }

    private void SimulateGatewayProcessing(OrderPayment payment)
    {
        if (string.IsNullOrWhiteSpace(payment.ExternalPaymentReference))
        {
            payment.ExternalPaymentReference = $"sim_{payment.OrderId:N}_{payment.PaymentSequential}_{Guid.NewGuid():N}";
        }

        // This repository intentionally simulates a successful payment provider interaction.
        payment.PaymentStatus = PaymentStatus.Paid;
        payment.PaidAtUtc ??= dateTimeProvider.UtcNow;
    }
}
