namespace Backend.Application.Common.Exceptions;

/// <summary>
/// Thrown when a requested operation conflicts with the current state of the resource.
/// Maps to HTTP 409 Conflict.
/// </summary>
public sealed class ConflictException : MarketplaceException
{
    public ConflictException(string message)
        : base(message)
    {
    }
}
