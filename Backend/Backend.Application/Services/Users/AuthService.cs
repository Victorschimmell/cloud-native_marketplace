using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;

namespace Backend.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthTokenGenerator _authTokenGenerator;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AuthService(
        IUserAccountRepository userAccountRepository,
        IPasswordHasher passwordHasher,
        IAuthTokenGenerator authTokenGenerator,
        IDateTimeProvider dateTimeProvider)
    {
        ArgumentNullException.ThrowIfNull(userAccountRepository);
        ArgumentNullException.ThrowIfNull(passwordHasher);
        ArgumentNullException.ThrowIfNull(authTokenGenerator);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);

        _userAccountRepository = userAccountRepository;
        _passwordHasher = passwordHasher;
        _authTokenGenerator = authTokenGenerator;
        _dateTimeProvider = dateTimeProvider;
    }

    public Task<Result<AuthenticationResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<AuthenticationResponse>.NotImplemented());
    }
}
