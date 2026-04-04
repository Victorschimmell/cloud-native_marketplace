namespace Backend.Infrastructure.Persistence.Import.Models;

public sealed record ProductRow(
    string ProductId,
    string? CategoryNamePt,
    int? ProductNameLength,
    int? ProductDescriptionLength,
    int? ProductPhotosQty,
    int? ProductWeightG,
    int? ProductLengthCm,
    int? ProductHeightCm,
    int? ProductWidthCm);
