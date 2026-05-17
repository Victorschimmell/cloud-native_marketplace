using System.ComponentModel.DataAnnotations;
using Backend.Api.Attributes;

namespace Backend.Api.Contracts.Commerce.Reviews;

public sealed record RecordReviewRequest
{
    [NotEmptyGuid]
    public required Guid OrderId { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "OrderItemId must be greater than zero.")]
    public required int OrderItemId { get; init; }

    [Range(1, 5, ErrorMessage = "ReviewScore must be between 1 and 5.")]
    public required int ReviewScore { get; init; }

    [MaxLength(200)]
    public string? ReviewCommentTitle { get; init; }

    [MaxLength(2000)]
    public string? ReviewCommentMessage { get; init; }
}
