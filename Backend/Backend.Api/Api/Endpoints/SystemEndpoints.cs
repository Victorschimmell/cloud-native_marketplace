namespace Backend.Api.Endpoints;

public static class SystemEndpoints
{
    public static IEndpointRouteBuilder MapSystemEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/system").WithTags("System");

        group.MapGet("/info", () => Results.Ok(new
        {
            Service = "Marketplace Backend",
            TimestampUtc = DateTimeOffset.UtcNow
        }));

        return app;
    }
}
