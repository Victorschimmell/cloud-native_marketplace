using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
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

    public Task<Result<IReadOnlyList<ReviewDto>>> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IReadOnlyList<ReviewDto>>> GetByProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<ReviewDto>> CreateAsync(CreateReviewRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

