using Backend.Application.DTOs;
using Backend.Domain.Entities.IdentityAccess;

namespace Backend.Application.Common.Abstractions;

public interface IAuthTokenGenerator
{
    AuthTokenDto CreateToken(UserAccount userAccount);
}
