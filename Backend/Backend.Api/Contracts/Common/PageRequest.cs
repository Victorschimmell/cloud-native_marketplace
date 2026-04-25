using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Contracts.Common;

public sealed record PageRequest
{
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, int.MaxValue)]
    public int PageSize { get; init; } = 20;
}
