using System.ComponentModel.DataAnnotations;
using Backend.Application.Common.Models;

namespace Backend.Api.Contracts.Common;

public sealed record PageRequest
{
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, PaginationRules.MaxPageSize)]
    public int PageSize { get; init; } = 20;
}
