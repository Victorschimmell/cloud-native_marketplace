namespace Backend.Application.Common.Exceptions;

/// <summary>
/// Thrown when the current user is authenticated but lacks permission to perform the requested operation.
/// Maps to HTTP 403 Forbidden.
/// </summary>
public sealed class ForbiddenException : MarketplaceException
{
    public ForbiddenException(string message)
        : base(message)
    {
    }
}
