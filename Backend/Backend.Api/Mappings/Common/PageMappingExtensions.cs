using Backend.Api.Contracts.Common;
using App = Backend.Application.Common.Models;

namespace Backend.Api.Mappings.Common;

public static class PageMappingExtensions
{
    public static App.PagedRequest ToDto(this PageRequest request)
    {
        return new App.PagedRequest(Page: request.Page, PageSize: request.PageSize);
    }

    public static PageResponse<T> ToResponse<T>(this App.PagedResult<T> pagedResult)
    {
        return new PageResponse<T>
        {
            Items = pagedResult.Items,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize,
            TotalCount = pagedResult.TotalCount
        };
    }
}
