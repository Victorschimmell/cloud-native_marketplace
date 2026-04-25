using Backend.Api.Contracts.User.Registration;

namespace Backend.Api.Contracts.User.SellerVerification;

public sealed record SellerVerificationSubmissionResponse
{
    public required SellerVerificationRequestDetailsModel Request { get; init; }
    public required SellerModel Seller { get; init; }
}
