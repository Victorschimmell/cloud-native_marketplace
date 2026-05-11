using Backend.Api.Contracts.User.Auth;
using Backend.Api.Mappings.User.Registration;
using App = Backend.Application.DTOs;

namespace Backend.Api.Mappings.User.Auth;

public static class AuthMappingExtensions
{
    public static App.LoginRequest ToDto(this LoginRequest request) =>
        new(request.Email, request.Password);

    public static LoginResponse ToResponse(this App.AuthenticationResponse response) =>
        new()
        {
            User = response.User.ToModel(),
            Token = response.Token.ToModel(),
            Customer = response.Customer?.ToModel(),
            Seller = response.Seller?.ToModel()
        };

    public static UserAccountModel ToModel(this App.UserAccountDto user) =>
        new()
        {
            Id = user.Id,
            Email = user.Email,
            IsAdmin = user.IsAdmin,
            IsBlocked = user.IsBlocked,
            AccountStatus = (AccountStatus)user.AccountStatus,
            LastLoginAtUtc = user.LastLoginAtUtc
        };

    public static AuthToken ToModel(this App.AuthTokenDto token) =>
        new()
        {
            AccessToken = token.AccessToken,
            ExpiresAtUtc = token.ExpiresAtUtc,
            RefreshToken = token.RefreshToken
        };
}
