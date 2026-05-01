using Backend.Api.Contracts.Commerce.Reviews;
using App = Backend.Application.DTOs;

namespace Backend.Api.Mappings.Commerce.Reviews;

public static class ReviewsMappingExtensions
{
    public static ReviewModel ToModel(this App.ReviewDto review) =>
        new()
        {
            Id = review.Id,
            OrderId = review.OrderId,
            ReviewScore = review.ReviewScore,
            ReviewCommentTitle = review.ReviewCommentTitle,
            ReviewCommentMessage = review.ReviewCommentMessage,
            ReviewCreationDateUtc = review.ReviewCreationDateUtc,
            ReviewAnswerTimestampUtc = review.ReviewAnswerTimestampUtc,
        };

    public static ReviewResponse ToResponse(this App.ReviewDto review) =>
        new()
        {
            Id = review.Id,
            OrderId = review.OrderId,
            ReviewScore = review.ReviewScore,
            ReviewCommentTitle = review.ReviewCommentTitle,
            ReviewCommentMessage = review.ReviewCommentMessage,
            ReviewCreationDateUtc = review.ReviewCreationDateUtc,
            ReviewAnswerTimestampUtc = review.ReviewAnswerTimestampUtc,
        };
}
