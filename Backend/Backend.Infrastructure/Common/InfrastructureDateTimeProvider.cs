using Backend.Application.Common.Abstractions;

namespace Backend.Infrastructure.Common;

internal sealed class InfrastructureDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
