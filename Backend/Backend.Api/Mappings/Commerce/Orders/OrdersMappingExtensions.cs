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
            UserId = order.UserId,
            ShippingAddressId = order.ShippingAddressId,
            OrderNumber = order.OrderNumber,
            OrderStatus = order.OrderStatus,
            OrderStatusDescription = order.OrderStatusDescription,
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

    public static OrderSummaryModel ToSummaryModel(this App.OrderSummaryDto order) =>
        new()
        {
            Id = order.Id,
            UserId = order.UserId,
            OrderNumber = order.OrderNumber,
            OrderStatus = order.OrderStatus,
            OrderStatusDescription = order.OrderStatusDescription,
            OrderPurchaseTimestampUtc = order.OrderPurchaseTimestampUtc,
            OrderApprovedAtUtc = order.OrderApprovedAtUtc,
            OrderDeliveredCarrierDateUtc = order.OrderDeliveredCarrierDateUtc,
            OrderDeliveredCustomerDateUtc = order.OrderDeliveredCustomerDateUtc,
            OrderEstimatedDeliveryDateUtc = order.OrderEstimatedDeliveryDateUtc,
            SubtotalAmount = order.SubtotalAmount,
            FreightAmount = order.FreightAmount,
            TotalAmount = order.TotalAmount,
            CurrencyCode = order.CurrencyCode,
            Items = order.Items.Select(i => i.ToModel()).ToArray(),
            Shipments = order.Shipments.Select(s => s.ToModel()).ToArray()
        };

    public static SellerOrderSummaryModel ToSellerSummaryModel(this App.SellerOrderSummaryDto order) =>
        new()
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            CustomerName = order.CustomerName,
            CustomerEmail = order.CustomerEmail,
            OrderNumber = order.OrderNumber,
            OrderStatus = order.OrderStatus,
            OrderStatusDescription = order.OrderStatusDescription,
            OrderPurchaseTimestampUtc = order.OrderPurchaseTimestampUtc,
            OrderApprovedAtUtc = order.OrderApprovedAtUtc,
            OrderDeliveredCarrierDateUtc = order.OrderDeliveredCarrierDateUtc,
            OrderDeliveredCustomerDateUtc = order.OrderDeliveredCustomerDateUtc,
            OrderEstimatedDeliveryDateUtc = order.OrderEstimatedDeliveryDateUtc,
            SubtotalAmount = order.SubtotalAmount,
            FreightAmount = order.FreightAmount,
            TotalAmount = order.TotalAmount,
            CurrencyCode = order.CurrencyCode,
            CanUpdateStatus = order.CanUpdateStatus,
            Items = order.Items.Select(i => i.ToModel()).ToArray(),
            Shipments = order.Shipments.Select(s => s.ToModel()).ToArray()
        };

    public static SellerOrderStatsModel ToSellerStatsModel(this App.SellerOrderStatsDto stats) =>
        new()
        {
            TotalOrders = stats.TotalOrders,
            ActiveOrders = stats.ActiveOrders,
            TotalRevenue = stats.TotalRevenue,
            CurrencyCode = stats.CurrencyCode
        };

    public static OrderItemModel ToModel(this App.OrderItemDto item) =>
        new()
        {
            OrderId = item.OrderId,
            OrderItemId = item.OrderItemId,
            ListingId = item.ListingId,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            ImageUrl = item.ImageUrl,
            ProductPhotosQty = item.ProductPhotosQty,
            SellerId = item.SellerId,
            SellerName = item.SellerName,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            LineTotal = item.LineTotal,
            FreightValue = item.FreightValue,
            CurrencyCode = item.CurrencyCode,
            ShippingLimitDateUtc = item.ShippingLimitDateUtc,
            FulfillmentStatus = item.FulfillmentStatus,
            FulfillmentApprovedAtUtc = item.FulfillmentApprovedAtUtc,
            FulfillmentProcessingAtUtc = item.FulfillmentProcessingAtUtc,
            FulfillmentShippedAtUtc = item.FulfillmentShippedAtUtc
        };

    public static App.CancelOrderRequest ToApplicationRequest(this CancelOrderRequest request, Guid orderId) =>
        new(orderId, request.Reason);
}
