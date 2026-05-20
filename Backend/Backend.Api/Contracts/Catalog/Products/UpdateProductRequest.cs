using Backend.Api.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Contracts.Catalog.Products;

public sealed record UpdateProductRequest
{
    [NotEmptyGuid]
    public required Guid CategoryId { get; init; }

    [MaxLength(500)]
    [MinLength(1)]
    public required string ProductName { get; init; }
    [MinLength(1)]
    public required string Description { get; init; }
    [MaxLength(2048)]
    [Url]
    public string? ImageUrl { get; init; }
    [Range(0, double.MaxValue)]
    public required decimal Price { get; init; }
    [Range(0, int.MaxValue)]
    public required int InventoryQuantity { get; init; }
    [Range(0, int.MaxValue)]
    public int ProductPhotosQty { get; init; } = 0;
    [Range(0, int.MaxValue)]
    public int ProductWeightG { get; init; } = 0;
    [Range(0, int.MaxValue)]
    public int ProductLengthCm { get; init; } = 0;
    [Range(0, int.MaxValue)]
    public int ProductHeightCm { get; init; } = 0;
    [Range(0, int.MaxValue)]
    public int ProductWidthCm { get; init; } = 0;
    public string VisibilityStatus { get; init; } = "Draft";
}
