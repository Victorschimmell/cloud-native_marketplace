using Backend.Api.Attributes;
using Backend.Api.Contracts.User.SellerVerification;
using Backend.Api.Mappings.User.SellerVerification;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers.User;

[Route("api/sellers/{sellerId:guid}/verifications")]
[Authorize]
public class SellerVerificationController : ApiControllerBase
{
    private readonly ISellerVerificationService _sellerVerificationService;

    public SellerVerificationController(ISellerVerificationService sellerVerificationService)
    {
        _sellerVerificationService = sellerVerificationService;
    }

    [HttpPost]
    public async Task<ActionResult<SellerVerificationSubmissionResponse>> SubmitVerificationAsync(
        [FromRoute][NotEmptyGuid] Guid sellerId,
        [FromBody] SellerVerificationRequest request,
        CancellationToken cancellationToken)
    {
        var applicationRequest = request.ToApplicationRequest(sellerId);
        var result = await _sellerVerificationService.SubmitVerificationAsync(applicationRequest, cancellationToken);
        return HandleResult(result, response => response.ToSubmissionResponse());
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SellerVerificationRequestDetailsResponse>>> GetVerificationStatus(
        [FromRoute][NotEmptyGuid] Guid sellerId,
        CancellationToken cancellationToken)
    {
        var result = await _sellerVerificationService.GetRequestsBySellerAsync(sellerId, cancellationToken);
        return HandleResult(result, items => items.Select(item => item.ToDetailsResponse()).ToArray());
    }
}
