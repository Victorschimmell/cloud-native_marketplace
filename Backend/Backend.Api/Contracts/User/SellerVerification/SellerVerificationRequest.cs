namespace Backend.Api.Contracts.User.SellerVerification;

public sealed record SellerVerificationRequest
{
    public required string SubmittedDetails { get; init; }
}
