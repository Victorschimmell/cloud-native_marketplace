using System.Security.Claims;
using Backend.Application.Common.Abstractions;

namespace Backend.Api.Auth;

internal sealed class HttpContextCurrentUserProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUserProvider
{
    public Guid? UserId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }

    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public bool IsAdmin =>
        httpContextAccessor.HttpContext?.User.IsInRole("Admin") == true ||
        httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role) == "Admin";
}
