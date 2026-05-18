using Backend.Api.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Contracts.Catalog.Products;

public sealed record CreateProductRequest
{
    [NotEmptyGuid]
    public required Guid CategoryId { get; init; }

    [MaxLength(500)]
    public required string ProductName { get; init; }
    public required string Description { get; init; }
    public required decimal Price { get; init; }
    public required bool InStock { get; init; }
    public int ProductPhotosQty { get; init; } = 0;
    public int ProductWeightG { get; init; } = 0;
    public int ProductLengthCm { get; init; } = 0;
    public int ProductHeightCm { get; init; } = 0;
    public int ProductWidthCm { get; init; } = 0;
}
