namespace Backend.Api.Contracts.Catalog.Products;

public sealed record BrowseProductResponse
{
    public required Guid ProductId { get; init; }
    public required Guid ListingId { get; init; }
    public required Guid CategoryId { get; init; }
    public required string ProductName { get; init; }
    public required string Description { get; init; }
    public required string? CategoryName { get; init; }
    public required decimal Price { get; init; }
    public required string CurrencyCode { get; init; }
    public required int StockQuantity { get; init; }
    public required double? AverageReviewScore { get; init; }
    public required int ReviewCount { get; init; }
    public required int ProductPhotosQty { get; init; }
    public required int ProductWeightG { get; init; }
    public required int ProductLengthCm { get; init; }
    public required int ProductHeightCm { get; init; }
    public required int ProductWidthCm { get; init; }
}
