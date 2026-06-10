using Backend.Application.Common.Results;
using Backend.Application.DTOs;

namespace Backend.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<Result<CategoryDto>> GetByIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CategoryDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<CategoryDto>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<Result<CategoryDto>> UpdateAsync(UpdateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid categoryId, CancellationToken cancellationToken = default);
}
