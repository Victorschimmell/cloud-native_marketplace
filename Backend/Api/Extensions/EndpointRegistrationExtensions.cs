using Backend.Api.Endpoints;

namespace Backend.Api.Extensions;

public static class EndpointRegistrationExtensions
{
    public static IEndpointRouteBuilder MapFoundationEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapSystemEndpoints();
        return app;
    }
}
