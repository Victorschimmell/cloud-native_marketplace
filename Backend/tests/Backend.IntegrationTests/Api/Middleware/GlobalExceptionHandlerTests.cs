using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Backend.Api.Middleware;
using Backend.Application.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Backend.IntegrationTests.Api.Middleware;

/// <summary>
/// Unit-style tests for GlobalExceptionHandler.
/// Uses DefaultHttpContext directly — no database or full HTTP pipeline required.
/// </summary>
public sealed class GlobalExceptionHandlerTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static (GlobalExceptionHandler Handler, FakeLogger Logger) BuildHandler(
        string? environmentName = null)
    {
        var logger = new FakeLogger();
        var env = new FakeHostEnvironment { EnvironmentName = environmentName ?? Environments.Production };
        var handler = new GlobalExceptionHandler(logger, env);
        return (handler, logger);
    }

    private static DefaultHttpContext BuildHttpContext(
        string method = "GET",
        string path = "/api/test",
        string traceId = "test-trace-0001",
        Guid? userId = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        context.TraceIdentifier = traceId;
        context.Response.Body = new MemoryStream();

        if (userId.HasValue)
        {
            var identity = new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString())],
                authenticationType: "Test");
            context.User = new ClaimsPrincipal(identity);
        }

        return context;
    }

    private static async Task<ProblemDetails> ReadProblemDetailsAsync(HttpResponse response, CancellationToken cancellationToken = default)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        var result = await JsonSerializer.DeserializeAsync<ProblemDetails>(
            response.Body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
            cancellationToken);
        return result!;
    }

    // ── HTTP status code mapping ─────────────────────────────────────────────

    [Fact]
    public async Task TryHandleAsync_NotFoundException_Returns404()
    {
        var (handler, _) = BuildHandler();
        var context = BuildHttpContext();

        await handler.TryHandleAsync(context, new NotFoundException("Order not found."), TestContext.Current.CancellationToken);

        Assert.Equal((int)HttpStatusCode.NotFound, context.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_ForbiddenException_Returns403()
    {
        var (handler, _) = BuildHandler();
        var context = BuildHttpContext();

        await handler.TryHandleAsync(context, new ForbiddenException("Not allowed."), TestContext.Current.CancellationToken);

        Assert.Equal((int)HttpStatusCode.Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_ConflictException_Returns409()
    {
        var (handler, _) = BuildHandler();
        var context = BuildHttpContext();

        await handler.TryHandleAsync(context, new ConflictException("Already exists."), TestContext.Current.CancellationToken);

        Assert.Equal((int)HttpStatusCode.Conflict, context.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_UnexpectedException_Returns500()
    {
        var (handler, _) = BuildHandler();
        var context = BuildHttpContext();

        await handler.TryHandleAsync(context, new InvalidOperationException("boom"), TestContext.Current.CancellationToken);

        Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_ReturnsTrue_ForAllExceptionTypes()
    {
        var exceptions = new Exception[]
        {
            new NotFoundException("x"),
            new ForbiddenException("x"),
            new ConflictException("x"),
            new InvalidOperationException("x")
        };

        foreach (var ex in exceptions)
        {
            var (handler, _) = BuildHandler();
            var context = BuildHttpContext();

            var handled = await handler.TryHandleAsync(context, ex, TestContext.Current.CancellationToken);

            Assert.True(handled, $"Expected handler to return true for {ex.GetType().Name}");
        }
    }

    // ── Safe production responses ────────────────────────────────────────────

    [Fact]
    public async Task TryHandleAsync_UnexpectedException_InProduction_DoesNotExposeExceptionMessage()
    {
        var (handler, _) = BuildHandler(Environments.Production);
        var context = BuildHttpContext();
        const string internalDetail = "Connection string contains password=s3cr3t";

        await handler.TryHandleAsync(context, new Exception(internalDetail), TestContext.Current.CancellationToken);

        var body = await ReadProblemDetailsAsync(context.Response, TestContext.Current.CancellationToken);
        Assert.DoesNotContain(internalDetail, body.Detail ?? string.Empty);
        Assert.DoesNotContain(internalDetail, body.Title ?? string.Empty);
    }

    [Fact]
    public async Task TryHandleAsync_UnexpectedException_InDevelopment_IncludesExceptionMessage()
    {
        var (handler, _) = BuildHandler(Environments.Development);
        var context = BuildHttpContext();
        const string detail = "Detailed dev error.";

        await handler.TryHandleAsync(context, new Exception(detail), TestContext.Current.CancellationToken);

        var body = await ReadProblemDetailsAsync(context.Response, TestContext.Current.CancellationToken);
        Assert.Contains(detail, body.Detail ?? string.Empty);
    }

    [Fact]
    public async Task TryHandleAsync_Response_IncludesTraceId()
    {
        var (handler, _) = BuildHandler();
        var context = BuildHttpContext(traceId: "trace-abc-123");

        await handler.TryHandleAsync(context, new NotFoundException("Missing."), TestContext.Current.CancellationToken);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var doc = await JsonDocument.ParseAsync(context.Response.Body, cancellationToken: TestContext.Current.CancellationToken);
        // ProblemDetails.Extensions entries are written as additional root-level properties in RFC 7807 JSON
        var traceId = doc.RootElement.GetProperty("traceId").GetString();
        Assert.Equal("trace-abc-123", traceId);
    }

    [Fact]
    public async Task TryHandleAsync_Response_ContentTypeIsProblemJson()
    {
        var (handler, _) = BuildHandler();
        var context = BuildHttpContext();

        await handler.TryHandleAsync(context, new NotFoundException("x"), TestContext.Current.CancellationToken);

        Assert.Equal("application/problem+json", context.Response.ContentType);
    }

    // ── Known exceptions expose their own safe message ───────────────────────

    [Fact]
    public async Task TryHandleAsync_NotFoundException_ResponseContainsExceptionMessage()
    {
        var (handler, _) = BuildHandler(Environments.Production);
        var context = BuildHttpContext();

        await handler.TryHandleAsync(context, new NotFoundException("Order abc was not found."), TestContext.Current.CancellationToken);

        var body = await ReadProblemDetailsAsync(context.Response, TestContext.Current.CancellationToken);
        Assert.Equal("Order abc was not found.", body.Detail);
    }

    // ── Logging ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task TryHandleAsync_UnexpectedException_LoggedAtErrorLevel()
    {
        var (handler, logger) = BuildHandler();
        var context = BuildHttpContext();

        await handler.TryHandleAsync(context, new InvalidOperationException("something failed"), TestContext.Current.CancellationToken);

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Error, entry.Level);
    }

    [Fact]
    public async Task TryHandleAsync_UnexpectedException_LogIncludesFullException()
    {
        var (handler, logger) = BuildHandler();
        var context = BuildHttpContext();
        var exception = new InvalidOperationException("something failed");

        await handler.TryHandleAsync(context, exception, TestContext.Current.CancellationToken);

        var entry = Assert.Single(logger.Entries);
        Assert.Same(exception, entry.Exception);
    }

    [Fact]
    public async Task TryHandleAsync_NotFoundException_LogIncludesFullException()
    {
        var (handler, logger) = BuildHandler();
        var context = BuildHttpContext();
        var exception = new NotFoundException("not found");

        await handler.TryHandleAsync(context, exception, TestContext.Current.CancellationToken);

        var entry = Assert.Single(logger.Entries);
        Assert.Same(exception, entry.Exception);
    }

    [Fact]
    public async Task TryHandleAsync_ForbiddenException_LogIncludesFullException()
    {
        var (handler, logger) = BuildHandler();
        var context = BuildHttpContext();
        var exception = new ForbiddenException("denied");

        await handler.TryHandleAsync(context, exception, TestContext.Current.CancellationToken);

        var entry = Assert.Single(logger.Entries);
        Assert.Same(exception, entry.Exception);
    }

    [Fact]
    public async Task TryHandleAsync_ConflictException_LogIncludesFullException()
    {
        var (handler, logger) = BuildHandler();
        var context = BuildHttpContext();
        var exception = new ConflictException("conflict");

        await handler.TryHandleAsync(context, exception, TestContext.Current.CancellationToken);

        var entry = Assert.Single(logger.Entries);
        Assert.Same(exception, entry.Exception);
    }

    [Fact]
    public async Task TryHandleAsync_UnexpectedException_LogIncludesRequestContext()
    {
        var (handler, logger) = BuildHandler();
        var userId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var context = BuildHttpContext(method: "POST", path: "/api/orders", traceId: "trace-xyz", userId: userId);

        await handler.TryHandleAsync(context, new InvalidOperationException("boom"), TestContext.Current.CancellationToken);

        var entry = Assert.Single(logger.Entries);
        Assert.Contains("POST", entry.Message);
        Assert.Contains("/api/orders", entry.Message);
        Assert.Contains("trace-xyz", entry.Message);
        Assert.Contains("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", entry.Message);
    }

    [Fact]
    public async Task TryHandleAsync_NotFoundException_LoggedAtWarningLevel()
    {
        var (handler, logger) = BuildHandler();
        var context = BuildHttpContext();

        await handler.TryHandleAsync(context, new NotFoundException("missing"), TestContext.Current.CancellationToken);

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Warning, entry.Level);
    }

    [Fact]
    public async Task TryHandleAsync_ForbiddenException_LoggedAtWarningLevel()
    {
        var (handler, logger) = BuildHandler();
        var context = BuildHttpContext();

        await handler.TryHandleAsync(context, new ForbiddenException("denied"), TestContext.Current.CancellationToken);

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Warning, entry.Level);
    }

    [Fact]
    public async Task TryHandleAsync_ConflictException_LoggedAtWarningLevel()
    {
        var (handler, logger) = BuildHandler();
        var context = BuildHttpContext();

        await handler.TryHandleAsync(context, new ConflictException("conflict"), TestContext.Current.CancellationToken);

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Warning, entry.Level);
    }

    [Fact]
    public async Task TryHandleAsync_UnexpectedException_LogDoesNotContainFullExceptionInResponseBody_InProduction()
    {
        // Ensure the stack trace / internal message stays in the log, not the HTTP response
        var (handler, _) = BuildHandler(Environments.Production);
        var context = BuildHttpContext();
        var exception = new Exception("super secret internal message with stack");

        await handler.TryHandleAsync(context, exception, TestContext.Current.CancellationToken);

        var body = await ReadProblemDetailsAsync(context.Response, TestContext.Current.CancellationToken);
        Assert.DoesNotContain("super secret internal message", body.Detail ?? string.Empty);
    }

    // ── Base MarketplaceException (non-specific subclass) ────────────────────

    [Fact]
    public async Task TryHandleAsync_BaseMarketplaceException_Returns400()
    {
        var (handler, _) = BuildHandler();
        var context = BuildHttpContext();

        await handler.TryHandleAsync(context, new CustomMarketplaceException("bad input"), TestContext.Current.CancellationToken);

        Assert.Equal((int)HttpStatusCode.BadRequest, context.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_BaseMarketplaceException_LoggedAtWarningLevel()
    {
        var (handler, logger) = BuildHandler();
        var context = BuildHttpContext();

        await handler.TryHandleAsync(context, new CustomMarketplaceException("bad input"), TestContext.Current.CancellationToken);

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Warning, entry.Level);
    }

    [Fact]
    public async Task TryHandleAsync_BaseMarketplaceException_LogIncludesFullException()
    {
        var (handler, logger) = BuildHandler();
        var context = BuildHttpContext();
        var exception = new CustomMarketplaceException("bad input");

        await handler.TryHandleAsync(context, exception, TestContext.Current.CancellationToken);

        var entry = Assert.Single(logger.Entries);
        Assert.Same(exception, entry.Exception);
    }

    [Fact]
    public async Task TryHandleAsync_BaseMarketplaceException_LogIncludesRequestContext()
    {
        var (handler, logger) = BuildHandler();
        var userId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var context = BuildHttpContext(method: "DELETE", path: "/api/items/1", traceId: "trace-base", userId: userId);

        await handler.TryHandleAsync(context, new CustomMarketplaceException("bad input"), TestContext.Current.CancellationToken);

        var entry = Assert.Single(logger.Entries);
        Assert.Contains("DELETE", entry.Message);
        Assert.Contains("/api/items/1", entry.Message);
        Assert.Contains("trace-base", entry.Message);
        Assert.Contains("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", entry.Message);
    }

    // ── Request context logged for known exceptions ───────────────────────────

    [Fact]
    public async Task TryHandleAsync_NotFoundException_LogIncludesRequestContext()
    {
        var (handler, logger) = BuildHandler();
        var userId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        var context = BuildHttpContext(method: "GET", path: "/api/orders/42", traceId: "trace-nfe", userId: userId);

        await handler.TryHandleAsync(context, new NotFoundException("not found"), TestContext.Current.CancellationToken);

        var entry = Assert.Single(logger.Entries);
        Assert.Contains("GET", entry.Message);
        Assert.Contains("/api/orders/42", entry.Message);
        Assert.Contains("trace-nfe", entry.Message);
        Assert.Contains("cccccccc-cccc-cccc-cccc-cccccccccccc", entry.Message);
    }

    [Fact]
    public async Task TryHandleAsync_ForbiddenException_LogIncludesRequestContext()
    {
        var (handler, logger) = BuildHandler();
        var userId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
        var context = BuildHttpContext(method: "PUT", path: "/api/products/5", traceId: "trace-fbe", userId: userId);

        await handler.TryHandleAsync(context, new ForbiddenException("denied"), TestContext.Current.CancellationToken);

        var entry = Assert.Single(logger.Entries);
        Assert.Contains("PUT", entry.Message);
        Assert.Contains("/api/products/5", entry.Message);
        Assert.Contains("trace-fbe", entry.Message);
        Assert.Contains("dddddddd-dddd-dddd-dddd-dddddddddddd", entry.Message);
    }

    [Fact]
    public async Task TryHandleAsync_ConflictException_LogIncludesRequestContext()
    {
        var (handler, logger) = BuildHandler();
        var userId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
        var context = BuildHttpContext(method: "POST", path: "/api/users", traceId: "trace-cex", userId: userId);

        await handler.TryHandleAsync(context, new ConflictException("conflict"), TestContext.Current.CancellationToken);

        var entry = Assert.Single(logger.Entries);
        Assert.Contains("POST", entry.Message);
        Assert.Contains("/api/users", entry.Message);
        Assert.Contains("trace-cex", entry.Message);
        Assert.Contains("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee", entry.Message);
    }

    // ── StatusCode in unexpected exception log ────────────────────────────────

    [Fact]
    public async Task TryHandleAsync_UnexpectedException_LogIncludesStatusCode500()
    {
        var (handler, logger) = BuildHandler();
        var context = BuildHttpContext();

        await handler.TryHandleAsync(context, new InvalidOperationException("crash"), TestContext.Current.CancellationToken);

        var entry = Assert.Single(logger.Entries);
        Assert.Contains("500", entry.Message);
    }

    // ── Fakes ────────────────────────────────────────────────────────────────

    /// <summary>Concrete subclass used to test the base MarketplaceException handler branch.</summary>
    private sealed class CustomMarketplaceException(string message) : MarketplaceException(message);


    private sealed class FakeLogger : ILogger<GlobalExceptionHandler>
    {
        public record LogEntry(LogLevel Level, Exception? Exception, string Message);
        public List<LogEntry> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add(new LogEntry(logLevel, exception, formatter(state, exception)));
        }
    }

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public string ApplicationName { get; set; } = "TestApp";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = Environments.Production;
    }
}
