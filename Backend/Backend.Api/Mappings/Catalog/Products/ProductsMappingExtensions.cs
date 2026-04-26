using Backend.Api.Contracts.Catalog.Products;
using App = Backend.Application.DTOs;

namespace Backend.Api.Mappings.Catalog.Products;

public static class ProductsMappingExtensions
{
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
            StockQuantity = product.StockQuantity,
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
}
