using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Contracts.Catalog.Categories;

public sealed record UpdateCategoryRequest
{
    [MaxLength(200)]
    public required string CategoryNamePt { get; init; }

    [MaxLength(200)]
    public string? CategoryNameEn { get; init; }
}
