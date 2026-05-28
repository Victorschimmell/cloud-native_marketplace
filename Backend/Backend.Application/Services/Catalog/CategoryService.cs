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

    public async Task<Result<IReadOnlyList<CategoryDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<CategoryDto>>.Success(categories.Select(ApplicationMappings.ToCategoryDto).ToArray());
    }
}

