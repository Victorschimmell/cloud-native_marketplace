using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Entities.Orders;
using Backend.Domain.Enums;
namespace Backend.Application.Services;

public sealed class ReviewService : IReviewService
{
    private readonly IOrderReviewRepository _reviewRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public ReviewService(
        IOrderReviewRepository reviewRepository,
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(reviewRepository);
        ArgumentNullException.ThrowIfNull(orderRepository);
        ArgumentNullException.ThrowIfNull(customerRepository);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        ArgumentNullException.ThrowIfNull(unitOfWork);

        _reviewRepository = reviewRepository;
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<ReviewDto>>> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var reviews = await _reviewRepository.GetByOrderIdAsync(orderId, cancellationToken);
        return Result<IReadOnlyList<ReviewDto>>.Success(reviews.Select(review => review.ToReviewDto()).ToArray());
    }

    public async Task<Result<IReadOnlyList<ReviewDto>>> GetByProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var reviews = await _reviewRepository.GetByProductIdAsync(productId, cancellationToken);
        return Result<IReadOnlyList<ReviewDto>>.Success(reviews.Select(review => review.ToReviewDto()).ToArray());
    }

    public async Task<Result<ReviewDto>> CreateAsync(CreateReviewRequest request, Guid authenticatedUserId, CancellationToken cancellationToken = default)
    {
        if (request.ReviewScore is < 1 or > 5)
        {
            return Result<ReviewDto>.ValidationFailure("Review score must be between 1 and 5.");
        }

        if (request.OrderItemId <= 0)
        {
            return Result<ReviewDto>.ValidationFailure("Order item id must be greater than zero.");
        }

        if (!TryNormalizeComment(request.ReviewCommentTitle, 200, out var title, out var titleError))
        {
            return Result<ReviewDto>.ValidationFailure(titleError);
        }

        if (!TryNormalizeComment(request.ReviewCommentMessage, 2000, out var message, out var messageError))
        {
            return Result<ReviewDto>.ValidationFailure(messageError);
        }

        var customer = await _customerRepository.GetByUserIdAsync(authenticatedUserId, cancellationToken);
        if (customer is null)
        {
            return Result<ReviewDto>.Forbidden("Only customer accounts can review purchased products.");
        }

        var order = await _orderRepository.GetByIdWithDetailsAsync(request.OrderId, cancellationToken);
        if (order is null || order.CustomerId != customer.Id)
        {
            return Result<ReviewDto>.NotFound("Order was not found for the authenticated customer.");
        }

        if (!IsReviewable(order))
        {
            return Result<ReviewDto>.ValidationFailure("Only delivered or returned orders can be reviewed.");
        }

        var orderItem = order.Items.FirstOrDefault(item => item.OrderItemId == request.OrderItemId);
        if (orderItem is null)
        {
            return Result<ReviewDto>.NotFound("Order item was not found for the authenticated customer.");
        }

        if (order.Reviews.Any(review => review.OrderItemId == request.OrderItemId) ||
            await _reviewRepository.ExistsForOrderItemAsync(order.Id, request.OrderItemId, cancellationToken))
        {
            return Result<ReviewDto>.Conflict("This order item has already been reviewed.");
        }

        if (order.Reviews.Any(review => review.OrderItem?.ProductId == orderItem.ProductId) ||
            await _reviewRepository.ExistsForCustomerProductAsync(customer.Id, orderItem.ProductId, cancellationToken))
        {
            return Result<ReviewDto>.Conflict("This product has already been reviewed by this customer.");
        }

        var review = new OrderReview
        {
            OrderId = order.Id,
            OrderItemId = request.OrderItemId,
            ReviewScore = request.ReviewScore,
            ReviewCommentTitle = title,
            ReviewCommentMessage = message,
            ReviewCreationDateUtc = _dateTimeProvider.UtcNow
        };

        await _reviewRepository.AddAsync(review, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        review.Order = order;
        review.OrderItem = orderItem;
        return Result<ReviewDto>.Success(review.ToReviewDto());
    }

    private static bool IsReviewable(Order order) =>
        order.OrderStatus is OrderStatus.Delivered or OrderStatus.Returned;

    private static bool TryNormalizeComment(string? value, int maxLength, out string? normalized, out string error)
    {
        normalized = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        error = string.Empty;

        if (normalized is not null && normalized.Length > maxLength)
        {
            error = $"Review text must be {maxLength} characters or fewer.";
            return false;
        }

        return true;
    }
}

