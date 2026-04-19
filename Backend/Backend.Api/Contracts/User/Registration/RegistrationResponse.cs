using Backend.Api.Contracts.User.Auth;

namespace Backend.Api.Contracts.User.Registration;

public sealed record RegistrationResponse
{
    public required UserAccountModel User { get; init; }
    public CustomerModel? Customer { get; init; }
    public SellerModel? Seller { get; init; }
}
