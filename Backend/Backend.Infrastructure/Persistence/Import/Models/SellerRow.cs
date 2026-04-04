namespace Backend.Infrastructure.Persistence.Import.Models;

public sealed record SellerRow(
    string SellerId,
    string ZipCodePrefix,
    string City,
    string State);
