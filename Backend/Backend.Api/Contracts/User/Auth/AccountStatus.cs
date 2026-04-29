using System.Text.Json.Serialization;

namespace Backend.Api.Contracts.User.Auth;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AccountStatus
{
    PendingActivation = 1,
    Active = 2,
    Suspended = 3,
    Locked = 4,
    Disabled = 5
}
