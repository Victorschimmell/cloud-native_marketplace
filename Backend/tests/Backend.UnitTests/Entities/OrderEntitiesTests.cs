using Backend.Domain.Entities.Orders;
using Backend.Domain.Enums;
using Backend.Domain.ValueObjects;

namespace Backend.UnitTests.Entities;

public class OrderEntitiesTests
{
    [Fact]
    public void Order_InitializesChildCollections()
    {
        var entity = new Order
        {
            OrderNumber = "ORD-2026-0001"
        };

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Empty(entity.Items);
        Assert.Empty(entity.Payments);
        Assert.Empty(entity.Reviews);
        Assert.Empty(entity.Shipments);
        Assert.Empty(entity.Notifications);
    }

    [Fact]
    public void OrderItem_StoresPriceSnapshots()
    {
        var entity = new OrderItem
        {
            UnitPrice = new Money(100m, "DKK"),
            FreightValue = new Money(10m, "DKK")
        };

        Assert.Equal("100.00 DKK", entity.UnitPrice.ToString());
        Assert.Equal("10.00 DKK", entity.FreightValue.ToString());
    }

    [Fact]
    public void OrderPayment_StoresPaymentMetadata()
    {
        var entity = new OrderPayment
        {
            PaymentType = PaymentType.CreditCard,
            PaymentStatus = PaymentStatus.Paid,
            PaymentValue = new Money(110m, "DKK")
        };

        Assert.Equal(PaymentType.CreditCard, entity.PaymentType);
        Assert.Equal(PaymentStatus.Paid, entity.PaymentStatus);
        Assert.Equal("110.00 DKK", entity.PaymentValue.ToString());
    }

    [Fact]
    public void OrderReview_InitializesIdentity()
    {
        var entity = new OrderReview();

        Assert.NotEqual(Guid.Empty, entity.Id);
    }

    [Fact]
    public void Shipment_StoresCarrierData()
    {
        var entity = new Shipment
        {
            CarrierName = "DHL",
            TrackingNumber = "TRACK123",
            ShipmentStatus = ShipmentStatus.InTransit
        };

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal("DHL", entity.CarrierName);
        Assert.Equal(ShipmentStatus.InTransit, entity.ShipmentStatus);
    }
}
