using Backend.Api.Attributes;
using Backend.Api.Contracts.Catalog.Categories;
using Backend.Api.Mappings.Catalog.Categories;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers.Catalog;

[Route("api/categories")]
public class CategoriesController : ApiControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet("{categoryId:guid}")]
    public async Task<ActionResult<CategoryResponse>> GetByIdAsync([NotEmptyGuid] Guid categoryId, CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetByIdAsync(categoryId, cancellationToken);
        return HandleResult(result, category => category.ToResponse());
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetAllAsync(cancellationToken);
        return HandleResult(result, categories => categories.Select(category => category.ToResponse()).ToArray());
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> CreateAsync([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _categoryService.CreateAsync(request.ToDto(), cancellationToken);
        return HandleResult(result, category => category.ToResponse());
    }

    [HttpPut("{categoryId:guid}")]
    public async Task<ActionResult<CategoryResponse>> UpdateAsync([NotEmptyGuid] Guid categoryId, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _categoryService.UpdateAsync(request.ToDto(categoryId), cancellationToken);
        return HandleResult(result, category => category.ToResponse());
    }

    [HttpDelete("{categoryId:guid}")]
    public async Task<IActionResult> DeleteAsync([NotEmptyGuid] Guid categoryId, CancellationToken cancellationToken)
    {
        var result = await _categoryService.DeleteAsync(categoryId, cancellationToken);
        return HandleResult(result);
    }
}