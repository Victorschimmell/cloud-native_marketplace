namespace Backend.Application.Common.Exceptions;

/// <summary>
/// Base class for application-specific exceptions in the marketplace platform.
/// Use exceptions only for exceptional or cross-cutting failures; prefer Result&lt;T&gt; for expected validation flows.
/// </summary>
public abstract class MarketplaceException : Exception
{
    protected MarketplaceException(string message)
        : base(message)
    {
    }

    protected MarketplaceException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
