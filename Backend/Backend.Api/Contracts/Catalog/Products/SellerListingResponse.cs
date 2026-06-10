namespace Backend.Api.Contracts.Catalog.Products;

public sealed record SellerListingResponse
{
    public required Guid ListingId { get; init; }
    public required Guid ProductId { get; init; }
    public required Guid CategoryId { get; init; }
    public required string ProductName { get; init; }
    public required string Description { get; init; }
    public string? ImageUrl { get; init; }
    public string? CategoryName { get; init; }
    public required decimal ListingPrice { get; init; }
    public required string CurrencyCode { get; init; }
    public required int InventoryQuantity { get; init; }
    public required string VisibilityStatus { get; init; }
}
