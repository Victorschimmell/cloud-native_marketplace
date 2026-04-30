namespace Backend.Api.Contracts.Commerce.Cart;

public sealed record CartItemModel
{
    public required Guid Id { get; init; }
    public required Guid CartId { get; init; }
    public required Guid ListingId { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPriceAtAddition { get; init; }
    public required string CurrencyCode { get; init; }
    public required DateTimeOffset AddedAtUtc { get; init; }
    public required DateTimeOffset UpdatedAtUtc { get; init; }
}
