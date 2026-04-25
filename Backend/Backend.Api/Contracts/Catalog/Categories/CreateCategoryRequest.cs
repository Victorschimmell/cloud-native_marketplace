using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Contracts.Catalog.Categories;

public sealed record CreateCategoryRequest
{
    [StringLength(200)]
    public required string CategoryNamePt { get; init; }

    [StringLength(200)]
    public string? CategoryNameEn { get; init; }
}
