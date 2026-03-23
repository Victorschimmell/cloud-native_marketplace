using Backend.Domain.Enums;
using Backend.Domain.ValueObjects;

namespace Backend.Domain.Entities.Orders;

public sealed class OrderPayment
{
    public Guid OrderId { get; set; }
    public int PaymentSequential { get; set; }
    public PaymentType PaymentType { get; set; }
    public int PaymentInstallments { get; set; }
    public Money PaymentValue { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string? ExternalPaymentReference { get; set; }
    public DateTimeOffset? PaidAtUtc { get; set; }

    public Order? Order { get; set; }
}
