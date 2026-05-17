using Backend.Api.Contracts.Catalog.Categories;
using App = Backend.Application.DTOs;

namespace Backend.Api.Mappings.Catalog.Categories;

public static class CategoriesMappingExtensions
{
    public static App.CreateCategoryRequest ToDto(this CreateCategoryRequest request)
    {
        return new App.CreateCategoryRequest(
            CategoryNamePt: request.CategoryNamePt,
            CategoryNameEn: request.CategoryNameEn);
    }

    public static CategoryResponse ToResponse(this App.CategoryDto dto)
    {
        return new CategoryResponse
        {
            Id = dto.Id,
            CategoryNamePt = dto.CategoryNamePt,
            CategoryNameEn = dto.CategoryNameEn
        };
    }

    public static App.UpdateCategoryRequest ToDto(this UpdateCategoryRequest request, Guid categoryId)
    {
        return new App.UpdateCategoryRequest(
            CategoryId: categoryId,
            CategoryNamePt: request.CategoryNamePt,
            CategoryNameEn: request.CategoryNameEn);
    }
}