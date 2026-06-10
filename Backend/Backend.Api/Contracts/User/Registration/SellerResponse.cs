using Backend.Domain.Enums;

namespace Backend.Api.Contracts.User.Registration;

public sealed record SellerResponse
{
    public required Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required string BusinessName { get; init; }
    public required string RegistrationNumber { get; init; }
    public required string PayoutInformation { get; init; }
    public Guid? DefaultAddressId { get; init; }
    public required VerificationStatus VerificationStatus { get; init; }
    public DateTimeOffset? VerifiedAtUtc { get; init; }
    public string? OlistSellerId { get; init; }
}
