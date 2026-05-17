namespace Backend.Infrastructure.Persistence.Import.Models;

public sealed record OrderPaymentRow(
    string OrderId,
    int PaymentSequential,
    string PaymentType,
    int PaymentInstallments,
    decimal PaymentValue);
