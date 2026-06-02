using System.Diagnostics;

namespace Backend.Api.Middleware;

public sealed class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestTimingMiddleware(RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(next);

        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, Serilog.IDiagnosticContext diagnosticContext)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(diagnosticContext);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            diagnosticContext.Set("DurationMs", Math.Round(stopwatch.Elapsed.TotalMilliseconds, 3));
        }
    }
}
