using Backend.Application.Common.Abstractions;
using Backend.Application.DTOs;
using Backend.Domain.Entities.IdentityAccess;

namespace Backend.Infrastructure.Auth;

internal sealed class InfrastructureAuthTokenGenerator : IAuthTokenGenerator
{
    public AuthTokenDto CreateToken(UserAccount userAccount) => throw new NotImplementedException();
}
