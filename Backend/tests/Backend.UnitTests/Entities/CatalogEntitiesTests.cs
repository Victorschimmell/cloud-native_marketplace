using Backend.Domain.Entities.Catalog;
using Backend.Domain.Enums;

namespace Backend.UnitTests.Entities;

public class CatalogEntitiesTests
{
    [Fact]
    public void ProductCategory_InitializesProductsCollection()
    {
        var entity = new ProductCategory
        {
            CategoryNamePt = "Hardware"
        };

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Empty(entity.Products);
    }

    [Fact]
    public void Product_StoresDimensionsAndRelatedCollections()
    {
        var entity = new Product
        {
            ProductName = "Headphones",
            Description = "Noise cancelling",
            ProductLengthCm = 20,
            ProductHeightCm = 10,
            ProductWidthCm = 15
        };

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(20, entity.ProductLengthCm);
        Assert.Equal(10, entity.ProductHeightCm);
        Assert.Equal(15, entity.ProductWidthCm);
        Assert.Empty(entity.Listings);
        Assert.Empty(entity.OrderItems);
    }

    [Fact]
    public void ProductListing_StoresPriceAndCollections()
    {
        var entity = new ProductListing
        {
            Sku = "SKU-123",
            ListingPrice = 499.99m,
            VisibilityStatus = ListingVisibilityStatus.Published
        };

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(499.99m, entity.ListingPrice);
        Assert.Equal(ListingVisibilityStatus.Published, entity.VisibilityStatus);
        Assert.Empty(entity.CartItems);
        Assert.Empty(entity.OrderItems);
    }
}
