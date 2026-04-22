using Backend.Api.Attributes;
using Backend.Api.Contracts.User.SellerVerification;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers.User;

[Route("api/sellers/{sellerId:guid}/verifications")]
public class SellerVerificationController : ApiControllerBase
{
    private readonly ISellerVerificationService _sellerVerificationService;

    public SellerVerificationController(ISellerVerificationService sellerVerificationService)
    {
        _sellerVerificationService = sellerVerificationService;
    }

    [HttpPost]
    public async Task<ActionResult<SellerVerificationSubmissionResponse>> SubmitVerificationAsync([FromRoute][NotEmptyGuid] Guid sellerId, [FromBody] SellerVerificationRequest request, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SellerVerificationRequestDetailsResponse>>> GetVerificationStatus([FromRoute][NotEmptyGuid] Guid sellerId, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }
}
