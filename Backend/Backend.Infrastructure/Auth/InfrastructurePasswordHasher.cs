using Backend.Application.Common.Abstractions;
using Backend.Domain.Entities.IdentityAccess;

namespace Backend.Infrastructure.Auth;

internal sealed class InfrastructurePasswordHasher : IPasswordHasher
{
    public string HashPassword(string password) => throw new NotImplementedException();

    public bool VerifyPassword(UserAccount userAccount, string password) => throw new NotImplementedException();
}
