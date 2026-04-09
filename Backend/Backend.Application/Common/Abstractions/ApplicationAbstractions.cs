using Backend.Application.DTOs;
using Backend.Domain.Entities.IdentityAccess;

namespace Backend.Application.Common.Abstractions;

public interface ICurrentUserProvider
{
    Guid? UserId { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
}

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}

public interface IIdGenerator
{
    Guid NewGuid();
}

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(UserAccount userAccount, string password);
}

public interface IAuthTokenGenerator
{
    AuthTokenDto CreateToken(UserAccount userAccount);
}
