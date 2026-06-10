namespace Backend.Application.Common.Models;

public static class PaginationRules
{
    public const int MaxPageSize = 100;

    public static string? Validate(int page, int pageSize)
    {
        if (page <= 0 || pageSize <= 0)
        {
            return "Page and PageSize must be greater than 0.";
        }

        if (pageSize > MaxPageSize)
        {
            return $"PageSize must be {MaxPageSize} or less.";
        }

        return null;
    }
}
