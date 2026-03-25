using Backend.Domain.Entities.Orders;

namespace Backend.Application.Abstractions.Repositories;

public interface ICurrencyRepository
{
    Task<Currency?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Currency?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Currency>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Currency currency, CancellationToken cancellationToken = default);
    Task UpdateAsync(Currency currency, CancellationToken cancellationToken = default);
    Task DeleteAsync(Currency currency, CancellationToken cancellationToken = default);
}
