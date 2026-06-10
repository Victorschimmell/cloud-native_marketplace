using Backend.Api.Contracts.Commerce.Payments;
using App = Backend.Application.DTOs;

namespace Backend.Api.Mappings.Commerce.Payments;

public static class PaymentsMappingExtensions
{
    public static App.RecordPaymentDetails ToApplicationRequest(this RecordPaymentRequest request) =>
        new(
            request.CurrencyId,
            request.PaymentType,
            request.PaymentInstallments,
            request.PaymentValue,
            request.ExternalPaymentReference);

    public static PaymentModel ToModel(this App.PaymentDto payment) =>
        new()
        {
            OrderId = payment.OrderId,
            PaymentSequential = payment.PaymentSequential,
            CurrencyId = payment.CurrencyId,
            PaymentType = payment.PaymentType,
            PaymentInstallments = payment.PaymentInstallments,
            PaymentValue = payment.PaymentValue,
            PaymentStatus = payment.PaymentStatus,
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
}
