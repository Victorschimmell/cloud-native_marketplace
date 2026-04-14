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

    public async Task<Result<AuthenticationResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Result<AuthenticationResponse>.ValidationFailure("Email and password are required.");
            }

            var userAccount = await _userAccountRepository.GetByEmailAsync(request.Email.Trim(), cancellationToken);
            if (userAccount is null)
            {
                return Result<AuthenticationResponse>.Unauthorized("Invalid email or password.");
            }

            if (!_passwordHasher.VerifyPassword(userAccount, request.Password))
            {
                userAccount.FailedLoginAttempts += 1;
                await _userAccountRepository.UpdateAsync(userAccount, cancellationToken);
                return Result<AuthenticationResponse>.Unauthorized("Invalid email or password.");
            }

            userAccount.FailedLoginAttempts = 0;
            userAccount.LastLoginAtUtc = _dateTimeProvider.UtcNow;
            await _userAccountRepository.UpdateAsync(userAccount, cancellationToken);

            var response = new AuthenticationResponse(userAccount.ToDto(), _authTokenGenerator.CreateToken(userAccount));
            return Result<AuthenticationResponse>.Success(response);
        }, "Unable to authenticate user.");
    }
}
