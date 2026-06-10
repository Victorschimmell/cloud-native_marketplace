using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Services;
using Backend.Domain.Entities.Orders;
using Backend.Domain.Enums;
using Backend.UnitTests.Application.Fakes;

namespace Backend.UnitTests.Application.Services;

public sealed class PaymentServiceTests
{
    [Fact]
    public async Task GetByOrderAsync_WhenPaymentsExist_ReturnsPaymentDtosForOrder()
    {
        var orderId = Guid.NewGuid();
        var otherOrderId = Guid.NewGuid();
        var currency = CreateCurrency();
        var paymentRepository = new FakePaymentRepository();
        paymentRepository.Payments.Add(CreatePayment(orderId, 1, currency.Id, PaymentStatus.Paid));
        paymentRepository.Payments.Add(CreatePayment(otherOrderId, 1, currency.Id, PaymentStatus.Pending));
        var service = CreateService(paymentRepository);

        var result = await service.GetByOrderAsync(orderId, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        var payment = Assert.Single(result.Value!);
        Assert.Equal(orderId, payment.OrderId);
        Assert.Equal(1, payment.PaymentSequential);
        Assert.Equal(PaymentStatus.Paid, payment.PaymentStatus);
    }

    [Fact]
    public async Task RecordPaymentAsync_WhenRequestIsValid_AddsPendingPaymentCommitsAndWritesAuditLog()
    {
        var paymentRepository = new FakePaymentRepository();
        var unitOfWork = new FakeUnitOfWork();
        var auditLogService = new FakeAuditLogService();
        var service = CreateService(paymentRepository, unitOfWork: unitOfWork, auditLogService: auditLogService);

        var result = await service.RecordPaymentAsync(CreateRecordPaymentRequest(), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, paymentRepository.AddCalls);
        Assert.Equal(1, result.Value!.PaymentSequential);
        Assert.Equal(PaymentStatus.Pending, result.Value.PaymentStatus);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.Contains("Payment 1 recorded", Assert.Single(auditLogService.Entries).Details);
    }

    [Fact]
    public async Task RecordCheckoutPaymentAsync_WhenRequestIsValid_AddsPaymentWithoutCommittingOrAuditing()
    {
        var paymentRepository = new FakePaymentRepository();
        var unitOfWork = new FakeUnitOfWork();
        var auditLogService = new FakeAuditLogService();
        var service = CreateService(paymentRepository, unitOfWork: unitOfWork, auditLogService: auditLogService);

        var result = await service.RecordCheckoutPaymentAsync(CreateRecordPaymentRequest(), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, paymentRepository.AddCalls);
        Assert.Equal(PaymentStatus.Pending, result.Value!.PaymentStatus);
        Assert.Equal(0, unitOfWork.SaveChangesCalls);
        Assert.Empty(auditLogService.Entries);
    }

    [Fact]
    public async Task GetCurrencyByCodeAsync_WhenCurrencyDoesNotExist_ReturnsNotFound()
    {
        var service = CreateService(currencyRepository: new FakeCurrencyRepository());

        var result = await service.GetCurrencyByCodeAsync("USD", TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task GetCurrencyByCodeAsync_WhenCurrencyExists_ReturnsCurrencyDto()
    {
        var currencyRepository = new FakeCurrencyRepository
        {
            Currency = CreateCurrency("DKK", "Danish krone", "kr")
        };
        var service = CreateService(currencyRepository: currencyRepository);

        var result = await service.GetCurrencyByCodeAsync("dkk", TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("DKK", result.Value!.Code);
        Assert.Equal("Danish krone", result.Value.Name);
        Assert.Equal("kr", result.Value.Symbol);
    }

    private static PaymentService CreateService(
        FakePaymentRepository? paymentRepository = null,
        FakeOrderRepository? orderRepository = null,
        FakeCurrencyRepository? currencyRepository = null,
        FakeDateTimeProvider? dateTimeProvider = null,
        FakeAuditLogService? auditLogService = null,
        FakeUnitOfWork? unitOfWork = null) =>
        new(
            paymentRepository ?? new FakePaymentRepository(),
            orderRepository ?? new FakeOrderRepository(),
            currencyRepository ?? new FakeCurrencyRepository(),
            dateTimeProvider ?? new FakeDateTimeProvider(),
            auditLogService ?? new FakeAuditLogService(),
            unitOfWork ?? new FakeUnitOfWork());

    private static RecordPaymentRequest CreateRecordPaymentRequest(Guid? orderId = null, Guid? currencyId = null) =>
        new(
            orderId ?? Guid.NewGuid(),
            new RecordPaymentDetails(
                currencyId ?? Guid.NewGuid(),
                PaymentType.CreditCard,
                2,
                199.90m,
                "pay-ext-1"));

    private static OrderPayment CreatePayment(Guid orderId, int paymentSequential, Guid currencyId, PaymentStatus status) =>
        new()
        {
            OrderId = orderId,
            PaymentSequential = paymentSequential,
            CurrencyId = currencyId,
            PaymentType = PaymentType.CreditCard,
            PaymentInstallments = 1,
            PaymentValue = 49.90m,
            PaymentStatus = status,
            ExternalPaymentReference = "pay-ext-1",
            PaidAtUtc = status == PaymentStatus.Paid ? new DateTimeOffset(2026, 4, 9, 12, 0, 0, TimeSpan.Zero) : null
        };

    private static Currency CreateCurrency(string code = "BRL", string name = "Brazilian real", string? symbol = "R$") =>
        new()
        {
            Code = code,
            Name = name,
            Symbol = symbol
        };
}
