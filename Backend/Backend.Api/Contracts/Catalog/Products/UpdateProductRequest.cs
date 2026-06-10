using Backend.Api.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Contracts.Catalog.Products;

public sealed record UpdateProductRequest
{
    [NotEmptyGuid]
    public required Guid CategoryId { get; init; }

    [MaxLength(500)]
    public required string ProductName { get; init; }
    public required string Description { get; init; }
    public required int ProductPhotosQty { get; init; }
    public required int ProductWeightG { get; init; }
    public required int ProductLengthCm { get; init; }
    public required int ProductHeightCm { get; init; }
    public required int ProductWidthCm { get; init; }
}
