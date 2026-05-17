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
            OrderItemId = review.OrderItemId,
            ProductId = review.ProductId,
            ReviewerDisplayName = review.ReviewerDisplayName,
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
            OrderItemId = review.OrderItemId,
            ProductId = review.ProductId,
            ReviewerDisplayName = review.ReviewerDisplayName,
            ReviewScore = review.ReviewScore,
            ReviewCommentTitle = review.ReviewCommentTitle,
            ReviewCommentMessage = review.ReviewCommentMessage,
            ReviewCreationDateUtc = review.ReviewCreationDateUtc,
            ReviewAnswerTimestampUtc = review.ReviewAnswerTimestampUtc,
        };

    public static App.CreateReviewRequest ToApplicationRequest(this RecordReviewRequest request) =>
        new(
            request.OrderId,
            request.OrderItemId,
            request.ReviewScore,
            request.ReviewCommentTitle,
            request.ReviewCommentMessage);
}
