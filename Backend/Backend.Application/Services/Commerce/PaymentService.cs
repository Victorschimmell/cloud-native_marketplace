using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Entities.Orders;
using Backend.Domain.Enums;

namespace Backend.Application.Services;

public sealed class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public PaymentService(IPaymentRepository paymentRepository, IOrderRepository orderRepository, IDateTimeProvider dateTimeProvider )
    {
        ArgumentNullException.ThrowIfNull(paymentRepository);
        ArgumentNullException.ThrowIfNull(orderRepository);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);

        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<IReadOnlyList<PaymentDto>>> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        if (orderId == Guid.Empty)
        {
            return Result<IReadOnlyList<PaymentDto>>.ValidationFailure("Order id is required.");
        }

        var payments = await _paymentRepository.GetByOrderIdAsync(orderId, cancellationToken);
        return Result<IReadOnlyList<PaymentDto>>.Success(payments.Select(static payment => payment.ToDto()).ToArray());
    }

    public async Task<Result<PaymentDto>> RecordPaymentAsync(RecordPaymentRequest request, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (request.OrderId == Guid.Empty || request.CurrencyId == Guid.Empty || request.PaymentValue <= 0)
            {
                return Result<PaymentDto>.ValidationFailure("Order id, currency id, and payment value are required.");
            }

            if (await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken) is null)
            {
                return Result<PaymentDto>.NotFound("Order was not found.");
            }

            var existingPayments = await _paymentRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);
            var payment = new OrderPayment
            {
                OrderId = request.OrderId,
                PaymentSequential = existingPayments.Count + 1,
                CurrencyId = request.CurrencyId,
                PaymentType = request.PaymentType,
                PaymentInstallments = request.PaymentInstallments,
                PaymentValue = request.PaymentValue,
                ExternalPaymentReference = request.ExternalPaymentReference,
                PaymentStatus = PaymentStatus.Pending,
                PaidAtUtc = _dateTimeProvider.UtcNow
            };

            await _paymentRepository.AddAsync(payment, cancellationToken);
            return Result<PaymentDto>.Success(payment.ToDto());
        }, "Unable to record payment.");
    }
}

