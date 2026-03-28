using Backend.Domain.Enums;
using Backend.Domain.ValueObjects;

namespace Backend.Infrastructure.Persistence.Import;

public static class OlistImportValueMapper
{
    public static OrderStatus MapOrderStatus(string sourceStatus) =>
        sourceStatus.Trim().ToLowerInvariant() switch
        {
            "created" => OrderStatus.Pending,
            "approved" => OrderStatus.Approved,
            "invoiced" => OrderStatus.Approved,
            "processing" => OrderStatus.Processing,
            "shipped" => OrderStatus.Shipped,
            "delivered" => OrderStatus.Delivered,
            "canceled" => OrderStatus.Cancelled,
            "unavailable" => OrderStatus.Cancelled,
            _ => OrderStatus.Pending
        };

    public static PaymentType MapPaymentType(string sourceType) =>
        sourceType.Trim().ToLowerInvariant() switch
        {
            "credit_card" => PaymentType.CreditCard,
            "debit_card" => PaymentType.DebitCard,
            "voucher" => PaymentType.Voucher,
            "boleto" => PaymentType.BankTransfer,
            "pix" => PaymentType.Pix,
            "wallet" => PaymentType.Wallet,
            _ => PaymentType.Other
        };

    public static EmailAddress CreateCustomerEmail(string customerUniqueId) =>
        new($"customer-{customerUniqueId.ToLowerInvariant()}@olist.import.local");

    public static EmailAddress CreateSellerEmail(string sellerId) =>
        new($"seller-{sellerId.ToLowerInvariant()}@olist.import.local");

    public static string CreateCustomerLastName(string customerUniqueId) =>
        $"Imported-{customerUniqueId[..Math.Min(8, customerUniqueId.Length)]}";

    public static string CreateSellerBusinessName(string sellerId) =>
        $"Olist Seller {sellerId[..Math.Min(8, sellerId.Length)]}";

    public static string CreateRegistrationNumber(string sellerId) =>
        $"OLIST-{sellerId.ToUpperInvariant()}";

    public static string CreateImportedProductName(string productId, string? categoryNameEn, string? categoryNamePt)
    {
        var label = categoryNameEn ?? categoryNamePt;
        return label is null
            ? $"Imported product {productId[..Math.Min(8, productId.Length)]}"
            : $"{label} ({productId[..Math.Min(8, productId.Length)]})";
    }

    public static string CreateImportedProductDescription(string productId, string? categoryNameEn, string? categoryNamePt)
    {
        var label = categoryNameEn ?? categoryNamePt ?? "unknown category";
        return $"Imported from the Olist dataset. Source product id: {productId}. Category: {label}.";
    }

    public static string BuildListingSku(string sellerId, string productId) =>
        $"OLIST-{sellerId}-{productId}".ToUpperInvariant();
}
