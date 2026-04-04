namespace Backend.Infrastructure.Persistence.Import.Models;

public sealed record CustomerRow(
    string CustomerId,
    string CustomerUniqueId,
    string ZipCodePrefix,
    string City,
    string State);
