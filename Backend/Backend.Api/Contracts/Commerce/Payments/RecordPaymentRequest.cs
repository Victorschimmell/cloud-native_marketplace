using Backend.Api.Attributes;
using Backend.Domain.Enums;

namespace Backend.Api.Contracts.Commerce.Payments;

public sealed record RecordPaymentRequest
{
    [NotEmptyGuid]
    public required Guid CurrencyId { get; init; }

    public required PaymentType PaymentType { get; init; }

    public required int PaymentInstallments { get; init; }

    public required decimal PaymentValue { get; init; }

    public string? ExternalPaymentReference { get; init; }
}
