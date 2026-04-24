using Backend.Application.Common.Abstractions;

namespace Backend.Infrastructure.Auth;

internal sealed class InfrastructureDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => throw new NotImplementedException();
}
