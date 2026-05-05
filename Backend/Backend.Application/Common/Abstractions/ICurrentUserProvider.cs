namespace Backend.Application.Common.Abstractions;

public interface ICurrentUserProvider
{
    Guid? UserId { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    string? IpAddress { get; }
}
