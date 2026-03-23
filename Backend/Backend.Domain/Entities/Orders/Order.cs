using Backend.Domain.Base;
using Backend.Domain.Entities.Location;
using Backend.Domain.Enums;
using Backend.Domain.ValueObjects;

namespace Backend.Domain.Entities.Orders;

public sealed class Order : AggregateRoot<Guid>
{
    public Order()
    {
        Id = Guid.NewGuid();
    }

    public Guid CustomerId { get; set; }
    public Guid ShippingAddressId { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public DateTimeOffset OrderPurchaseTimestampUtc { get; set; }
    public DateTimeOffset? OrderApprovedAtUtc { get; set; }
    public DateTimeOffset? OrderDeliveredCarrierDateUtc { get; set; }
    public DateTimeOffset? OrderDeliveredCustomerDateUtc { get; set; }
    public DateTimeOffset? OrderEstimatedDeliveryDateUtc { get; set; }
    public Money SubtotalAmount { get; set; }
    public Money FreightAmount { get; set; }
    public Money TotalAmount { get; set; }
    public Guid? PlacedFromCartId { get; set; }
    public required string OrderNumber { get; set; }

    public IdentityAccess.Customer? Customer { get; set; }
    public Address? ShippingAddress { get; set; }
    public Carts.ShoppingCart? PlacedFromCart { get; set; }
    public ICollection<OrderItem> Items { get; } = [];
    public ICollection<OrderPayment> Payments { get; } = [];
    public ICollection<OrderReview> Reviews { get; } = [];
    public ICollection<Shipment> Shipments { get; } = [];
    public ICollection<Operations.Notification> Notifications { get; } = [];
}


