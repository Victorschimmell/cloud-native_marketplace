using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Enums;

namespace Backend.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthTokenGenerator _authTokenGenerator;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        IUserAccountRepository userAccountRepository,
        IPasswordHasher passwordHasher,
        IAuthTokenGenerator authTokenGenerator,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(userAccountRepository);
        ArgumentNullException.ThrowIfNull(passwordHasher);
        ArgumentNullException.ThrowIfNull(authTokenGenerator);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        ArgumentNullException.ThrowIfNull(unitOfWork);

        _userAccountRepository = userAccountRepository;
        _passwordHasher = passwordHasher;
        _authTokenGenerator = authTokenGenerator;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthenticationResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var userAccount = await _userAccountRepository.GetByEmailAsync(email, cancellationToken);

        if (userAccount is null)
        {
            return Result<AuthenticationResponse>.Unauthorized("Invalid email or password.");
        }

        var now = _dateTimeProvider.UtcNow;

        if (userAccount.IsBlocked || userAccount.AccountStatus is AccountStatus.Disabled or AccountStatus.Suspended)
        {
            return Result<AuthenticationResponse>.Failure("This account cannot sign in.", ResultFailureType.Forbidden);
        }

        if (userAccount.LockedUntilUtc is not null && userAccount.LockedUntilUtc > now)
        {
            return Result<AuthenticationResponse>.Unauthorized("This account is temporarily locked.");
        }

        if (!_passwordHasher.VerifyPassword(userAccount, request.Password))
        {
            userAccount.FailedLoginAttempts += 1;

            if (userAccount.FailedLoginAttempts >= 5)
            {
                userAccount.AccountStatus = AccountStatus.Locked;
                userAccount.LockedUntilUtc = now.AddMinutes(15);
            }

            await _userAccountRepository.UpdateAsync(userAccount, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<AuthenticationResponse>.Unauthorized("Invalid email or password.");
        }

        userAccount.FailedLoginAttempts = 0;
        userAccount.LockedUntilUtc = null;
        userAccount.AccountStatus = AccountStatus.Active;
        userAccount.LastLoginAtUtc = now;

        await _userAccountRepository.UpdateAsync(userAccount, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var token = _authTokenGenerator.CreateToken(userAccount);

        return Result<AuthenticationResponse>.Success(new AuthenticationResponse(userAccount.ToUserAccountDto(), token));
    }
}
