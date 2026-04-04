using Backend.Domain.Entities.Orders;
using Backend.Domain.Enums;

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
    }

    [Fact]
    public void OrderItem_StoresPriceSnapshots()
    {
        var entity = new OrderItem
        {
            UnitPrice = 100m,
            FreightValue = 10m
        };

        Assert.Equal(100m, entity.UnitPrice);
        Assert.Equal(10m, entity.FreightValue);
    }

    [Fact]
    public void OrderPayment_StoresPaymentMetadata()
    {
        var entity = new OrderPayment
        {
            CurrencyId = Guid.NewGuid(),
            PaymentType = PaymentType.CreditCard,
            PaymentStatus = PaymentStatus.Paid,
            PaymentValue = 110m
        };

        Assert.NotEqual(Guid.Empty, entity.CurrencyId);
        Assert.Equal(PaymentType.CreditCard, entity.PaymentType);
        Assert.Equal(PaymentStatus.Paid, entity.PaymentStatus);
        Assert.Equal(110m, entity.PaymentValue);
    }

    [Fact]
    public void OrderReview_InitializesIdentity()
    {
        var entity = new OrderReview
        {
            OlistReviewId = "review-1"
        };

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal("review-1", entity.OlistReviewId);
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

    [Fact]
    public void Currency_InitializesOrderPaymentsCollection()
    {
        var entity = new Currency
        {
            Code = "DKK",
            Name = "Danish Krone",
            Symbol = "kr."
        };

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal("DKK", entity.Code);
        Assert.Empty(entity.OrderPayments);
    }
}
