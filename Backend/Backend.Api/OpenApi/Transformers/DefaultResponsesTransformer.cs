using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Backend.Api.OpenApi.Transformers;

internal sealed class DefaultResponsesTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;
        operation.Responses ??= new OpenApiResponses();

        if (metadata.OfType<HttpGetAttribute>().Any())
        {
            operation.Responses.TryAdd("200", new OpenApiResponse { Description = "OK" });
            operation.Responses.TryAdd("404", new OpenApiResponse { Description = "Not Found" });
        }
        else if (metadata.OfType<HttpPostAttribute>().Any())
        {
            operation.Responses.TryAdd("201", new OpenApiResponse { Description = "Created" });
            operation.Responses.TryAdd("400", new OpenApiResponse { Description = "Bad Request" });
            operation.Responses.TryAdd("409", new OpenApiResponse { Description = "Conflict" });
        }
        else if (metadata.OfType<HttpPutAttribute>().Any())
        {
            operation.Responses.TryAdd("200", new OpenApiResponse { Description = "OK" });
            operation.Responses.TryAdd("400", new OpenApiResponse { Description = "Bad Request" });
            operation.Responses.TryAdd("404", new OpenApiResponse { Description = "Not Found" });
        }
        else if (metadata.OfType<HttpPatchAttribute>().Any())
        {
            operation.Responses.TryAdd("200", new OpenApiResponse { Description = "OK" });
            operation.Responses.TryAdd("400", new OpenApiResponse { Description = "Bad Request" });
            operation.Responses.TryAdd("404", new OpenApiResponse { Description = "Not Found" });
        }
        else if (metadata.OfType<HttpDeleteAttribute>().Any())
        {
            operation.Responses.TryAdd("204", new OpenApiResponse { Description = "No Content" });
            operation.Responses.TryAdd("404", new OpenApiResponse { Description = "Not Found" });
        }

        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
        operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });
        operation.Responses.TryAdd("500", new OpenApiResponse { Description = "Internal Server Error" });
        operation.Responses.TryAdd("501", new OpenApiResponse { Description = "Not Implemented" });

        return Task.CompletedTask;
    }
}