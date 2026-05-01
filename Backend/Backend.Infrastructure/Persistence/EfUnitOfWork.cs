using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Backend.Infrastructure.Persistence;

internal sealed class EfUnitOfWork(ApplicationDbContext dbContext) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (TryGetUniqueConstraintViolation(exception, out var target, out var constraintName))
        {
            throw new UniqueConstraintViolationException(target, constraintName, exception);
        }
    }

    private static bool TryGetUniqueConstraintViolation(
        DbUpdateException exception,
        out UniqueConstraintTarget target,
        out string? constraintName)
    {
        target = UniqueConstraintTarget.Unknown;
        constraintName = null;

        if (exception.InnerException is not PostgresException postgresException ||
            postgresException.SqlState != PostgresErrorCodes.UniqueViolation)
        {
            return false;
        }

        constraintName = postgresException.ConstraintName;
        target = GetConstraintTarget(constraintName);

        return true;
    }

    private static UniqueConstraintTarget GetConstraintTarget(string? constraintName)
    {
        if (constraintName?.Contains("user_account", StringComparison.OrdinalIgnoreCase) == true &&
            constraintName.Contains("email", StringComparison.OrdinalIgnoreCase))
        {
            return UniqueConstraintTarget.UserAccountEmail;
        }

        return UniqueConstraintTarget.Unknown;
    }
}
