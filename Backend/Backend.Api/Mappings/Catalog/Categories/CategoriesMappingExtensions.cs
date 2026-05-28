using Backend.Api.Contracts.Catalog.Categories;
using App = Backend.Application.DTOs;

namespace Backend.Api.Mappings.Catalog.Categories;

public static class CategoriesMappingExtensions
{
    public static CategoryResponse ToResponse(this App.CategoryDto dto)
    {
        return new CategoryResponse
        {
            Id = dto.Id,
            CategoryNamePt = dto.CategoryNamePt,
            CategoryNameEn = dto.CategoryNameEn
        };
    }

}
