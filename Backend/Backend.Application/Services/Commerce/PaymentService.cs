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
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IOrderRepository orderRepository,
        ICurrencyRepository currencyRepository,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(paymentRepository);
        ArgumentNullException.ThrowIfNull(orderRepository);
        ArgumentNullException.ThrowIfNull(currencyRepository);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        ArgumentNullException.ThrowIfNull(unitOfWork);

        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _currencyRepository = currencyRepository;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<PaymentDto>>> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var payments = await _paymentRepository.GetByOrderIdAsync(orderId, cancellationToken);
        var paymentDtos = payments.Select(payment => payment.ToPaymentDto()).ToArray();

        return Result<IReadOnlyList<PaymentDto>>.Success(paymentDtos);
    }

    public async Task<Result<PaymentDto>> RecordPaymentAsync(RecordPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var result = await AddPaymentAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return result;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return result;
    }

    public Task<Result<PaymentDto>> RecordCheckoutPaymentAsync(RecordPaymentRequest request, CancellationToken cancellationToken = default)
    {
        return AddPaymentAsync(request, cancellationToken);
    }

    private async Task<Result<PaymentDto>> AddPaymentAsync(RecordPaymentRequest request, CancellationToken cancellationToken)
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

        return Result<PaymentDto>.Success(orderPayment.ToPaymentDto());
    }

    public async Task<Result<CurrencyDto>> GetCurrencyByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var currency = await _currencyRepository.GetByCodeAsync(code, cancellationToken);
        if (currency == null)
        {
            return Result<CurrencyDto>.NotFound("Currency not found");
        }

        var currencyDto = currency.ToCurrencyDto();
        return Result<CurrencyDto>.Success(currencyDto);
    }
}

