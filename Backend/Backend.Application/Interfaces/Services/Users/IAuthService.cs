using Backend.Application.Common.Results;
using Backend.Application.DTOs;

namespace Backend.Application.Interfaces.Services;

public interface IAuthService
{
    Task<Result<AuthenticationResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
