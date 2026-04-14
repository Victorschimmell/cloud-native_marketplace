using Backend.Domain.Entities.IdentityAccess;

namespace Backend.Application.Common.Abstractions;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(UserAccount userAccount, string password);
}
