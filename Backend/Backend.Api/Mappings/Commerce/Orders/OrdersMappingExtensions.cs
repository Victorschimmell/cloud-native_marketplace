using Backend.Api.Contracts.Commerce.Orders;
using Backend.Api.Mappings.Commerce.Payments;
using Backend.Api.Mappings.Commerce.Reviews;
using Backend.Api.Mappings.Commerce.Shipments;
using App = Backend.Application.DTOs;

namespace Backend.Api.Mappings.Commerce.Orders;

public static class OrdersMappingExtensions
{
    public static OrderModel ToModel(this App.OrderDto order) =>
        new()
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            ShippingAddressId = order.ShippingAddressId,
            OrderNumber = order.OrderNumber,
            OrderStatus = (OrderStatus)order.OrderStatus,
            OrderPurchaseTimestampUtc = order.OrderPurchaseTimestampUtc,
            OrderApprovedAtUtc = order.OrderApprovedAtUtc,
            OrderDeliveredCarrierDateUtc = order.OrderDeliveredCarrierDateUtc,
            OrderDeliveredCustomerDateUtc = order.OrderDeliveredCustomerDateUtc,
            OrderEstimatedDeliveryDateUtc = order.OrderEstimatedDeliveryDateUtc,
            SubtotalAmount = order.SubtotalAmount,
            FreightAmount = order.FreightAmount,
            TotalAmount = order.TotalAmount,
            CurrencyCode = order.CurrencyCode,
            PlacedFromCartId = order.PlacedFromCartId,
            Items = order.Items.Select(i => i.ToModel()).ToArray(),
            Payments = order.Payments.Select(p => p.ToModel()).ToArray(),
            Reviews = order.Reviews.Select(r => r.ToModel()).ToArray(),
            Shipments = order.Shipments.Select(s => s.ToModel()).ToArray()
        };

    public static OrderItemModel ToModel(this App.OrderItemDto item) =>
        new()
        {
            OrderId = item.OrderId,
            OrderItemId = item.OrderItemId,
            ListingId = item.ListingId,
            ProductId = item.ProductId,
            SellerId = item.SellerId,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            FreightValue = item.FreightValue,
            CurrencyCode = item.CurrencyCode,
            ShippingLimitDateUtc = item.ShippingLimitDateUtc
        };
}
