namespace Backend.Api.Contracts.Commerce.Checkout;

public sealed record CheckoutPreviewLineResponse
{
    public required Guid ListingId { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
    public required decimal LineTotal { get; init; }
    public required string CurrencyCode { get; init; }
}
