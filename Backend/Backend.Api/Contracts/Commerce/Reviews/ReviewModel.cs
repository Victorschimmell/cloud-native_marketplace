namespace Backend.Api.Contracts.Commerce.Reviews;

public sealed record ReviewModel
{
    public required Guid Id { get; init; }
    public required Guid OrderId { get; init; }
    public int? OrderItemId { get; init; }
    public Guid? ProductId { get; init; }
    public string? ReviewerDisplayName { get; init; }
    public required int ReviewScore { get; init; }
    public string? ReviewCommentTitle { get; init; }
    public string? ReviewCommentMessage { get; init; }
    public required DateTimeOffset ReviewCreationDateUtc { get; init; }
    public DateTimeOffset? ReviewAnswerTimestampUtc { get; init; }
}
