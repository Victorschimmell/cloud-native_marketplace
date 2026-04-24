using Backend.Application.Abstractions.Repositories;
using Backend.Domain.Entities.Catalog;
using Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.Repositories;

internal sealed class ProductCategoryRepository(ApplicationDbContext dbContext) : IProductCategoryRepository
{
    public async Task<ProductCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.ProductCategories
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ProductCategory>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.ProductCategories
            .OrderBy(c => c.CategoryNamePt)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(ProductCategory category, CancellationToken cancellationToken = default)
    {
        dbContext.ProductCategories.Add(category);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(ProductCategory category, CancellationToken cancellationToken = default)
    {
        dbContext.ProductCategories.Update(category);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ProductCategory category, CancellationToken cancellationToken = default)
    {
        dbContext.ProductCategories.Remove(category);
        return Task.CompletedTask;
    }
}
