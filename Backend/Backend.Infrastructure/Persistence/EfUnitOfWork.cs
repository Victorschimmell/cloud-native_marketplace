using Backend.Application.Common.Abstractions;

namespace Backend.Infrastructure.Persistence;

internal sealed class EfUnitOfWork(ApplicationDbContext dbContext) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
