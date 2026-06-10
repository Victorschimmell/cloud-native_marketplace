namespace Backend.Api.Contracts.Catalog.Products;

public sealed record ProductDetailsResponse
{
    public required Guid ProductId { get; init; }
    public required Guid ListingId { get; init; }
    public required Guid CategoryId { get; init; }
    public required string ProductName { get; init; }
    public required string Description { get; init; }
    public string? ImageUrl { get; init; }
    public required string? CategoryName { get; init; }
    public required decimal Price { get; init; }
    public required string CurrencyCode { get; init; }
    public required int StockQuantity { get; init; }
    public required int ProductPhotosQty { get; init; }
    public required int ProductWeightG { get; init; }
    public required int ProductLengthCm { get; init; }
    public required int ProductHeightCm { get; init; }
    public required int ProductWidthCm { get; init; }
    public required Guid SellerId { get; init; }
    public required string SellerName { get; init; }
    public required string SellerVerificationStatus { get; init; }
}
