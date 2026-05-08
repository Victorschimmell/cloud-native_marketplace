using Backend.Application.Common.Exceptions;

namespace Backend.UnitTests.Application.Exceptions;

public sealed class MarketplaceExceptionTests
{
    // ── NotFoundException ────────────────────────────────────────────────────

    [Fact]
    public void NotFoundException_WithMessage_SetsMessageAndIsMarketplaceException()
    {
        var ex = new NotFoundException("Product not found.");

        Assert.Equal("Product not found.", ex.Message);
        Assert.IsAssignableFrom<MarketplaceException>(ex);
    }

    [Fact]
    public void NotFoundException_WithResourceNameAndKey_FormatsMessageAndExposesProperties()
    {
        var key = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        var ex = new NotFoundException("Product", key);

        Assert.Equal($"Product with key '{key}' was not found.", ex.Message);
        Assert.Equal("Product", ex.ResourceName);
        Assert.Equal(key, ex.Key);
    }

    // ── ForbiddenException ───────────────────────────────────────────────────

    [Fact]
    public void ForbiddenException_WithMessage_SetsMessageAndIsMarketplaceException()
    {
        var ex = new ForbiddenException("Access denied.");

        Assert.Equal("Access denied.", ex.Message);
        Assert.IsAssignableFrom<MarketplaceException>(ex);
    }

    // ── ConflictException ────────────────────────────────────────────────────

    [Fact]
    public void ConflictException_WithMessage_SetsMessageAndIsMarketplaceException()
    {
        var ex = new ConflictException("Resource already exists.");

        Assert.Equal("Resource already exists.", ex.Message);
        Assert.IsAssignableFrom<MarketplaceException>(ex);
    }

}
