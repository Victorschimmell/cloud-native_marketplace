using Backend.Domain.Entities.Carts;
using Backend.Domain.Enums;
using Backend.Domain.ValueObjects;

namespace Backend.UnitTests.Entities;

public class CartEntitiesTests
{
    [Fact]
    public void ShoppingCart_InitializesNestedCollections()
    {
        var entity = new ShoppingCart
        {
            Status = CartStatus.Active
        };

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(CartStatus.Active, entity.Status);
        Assert.Empty(entity.RecoveredCarts);
        Assert.Empty(entity.Items);
        Assert.Empty(entity.OrdersPlacedFromCart);
    }

    [Fact]
    public void CartItem_StoresAddedPriceSnapshot()
    {
        var entity = new CartItem
        {
            UnitPriceAtAddition = new Money(15.50m, "DKK")
        };

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal("15.50 DKK", entity.UnitPriceAtAddition.ToString());
    }
}
