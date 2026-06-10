using Backend.Domain.Enums;

namespace Backend.Api.Contracts.User.Auth;

public sealed record LoginResponse
{
    public required UserAccountModel User { get; init; }
    public required AuthToken Token { get; init; }
    public required AuthProfileModel Profile { get; init; }
}

public sealed record AuthProfileModel
{
    public Guid? CustomerId { get; init; }
    public Guid? SellerId { get; init; }
    public VerificationStatus? SellerVerificationStatus { get; init; }
}
