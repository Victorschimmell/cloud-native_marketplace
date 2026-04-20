using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;

namespace Backend.Application.Services;

public sealed class CategoryService : ICategoryService
{
    private readonly IProductCategoryRepository _categoryRepository;

    public CategoryService(IProductCategoryRepository categoryRepository )
    {
        ArgumentNullException.ThrowIfNull(categoryRepository);
        _categoryRepository = categoryRepository;
    }

    public Task<Result<CategoryDto>> GetByIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IReadOnlyList<CategoryDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<CategoryDto>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<CategoryDto>> UpdateAsync(UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeleteAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

