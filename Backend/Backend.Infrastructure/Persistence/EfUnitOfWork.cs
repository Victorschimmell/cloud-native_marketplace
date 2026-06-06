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

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                await operation(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
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

        if (constraintName?.Contains("order_review", StringComparison.OrdinalIgnoreCase) == true &&
            constraintName.Contains("OrderId", StringComparison.OrdinalIgnoreCase) &&
            constraintName.Contains("OrderItemId", StringComparison.OrdinalIgnoreCase))
        {
            return UniqueConstraintTarget.OrderReviewOrderItem;
        }

        if (constraintName?.Contains("order_review", StringComparison.OrdinalIgnoreCase) == true &&
            constraintName.Contains("CustomerId", StringComparison.OrdinalIgnoreCase) &&
            constraintName.Contains("ProductId", StringComparison.OrdinalIgnoreCase))
        {
            return UniqueConstraintTarget.OrderReviewCustomerProduct;
        }

        return UniqueConstraintTarget.Unknown;
    }
}
