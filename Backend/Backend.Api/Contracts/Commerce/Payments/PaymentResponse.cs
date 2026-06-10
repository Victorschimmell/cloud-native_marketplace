namespace Backend.Api.Contracts.Commerce.Payments;

public sealed record PaymentResponse
{
    public required Guid OrderId { get; init; }
    public required int PaymentSequential { get; init; }
    public required Guid CurrencyId { get; init; }
    public required PaymentType PaymentType { get; init; }
    public required int PaymentInstallments { get; init; }
    public required decimal PaymentValue { get; init; }
    public required PaymentStatus PaymentStatus { get; init; }
    public string? ExternalPaymentReference { get; init; }
    public DateTimeOffset? PaidAtUtc { get; init; }
}
