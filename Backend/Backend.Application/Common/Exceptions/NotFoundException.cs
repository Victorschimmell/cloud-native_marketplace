namespace Backend.Application.Common.Exceptions;

/// <summary>
/// Thrown when a requested resource cannot be found.
/// Maps to HTTP 404 Not Found.
/// </summary>
public sealed class NotFoundException : MarketplaceException
{
    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string resourceName, object key)
        : base($"{resourceName} with key '{key}' was not found.")
    {
        ResourceName = resourceName;
        Key = key;
    }

    public string? ResourceName { get; }
    public object? Key { get; }
}
