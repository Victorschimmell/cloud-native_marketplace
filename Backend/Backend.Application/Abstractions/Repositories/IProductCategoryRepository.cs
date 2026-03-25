using Backend.Domain.Entities.Catalog;

namespace Backend.Application.Abstractions.Repositories;

public interface IProductCategoryRepository
{
    Task<ProductCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductCategory>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(ProductCategory category, CancellationToken cancellationToken = default);
    Task UpdateAsync(ProductCategory category, CancellationToken cancellationToken = default);
    Task DeleteAsync(ProductCategory category, CancellationToken cancellationToken = default);
}
