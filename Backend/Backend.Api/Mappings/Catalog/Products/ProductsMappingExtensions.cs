using Backend.Api.Contracts.Catalog.Products;
using App = Backend.Application.DTOs;

namespace Backend.Api.Mappings.Catalog.Products;

public static class ProductsMappingExtensions
{
    public static ProductDetailsResponse ToResponse(this App.ProductDetailsDto product)
    {
        return new ProductDetailsResponse
        {
            ProductId = product.ProductId,
            ListingId = product.ListingId,
            CategoryId = product.CategoryId,
            ProductName = product.ProductName,
            Description = product.Description,
            CategoryName = product.CategoryName,
            Price = product.Price,
            CurrencyCode = product.CurrencyCode,
            StockQuantity = product.StockQuantity,
            ProductPhotosQty = product.ProductPhotosQty,
            ProductWeightG = product.ProductWeightG,
            ProductLengthCm = product.ProductLengthCm,
            ProductHeightCm = product.ProductHeightCm,
            ProductWidthCm = product.ProductWidthCm,
            SellerId = product.SellerId,
            SellerName = product.SellerName,
            SellerVerificationStatus = product.SellerVerificationStatus
        };
    }

    public static BrowseProductResponse ToResponse(this App.BrowseProductDto product)
    {
        return new BrowseProductResponse
        {
            ProductId = product.ProductId,
            ListingId = product.ListingId,
            CategoryId = product.CategoryId,
            ProductName = product.ProductName,
            Description = product.Description,
            CategoryName = product.CategoryName,
            Price = product.Price,
            CurrencyCode = product.CurrencyCode,
            StockQuantity = product.StockQuantity,
            AverageReviewScore = product.AverageReviewScore,
            ReviewCount = product.ReviewCount,
            ProductPhotosQty = product.ProductPhotosQty,
            ProductWeightG = product.ProductWeightG,
            ProductLengthCm = product.ProductLengthCm,
            ProductHeightCm = product.ProductHeightCm,
            ProductWidthCm = product.ProductWidthCm
        };
    }

    public static ProductResponse ToResponse(this App.ProductDto product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            CategoryId = product.CategoryId,
            ProductName = product.ProductName,
            Description = product.Description,
            ProductNameLength = product.ProductNameLength,
            ProductDescriptionLength = product.ProductDescriptionLength,
            ProductPhotosQty = product.ProductPhotosQty,
            ProductWeightG = product.ProductWeightG,
            ProductLengthCm = product.ProductLengthCm,
            ProductHeightCm = product.ProductHeightCm,
            ProductWidthCm = product.ProductWidthCm
        };
    }

    public static App.CreateProductRequest ToDto(this CreateProductRequest request)
    {
        return new App.CreateProductRequest(
            CategoryId: request.CategoryId,
            ProductName: request.ProductName,
            Description: request.Description,
            ProductPhotosQty: request.ProductPhotosQty,
            ProductWeightG: request.ProductWeightG,
            ProductLengthCm: request.ProductLengthCm,
            ProductHeightCm: request.ProductHeightCm,
            ProductWidthCm: request.ProductWidthCm,
            Price: request.Price,
            InventoryQuantity: request.InventoryQuantity);
    }

    public static App.UpdateProductRequest ToDto(this UpdateProductRequest request, Guid listingId)
    {
        return new App.UpdateProductRequest(
            ListingId: listingId,
            CategoryId: request.CategoryId,
            ProductName: request.ProductName,
            Description: request.Description,
            ProductPhotosQty: request.ProductPhotosQty,
            ProductWeightG: request.ProductWeightG,
            ProductLengthCm: request.ProductLengthCm,
            ProductHeightCm: request.ProductHeightCm,
            ProductWidthCm: request.ProductWidthCm,
            Price: request.Price,
            InventoryQuantity: request.InventoryQuantity,
            VisibilityStatus: request.VisibilityStatus);
    }

    public static SellerListingResponse ToResponse(this App.SellerListingDto listing)
    {
        return new SellerListingResponse
        {
            ListingId = listing.ListingId,
            ProductId = listing.ProductId,
            CategoryId = listing.CategoryId,
            ProductName = listing.ProductName,
            Description = listing.Description,
            CategoryName = listing.CategoryName,
            ListingPrice = listing.ListingPrice,
            InventoryQuantity = listing.InventoryQuantity,
            VisibilityStatus = listing.VisibilityStatus,
        };
    }
}
