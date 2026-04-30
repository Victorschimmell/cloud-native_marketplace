using Backend.Application.DTOs;
using Backend.Domain.Entities.Carts;
using Backend.Domain.Entities.Catalog;
using Backend.Domain.Entities.IdentityAccess;
using Backend.Domain.Entities.Operations;
using Backend.Domain.Entities.Orders;
using Backend.Domain.Enums;

namespace Backend.Application.Services;

internal static class ApplicationMappings
{
    public static CustomerDto ToCustomerDto(this Customer customer) =>
        new(
            customer.Id,
            customer.UserId,
            customer.FirstName,
            customer.LastName,
            customer.Phone,
            customer.DefaultAddressId,
            customer.OlistCustomerId,
            customer.OlistCustomerUniqueId);

    public static SellerDto ToSellerDto(this Seller seller) =>
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

    public static UserAccountDto ToUserAccountDto(this UserAccount userAccount) =>
        new(
            userAccount.Id,
            userAccount.Email.Value,
            userAccount.IsAdmin,
            userAccount.IsBlocked,
            userAccount.AccountStatus,
            userAccount.FailedLoginAttempts,
            userAccount.LockedUntilUtc,
            userAccount.LastLoginAtUtc);

    public static ProductDto ToProductDto(this Product product) =>
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

    public static BrowseProductDto ToBrowseDto(this ProductListing listing, string currencyCode, decimal convertedPrice)
    {
        var product = listing.Product ?? throw new InvalidOperationException("Product listing must include product details.");

        return new BrowseProductDto(
            product.Id,
            listing.Id,
            product.CategoryId,
            product.ProductName,
            product.Description,
            product.Category?.CategoryNameEn ?? product.Category?.CategoryNamePt,
            convertedPrice,
            currencyCode,
            listing.InventoryQuantity,
            product.ProductPhotosQty,
            product.ProductWeightG,
            product.ProductLengthCm,
            product.ProductHeightCm,
            product.ProductWidthCm);
    }

    public static ProductDetailsDto ToProductDetailsDto(this ProductListing listing, string currencyCode, decimal convertedPrice)
    {
        var product = listing.Product ?? throw new InvalidOperationException("Product listing must include product details.");
        var seller = listing.Seller ?? throw new InvalidOperationException("Product listing must include seller details.");

        return new ProductDetailsDto(
            product.Id,
            listing.Id,
            product.CategoryId,
            product.ProductName,
            product.Description,
            product.Category?.CategoryNameEn ?? product.Category?.CategoryNamePt,
            convertedPrice,
            currencyCode,
            listing.InventoryQuantity,
            product.ProductPhotosQty,
            product.ProductWeightG,
            product.ProductLengthCm,
            product.ProductHeightCm,
            product.ProductWidthCm,
            seller.Id,
            seller.BusinessName,
            seller.VerificationStatus.ToString());
    }

    public static CategoryDto ToCategoryDto(this ProductCategory category) =>
        new(category.Id, category.CategoryNamePt, category.CategoryNameEn);

    public static CartItemDto ToCartItemDto(this CartItem item, string currencyCode, Func<decimal, decimal> priceConverter) =>
        new(
            item.Id,
            item.CartId,
            item.ListingId,
            item.Quantity,
            priceConverter(item.UnitPriceAtAddition),
            currencyCode,
            item.AddedAtUtc,
            item.UpdatedAtUtc);

    public static CartDto ToCartDto(this ShoppingCart cart, string currencyCode, Func<decimal, decimal> priceConverter) =>
        new(
            cart.Id,
            cart.UserId,
            cart.SessionId,
            cart.Status.ToString(),
            cart.ExpiresAtUtc,
            cart.Items.Select(item => item.ToCartItemDto(currencyCode, priceConverter)).ToArray());

    public static OrderItemDto ToOrderItemDto(this OrderItem item) =>
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

    public static PaymentDto ToPaymentDto(this OrderPayment payment) =>
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

    public static ReviewDto ToReviewDto(this OrderReview review) =>
        new(
            review.Id,
            review.OrderId,
            review.ReviewScore,
            review.ReviewCommentTitle,
            review.ReviewCommentMessage,
            review.ReviewCreationDateUtc,
            review.ReviewAnswerTimestampUtc);

    public static ShipmentDto ToShipmentDto(this Shipment shipment) =>
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

    public static OrderDto ToOrderDto(this Order order) =>
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
            order.Items.Select(ToOrderItemDto).ToArray(),
            order.Payments.Select(ToPaymentDto).ToArray(),
            order.Reviews.Select(ToReviewDto).ToArray(),
            order.Shipments.Select(ToShipmentDto).ToArray());

    public static AuditLogEntryDto ToAuditLogEntryDto(this AuditLog auditLog) =>
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

    public static SellerVerificationRequestDto ToSellerVerificationRequestDto(this SellerVerificationRequest request) =>
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
