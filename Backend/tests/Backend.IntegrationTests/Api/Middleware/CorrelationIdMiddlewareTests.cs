using Backend.Api.Middleware;
using Microsoft.AspNetCore.Http;

namespace Backend.IntegrationTests.Api.Middleware;

public sealed class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_UsesIncomingCorrelationId()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] = "customer-trace-123";
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal("customer-trace-123", context.TraceIdentifier);
        Assert.Equal("customer-trace-123", context.Items[CorrelationIdMiddleware.ItemKey]);
        Assert.Equal("customer-trace-123", context.Response.Headers[CorrelationIdMiddleware.HeaderName]);
    }

    [Fact]
    public async Task InvokeAsync_GeneratesCorrelationId_WhenHeaderIsMissing()
    {
        var context = new DefaultHttpContext();
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.False(string.IsNullOrWhiteSpace(context.TraceIdentifier));
        Assert.Equal(32, context.TraceIdentifier.Length);
        Assert.Equal(context.TraceIdentifier, context.Items[CorrelationIdMiddleware.ItemKey]);
        Assert.Equal(context.TraceIdentifier, context.Response.Headers[CorrelationIdMiddleware.HeaderName]);
    }

    [Fact]
    public async Task InvokeAsync_GeneratesCorrelationId_WhenHeaderIsTooLong()
    {
        var context = new DefaultHttpContext();
        var invalidCorrelationId = new string('a', 129);
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] = invalidCorrelationId;
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.NotEqual(invalidCorrelationId, context.TraceIdentifier);
        Assert.Equal(32, context.TraceIdentifier.Length);
    }

    [Fact]
    public async Task InvokeAsync_CorrelationIdIsAvailableToNextMiddleware()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] = "flow-456";
        var observedCorrelationId = string.Empty;
        var middleware = new CorrelationIdMiddleware(nextContext =>
        {
            observedCorrelationId = nextContext.TraceIdentifier;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        Assert.Equal("flow-456", observedCorrelationId);
    }
}
