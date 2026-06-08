using Backend.Application.Abstractions.Repositories;
using Backend.Domain.Entities.Operations;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.Repositories;

internal sealed class OrderNumberRepository(ApplicationDbContext dbContext) : IOrderNumberGenerator
{
    public async Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken = default)
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var seq = await dbContext.NumberSequences
            .FromSqlInterpolated($"SELECT * FROM number_sequence WHERE \"SequenceKey\" = {"orders"} FOR UPDATE")
            .AsTracking()
            .SingleOrDefaultAsync(cancellationToken);

        if (seq is null)
        {
            seq = new NumberSequence { SequenceKey = "orders", LastValue = 0 };
            dbContext.NumberSequences.Add(seq);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        seq.LastValue += 1;
        await dbContext.SaveChangesAsync(cancellationToken);

        await tx.CommitAsync(cancellationToken);

        // Format: ORD-yyyyMMdd-00000001
        var formatted = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{seq.LastValue:D8}";
        return formatted;
    }
}
