namespace Backend.Api.Contracts.Catalog.Products;

public sealed record ProductModel
{
    public required Guid Id { get; init; }
    public required Guid CategoryId { get; init; }
    public required string ProductName { get; init; }
    public required string Description { get; init; }
    public required int ProductNameLength { get; init; }
    public required int ProductDescriptionLength { get; init; }
    public required int ProductPhotosQty { get; init; }
    public required int ProductWeightG { get; init; }
    public required int ProductLengthCm { get; init; }
    public required int ProductHeightCm { get; init; }
    public required int ProductWidthCm { get; init; }
}
