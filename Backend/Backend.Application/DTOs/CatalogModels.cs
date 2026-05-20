namespace Backend.Application.DTOs;

public sealed record ProductDto(
    Guid Id,
    Guid CategoryId,
    string ProductName,
    string Description,
    string? ImageUrl,
    int ProductNameLength,
    int ProductDescriptionLength,
    int ProductPhotosQty,
    int ProductWeightG,
    int ProductLengthCm,
    int ProductHeightCm,
    int ProductWidthCm);

public sealed record BrowseProductDto(
    Guid ProductId,
    Guid ListingId,
    Guid CategoryId,
    string ProductName,
    string Description,
    string? ImageUrl,
    string? CategoryName,
    decimal Price,
    string CurrencyCode,
    int StockQuantity,
    double? AverageReviewScore,
    int ReviewCount,
    int ProductPhotosQty,
    int ProductWeightG,
    int ProductLengthCm,
    int ProductHeightCm,
    int ProductWidthCm);

public sealed record ProductDetailsDto(
    Guid ProductId,
    Guid ListingId,
    Guid CategoryId,
    string ProductName,
    string Description,
    string? ImageUrl,
    string? CategoryName,
    decimal Price,
    string CurrencyCode,
    int StockQuantity,
    int ProductPhotosQty,
    int ProductWeightG,
    int ProductLengthCm,
    int ProductHeightCm,
    int ProductWidthCm,
    Guid SellerId,
    string SellerName,
    string SellerVerificationStatus);

public sealed record BrowseProductsRequest(
    Guid? CategoryId,
    string? Search,
    string Sort,
    string Currency,
    int Page,
    int PageSize);

public sealed record CategoryDto(Guid Id, string CategoryNamePt, string? CategoryNameEn);

public sealed record CreateProductRequest(
    Guid CategoryId,
    string ProductName,
    string Description,
    string? ImageUrl,
    int ProductPhotosQty,
    int ProductWeightG,
    int ProductLengthCm,
    int ProductHeightCm,
    int ProductWidthCm,
    decimal Price,
    int InventoryQuantity);

public sealed record UpdateProductRequest(
    Guid ListingId,
    Guid CategoryId,
    string ProductName,
    string Description,
    string? ImageUrl,
    int ProductPhotosQty,
    int ProductWeightG,
    int ProductLengthCm,
    int ProductHeightCm,
    int ProductWidthCm,
    decimal Price,
    int InventoryQuantity,
    string VisibilityStatus);

public sealed record CreateCategoryRequest(string CategoryNamePt, string? CategoryNameEn);

public sealed record UpdateCategoryRequest(Guid CategoryId, string CategoryNamePt, string? CategoryNameEn);

public sealed record SellerListingDto(
    Guid ListingId,
    Guid ProductId,
    Guid CategoryId,
    string ProductName,
    string Description,
    string? ImageUrl,
    string? CategoryName,
    decimal ListingPrice,
    int InventoryQuantity,
    string VisibilityStatus);
