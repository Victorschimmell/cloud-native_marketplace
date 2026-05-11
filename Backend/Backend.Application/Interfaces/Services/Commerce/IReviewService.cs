using Backend.Application.Common.Results;
using Backend.Application.DTOs;

namespace Backend.Application.Interfaces.Services;

public interface IReviewService
{
    Task<Result<IReadOnlyList<ReviewDto>>> GetByOrderAsync(Guid orderId, Guid authenticatedUserId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ReviewDto>>> GetByProductAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<Result<ReviewDto>> CreateAsync(CreateReviewRequest request, Guid authenticatedUserId, CancellationToken cancellationToken = default);
}
