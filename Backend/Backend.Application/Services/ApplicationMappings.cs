using Backend.Application.DTOs;
using Backend.Domain.Entities.Carts;
using Backend.Domain.Entities.Catalog;
using Backend.Domain.Entities.IdentityAccess;
using Backend.Domain.Entities.Operations;
using Backend.Domain.Entities.Orders;

namespace Backend.Application.Services;

internal static class ApplicationMappings
{
    public static CustomerDto ToDto(this Customer customer) =>
        new(
            customer.Id,
            customer.UserId,
            customer.FirstName,
            customer.LastName,
            customer.Phone,
            customer.DefaultAddressId,
            customer.OlistCustomerId,
            customer.OlistCustomerUniqueId);

    public static SellerDto ToDto(this Seller seller) =>
        new(
            seller.Id,
            seller.UserId,
            seller.BusinessName,
            seller.RegistrationNumber,
            seller.PayoutInformation,
            seller.DefaultAddressId,
            seller.VerificationStatus,
            seller.VerifiedAtUtc,
            seller.OlistSellerId);

    public static UserAccountDto ToDto(this UserAccount userAccount) =>
        new(
            userAccount.Id,
            userAccount.Email.Value,
            userAccount.IsAdmin,
            userAccount.IsBlocked,
            userAccount.AccountStatus,
            userAccount.FailedLoginAttempts,
            userAccount.LockedUntilUtc,
            userAccount.LastLoginAtUtc);

    public static ProductDto ToDto(this Product product) =>
        new(
            product.Id,
            product.CategoryId,
            product.ProductName,
            product.Description,
            product.ProductNameLength,
            product.ProductDescriptionLength,
            product.ProductPhotosQty,
            product.ProductWeightG,
            product.ProductLengthCm,
            product.ProductHeightCm,
            product.ProductWidthCm);

    public static CategoryDto ToDto(this ProductCategory category) =>
        new(category.Id, category.CategoryNamePt, category.CategoryNameEn);

    public static CartItemDto ToDto(this CartItem item) =>
        new(item.Id, item.CartId, item.ListingId, item.Quantity, item.UnitPriceAtAddition, item.AddedAtUtc, item.UpdatedAtUtc);

    public static CartDto ToDto(this ShoppingCart cart) =>
        new(cart.Id, cart.UserId, cart.SessionId, cart.Status.ToString(), cart.ExpiresAtUtc, cart.Items.Select(ToDto).ToArray());

    public static OrderItemDto ToDto(this OrderItem item) =>
        new(
            item.OrderId,
            item.OrderItemId,
            item.ListingId,
            item.ProductId,
            item.SellerId,
            item.Quantity,
            item.UnitPrice,
            item.FreightValue,
            item.ShippingLimitDateUtc);

    public static PaymentDto ToDto(this OrderPayment payment) =>
        new(
            payment.OrderId,
            payment.PaymentSequential,
            payment.CurrencyId,
            payment.PaymentType,
            payment.PaymentInstallments,
            payment.PaymentValue,
            payment.PaymentStatus,
            payment.ExternalPaymentReference,
            payment.PaidAtUtc);

    public static ReviewDto ToDto(this OrderReview review) =>
        new(
            review.Id,
            review.OrderId,
            review.ReviewScore,
            review.ReviewCommentTitle,
            review.ReviewCommentMessage,
            review.ReviewCreationDateUtc,
            review.ReviewAnswerTimestampUtc);

    public static ShipmentDto ToDto(this Shipment shipment) =>
        new(
            shipment.Id,
            shipment.OrderId,
            shipment.SellerId,
            shipment.CarrierName,
            shipment.TrackingNumber,
            shipment.ShipmentStatus,
            shipment.ShippedAtUtc,
            shipment.DeliveredAtUtc,
            shipment.ReturnedAtUtc);

    public static OrderDto ToDto(this Order order) =>
        new(
            order.Id,
            order.CustomerId,
            order.ShippingAddressId,
            order.OrderNumber,
            order.OrderStatus,
            order.OrderPurchaseTimestampUtc,
            order.OrderApprovedAtUtc,
            order.OrderDeliveredCarrierDateUtc,
            order.OrderDeliveredCustomerDateUtc,
            order.OrderEstimatedDeliveryDateUtc,
            order.SubtotalAmount,
            order.FreightAmount,
            order.TotalAmount,
            order.PlacedFromCartId,
            order.Items.Select(ToDto).ToArray(),
            order.Payments.Select(ToDto).ToArray(),
            order.Reviews.Select(ToDto).ToArray(),
            order.Shipments.Select(ToDto).ToArray());

    public static AuditLogEntryDto ToDto(this AuditLog auditLog) =>
        new(
            auditLog.Id,
            auditLog.ActorUserId,
            auditLog.ActorIpAddress,
            auditLog.ActionType,
            auditLog.TargetEntityType,
            auditLog.TargetEntityId,
            auditLog.Outcome,
            auditLog.Details,
            auditLog.CreatedAtUtc);

    public static SellerVerificationRequestDto ToDto(this SellerVerificationRequest request) =>
        new(
            request.Id,
            request.SellerId,
            request.SubmittedAtUtc,
            request.Status,
            request.BusinessNameSnapshot,
            request.RegistrationNumberSnapshot,
            request.SubmittedDetails,
            request.ReviewNotes,
            request.ReviewedByUserId,
            request.ReviewedAtUtc,
            request.RejectionReason);
}
