using Backend.Domain.Enums;

namespace Backend.Api.Contracts.User.SellerVerification;

public sealed record SellerVerificationRequestDetailsResponse
{
    public required Guid Id { get; init; }
    public required Guid SellerId { get; init; }
    public required DateTimeOffset SubmittedAtUtc { get; init; }
    public required SellerVerificationRequestStatus Status { get; init; }
    public required string BusinessNameSnapshot { get; init; }
    public required string RegistrationNumberSnapshot { get; init; }
    public required string SubmittedDetails { get; init; }
    public string? ReviewNotes { get; init; }
    public Guid? ReviewedByUserId { get; init; }
    public DateTimeOffset? ReviewedAtUtc { get; init; }
    public string? RejectionReason { get; init; }
}
