using System.Text.Json;
using System.Text.Json.Serialization;

namespace Backend.IntegrationTests;

internal static class IntegrationTestJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
