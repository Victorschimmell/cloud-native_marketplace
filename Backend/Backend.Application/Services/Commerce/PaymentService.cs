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
    private readonly IUnitOfWork _unitOfWork;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IOrderRepository orderRepository,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(paymentRepository);
        ArgumentNullException.ThrowIfNull(orderRepository);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        ArgumentNullException.ThrowIfNull(unitOfWork);

        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public Task<Result<IReadOnlyList<PaymentDto>>> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var result = _paymentRepository.GetByOrderIdAsync(orderId, cancellationToken)
            .ContinueWith(task => task.Result.Select(payment => payment.ToPaymentDto()).ToList(), cancellationToken);
        return Task.FromResult(Result<IReadOnlyList<PaymentDto>>.Success(result.Result));
    }

    public async Task<Result<PaymentDto>> RecordPaymentAsync(RecordPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var orderId = request.OrderId;
        var paymentDetails = request.PaymentDetails;
        var orderPayment = new OrderPayment
        {
            OrderId = orderId,
            PaymentSequential = 0, // Will be set in repository
            CurrencyId = paymentDetails.CurrencyId,
            PaymentType = paymentDetails.PaymentType,
            PaymentInstallments = paymentDetails.PaymentInstallments,
            PaymentValue = paymentDetails.PaymentValue,
            PaymentStatus = PaymentStatus.Pending,
            ExternalPaymentReference = paymentDetails.ExternalPaymentReference,
        };

        // NOTE: Currently never fails
        await _paymentRepository.AddAsync(orderPayment, cancellationToken);

        // NOTE: CheckoutService will also save changes again
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PaymentDto>.Success(orderPayment.ToPaymentDto());
    }
}

