using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
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

    public Task<Result<IReadOnlyList<PaymentDto>>> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<PaymentDto>> RecordPaymentAsync(RecordPaymentRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

