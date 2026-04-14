using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Entities.Catalog;

namespace Backend.Application.Services;

public sealed class CategoryService : ICategoryService
{
    private readonly IProductCategoryRepository _categoryRepository;

    public CategoryService(IProductCategoryRepository categoryRepository )
    {
        ArgumentNullException.ThrowIfNull(categoryRepository);
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<CategoryDto>> GetByIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        if (categoryId == Guid.Empty)
        {
            return Result<CategoryDto>.ValidationFailure("Category id is required.");
        }

        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
        return category is null
            ? Result<CategoryDto>.NotFound("Category was not found.")
            : Result<CategoryDto>.Success(category.ToDto());
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<CategoryDto>>.Success(categories.Select(static category => category.ToDto()).ToArray());
    }

    public async Task<Result<CategoryDto>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(request.CategoryNamePt))
            {
                return Result<CategoryDto>.ValidationFailure("Category name is required.");
            }

            var category = new ProductCategory
            {
                CategoryNamePt = request.CategoryNamePt.Trim(),
                CategoryNameEn = string.IsNullOrWhiteSpace(request.CategoryNameEn) ? null : request.CategoryNameEn.Trim()
            };

            await _categoryRepository.AddAsync(category, cancellationToken);
            return Result<CategoryDto>.Success(category.ToDto());
        }, "Unable to create category.");
    }

    public async Task<Result<CategoryDto>> UpdateAsync(UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (request.CategoryId == Guid.Empty || string.IsNullOrWhiteSpace(request.CategoryNamePt))
            {
                return Result<CategoryDto>.ValidationFailure("Category id and category name are required.");
            }

            var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category is null)
            {
                return Result<CategoryDto>.NotFound("Category was not found.");
            }

            category.CategoryNamePt = request.CategoryNamePt.Trim();
            category.CategoryNameEn = string.IsNullOrWhiteSpace(request.CategoryNameEn) ? null : request.CategoryNameEn.Trim();

            await _categoryRepository.UpdateAsync(category, cancellationToken);
            return Result<CategoryDto>.Success(category.ToDto());
        }, "Unable to update category.");
    }

    public async Task<Result> DeleteAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (categoryId == Guid.Empty)
            {
                return Result.ValidationFailure("Category id is required.");
            }

            var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
            if (category is null)
            {
                return Result.NotFound("Category was not found.");
            }

            await _categoryRepository.DeleteAsync(category, cancellationToken);
            return Result.Success();
        }, "Unable to delete category.");
    }
}

