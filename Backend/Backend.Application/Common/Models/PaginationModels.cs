namespace Backend.Application.Common.Models;

public sealed record PagedRequest(int Page = 1, int PageSize = 20);

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
