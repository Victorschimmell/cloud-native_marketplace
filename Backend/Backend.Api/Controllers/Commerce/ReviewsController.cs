using Backend.Api.Contracts.Commerce.Reviews;
using Backend.Api.Mappings.Commerce.Reviews;
using Backend.Application.Common.Abstractions;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers.Commerce;

[Route("api/reviews")]
public class ReviewsController : ApiControllerBase
{
    private readonly IReviewService _reviewService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public ReviewsController(IReviewService reviewService, ICurrentUserProvider currentUserProvider)
    {
        _reviewService = reviewService;
        _currentUserProvider = currentUserProvider;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ReviewResponse>> RecordReviewAsync([FromBody] RecordReviewRequest request, CancellationToken cancellationToken)
    {
        if (_currentUserProvider.UserId is not { } userId)
        {
            return Unauthorized(new { Error = "Authenticated user id is missing." });
        }

        var result = await _reviewService.CreateAsync(request.ToApplicationRequest(), userId, cancellationToken);
        return HandleResult(result, review => review.ToResponse());
    }
}
