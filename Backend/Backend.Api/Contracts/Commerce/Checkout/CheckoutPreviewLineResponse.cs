namespace Backend.Api.Contracts.Commerce.Checkout;

public sealed record CheckoutPreviewLineResponse
{
    public required Guid ListingId { get; init; }
    public required Guid ProductId { get; init; }
    public required string ProductName { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
    public required decimal LineTotal { get; init; }
    public required string CurrencyCode { get; init; }
}

public sealed record CheckoutPreviewResponse
{
    public required IReadOnlyList<CheckoutPreviewLineResponse> Lines { get; init; }
    public required decimal SubtotalAmount { get; init; }
    public required decimal FreightAmount { get; init; }
    public required decimal TotalAmount { get; init; }
    public required string CurrencyCode { get; init; }
}
