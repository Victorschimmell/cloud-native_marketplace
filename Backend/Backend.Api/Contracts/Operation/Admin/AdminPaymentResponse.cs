namespace Backend.Api.Contracts.Operation.Admin;

public sealed record AdminPaymentResponse
{
    public required string Id { get; init; }
    public required Guid OrderId { get; init; }
    public required int PaymentSequential { get; init; }
    public required string CustomerName { get; init; }
    public required DateTimeOffset Date { get; init; }
    public required decimal Amount { get; init; }
    public required string CurrencyCode { get; init; }
    public required string Status { get; init; }
}
