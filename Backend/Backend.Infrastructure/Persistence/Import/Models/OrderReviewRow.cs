namespace Backend.Infrastructure.Persistence.Import.Models;

public sealed record OrderReviewRow(
    string ReviewId,
    string OrderId,
    int ReviewScore,
    string? ReviewCommentTitle,
    string? ReviewCommentMessage,
    DateTimeOffset ReviewCreationDateUtc,
    DateTimeOffset? ReviewAnswerTimestampUtc);
