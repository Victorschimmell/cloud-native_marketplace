namespace Backend.Api.Contracts.Catalog.Categories;

public sealed record CategoryResponse
{
    public required Guid Id { get; init; }
    public required string CategoryNamePt { get; init; }
    public string? CategoryNameEn { get; init; }
}
