using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Entities.Orders;

namespace Backend.Application.Services;

public sealed class ReviewService : IReviewService
{
    private readonly IOrderReviewRepository _reviewRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ReviewService(IOrderReviewRepository reviewRepository, IOrderRepository orderRepository, IDateTimeProvider dateTimeProvider )
    {
        ArgumentNullException.ThrowIfNull(reviewRepository);
        ArgumentNullException.ThrowIfNull(orderRepository);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);

        _reviewRepository = reviewRepository;
        _orderRepository = orderRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<IReadOnlyList<ReviewDto>>> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        if (orderId == Guid.Empty)
        {
            return Result<IReadOnlyList<ReviewDto>>.ValidationFailure("Order id is required.");
        }

        var reviews = await _reviewRepository.GetByOrderIdAsync(orderId, cancellationToken);
        return Result<IReadOnlyList<ReviewDto>>.Success(reviews.Select(static review => review.ToDto()).ToArray());
    }

    public async Task<Result<IReadOnlyList<ReviewDto>>> GetByProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        if (productId == Guid.Empty)
        {
            return Result<IReadOnlyList<ReviewDto>>.ValidationFailure("Product id is required.");
        }

        var reviews = await _reviewRepository.GetByProductIdAsync(productId, cancellationToken);
        return Result<IReadOnlyList<ReviewDto>>.Success(reviews.Select(static review => review.ToDto()).ToArray());
    }

    public async Task<Result<ReviewDto>> CreateAsync(CreateReviewRequest request, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (request.OrderId == Guid.Empty || request.ReviewScore < 1 || request.ReviewScore > 5)
            {
                return Result<ReviewDto>.ValidationFailure("Order id is required and review score must be between 1 and 5.");
            }

            if (await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken) is null)
            {
                return Result<ReviewDto>.NotFound("Order was not found.");
            }

            var review = new OrderReview
            {
                OrderId = request.OrderId,
                ReviewScore = request.ReviewScore,
                ReviewCommentTitle = request.ReviewCommentTitle,
                ReviewCommentMessage = request.ReviewCommentMessage,
                ReviewCreationDateUtc = _dateTimeProvider.UtcNow
            };

            await _reviewRepository.AddAsync(review, cancellationToken);
            return Result<ReviewDto>.Success(review.ToDto());
        }, "Unable to create review.");
    }
}

