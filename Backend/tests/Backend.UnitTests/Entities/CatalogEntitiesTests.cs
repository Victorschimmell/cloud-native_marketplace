using Backend.Domain.Entities.Catalog;
using Backend.Domain.Enums;
using Backend.Domain.ValueObjects;

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
            DimensionsCm = new ProductDimensions(20, 10, 15)
        };

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(new ProductDimensions(20, 10, 15), entity.DimensionsCm);
        Assert.Empty(entity.Listings);
        Assert.Empty(entity.OrderItems);
    }

    [Fact]
    public void ProductListing_StoresPriceAndCollections()
    {
        var entity = new ProductListing
        {
            Sku = "SKU-123",
            ListingPrice = new Money(499.99m, "DKK"),
            VisibilityStatus = ListingVisibilityStatus.Published
        };

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal("499.99 DKK", entity.ListingPrice.ToString());
        Assert.Equal(ListingVisibilityStatus.Published, entity.VisibilityStatus);
        Assert.Empty(entity.CartItems);
        Assert.Empty(entity.OrderItems);
    }
}
