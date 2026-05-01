using Backend.Api.Contracts.Commerce.Payments;
using App = Backend.Application.DTOs;
using DomainEums = Backend.Domain.Enums;

namespace Backend.Api.Mappings.Commerce.Payments;

public static class PaymentsMappingExtensions
{
    public static App.RecordPaymentDetails ToApplicationRequest(this RecordPaymentRequest request) =>
        new(
            request.CurrencyId,
            request.PaymentType.ToDomain(),
            request.PaymentInstallments,
            request.PaymentValue,
            request.ExternalPaymentReference);

    public static PaymentModel ToModel(this App.PaymentDto payment) =>
        new()
        {
            OrderId = payment.OrderId,
            PaymentSequential = payment.PaymentSequential,
            CurrencyId = payment.CurrencyId,
            PaymentType = (PaymentType)payment.PaymentType,
            PaymentInstallments = payment.PaymentInstallments,
            PaymentValue = payment.PaymentValue,
            PaymentStatus = (PaymentStatus)payment.PaymentStatus,
            ExternalPaymentReference = payment.ExternalPaymentReference,
            PaidAtUtc = payment.PaidAtUtc
        };

    public static CurrencyResponse ToResponse(this App.CurrencyDto currency) =>
        new()
        {
            Id = currency.Id,
            Code = currency.Code,
            Name = currency.Name,
            Symbol = currency.Symbol
        };

    private static DomainEums.PaymentType ToDomain(this PaymentType paymentType) =>
        paymentType switch
        {
            PaymentType.CreditCard => DomainEums.PaymentType.CreditCard,
            PaymentType.DebitCard => DomainEums.PaymentType.DebitCard,
            PaymentType.Voucher => DomainEums.PaymentType.Voucher,
            PaymentType.BankTransfer => DomainEums.PaymentType.BankTransfer,
            PaymentType.Pix => DomainEums.PaymentType.Pix,
            PaymentType.Wallet => DomainEums.PaymentType.Wallet,
            PaymentType.Other => DomainEums.PaymentType.Other,
            _ => throw new ArgumentOutOfRangeException(nameof(paymentType), paymentType, null)
        };
}
