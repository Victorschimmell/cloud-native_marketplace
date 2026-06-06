using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Contracts.User.SellerVerification;

public sealed record SellerVerificationRequest
{
    [Required]
    [StringLength(4000, MinimumLength = 1)]
    public required string SubmittedDetails { get; init; }
}
