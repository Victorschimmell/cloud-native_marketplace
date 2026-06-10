using Backend.Application.Abstractions.Repositories;
using Backend.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.Repositories;

internal sealed class CurrencyRepository(ApplicationDbContext dbContext) : ICurrencyRepository
{
    public async Task<Currency?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Currencies
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Currency?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await dbContext.Currencies
            .FirstOrDefaultAsync(c => c.Code == code, cancellationToken);
    }

    public async Task<IReadOnlyList<Currency>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Currencies
            .OrderBy(c => c.Code)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(Currency currency, CancellationToken cancellationToken = default)
    {
        dbContext.Currencies.Add(currency);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Currency currency, CancellationToken cancellationToken = default)
    {
        dbContext.Currencies.Update(currency);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Currency currency, CancellationToken cancellationToken = default)
    {
        dbContext.Currencies.Remove(currency);
        return Task.CompletedTask;
    }
}
