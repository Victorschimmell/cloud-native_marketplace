namespace Backend.Application.DTOs;

public sealed record ProductDto(
    Guid Id,
    Guid CategoryId,
    string ProductName,
    string Description,
    int ProductNameLength,
    int ProductDescriptionLength,
    int ProductPhotosQty,
    int ProductWeightG,
    int ProductLengthCm,
    int ProductHeightCm,
    int ProductWidthCm);

public sealed record CategoryDto(Guid Id, string CategoryNamePt, string? CategoryNameEn);

public sealed record CreateProductRequest(
    Guid CategoryId,
    string ProductName,
    string Description,
    int ProductPhotosQty,
    int ProductWeightG,
    int ProductLengthCm,
    int ProductHeightCm,
    int ProductWidthCm);

public sealed record UpdateProductRequest(
    Guid ProductId,
    Guid CategoryId,
    string ProductName,
    string Description,
    int ProductPhotosQty,
    int ProductWeightG,
    int ProductLengthCm,
    int ProductHeightCm,
    int ProductWidthCm);

public sealed record CreateCategoryRequest(string CategoryNamePt, string? CategoryNameEn);

public sealed record UpdateCategoryRequest(Guid CategoryId, string CategoryNamePt, string? CategoryNameEn);
