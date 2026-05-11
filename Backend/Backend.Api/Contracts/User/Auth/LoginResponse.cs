using Backend.Api.Contracts.User.Registration;

namespace Backend.Api.Contracts.User.Auth;

public sealed record LoginResponse
{
    public required UserAccountModel User { get; init; }
    public required AuthToken Token { get; init; }
    public CustomerModel? Customer { get; init; }
    public SellerModel? Seller { get; init; }
}
