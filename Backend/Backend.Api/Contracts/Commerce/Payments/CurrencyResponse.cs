namespace Backend.Api.Contracts.Commerce.Payments;

public sealed record CurrencyResponse
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public string? Symbol { get; init; }
}
