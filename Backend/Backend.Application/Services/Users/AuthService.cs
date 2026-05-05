using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Entities.IdentityAccess;
using Backend.Domain.Enums;

namespace Backend.Application.Services;

public sealed class AuthService : IAuthService
{
    private const string InvalidCredentialsMessage = "Invalid email or password.";

    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthTokenGenerator _authTokenGenerator;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuditLogService _auditLogService;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        IUserAccountRepository userAccountRepository,
        IPasswordHasher passwordHasher,
        IAuthTokenGenerator authTokenGenerator,
        IDateTimeProvider dateTimeProvider,
        IAuditLogService auditLogService,
        IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(userAccountRepository);
        ArgumentNullException.ThrowIfNull(passwordHasher);
        ArgumentNullException.ThrowIfNull(authTokenGenerator);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        ArgumentNullException.ThrowIfNull(auditLogService);
        ArgumentNullException.ThrowIfNull(unitOfWork);

        _userAccountRepository = userAccountRepository;
        _passwordHasher = passwordHasher;
        _authTokenGenerator = authTokenGenerator;
        _dateTimeProvider = dateTimeProvider;
        _auditLogService = auditLogService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthenticationResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var userAccount = await _userAccountRepository.GetByEmailAsync(email, cancellationToken);

        if (userAccount is null)
        {
            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: AuditActionType.Login,
                TargetEntityType: nameof(UserAccount),
                TargetEntityId: email,
                Outcome: AuditOutcome.Failed,
                Details: "Account not found"
            ), cancellationToken);

            return Result<AuthenticationResponse>.Unauthorized(InvalidCredentialsMessage);
        }

        var now = _dateTimeProvider.UtcNow;

        if (userAccount.IsBlocked || userAccount.AccountStatus is AccountStatus.Disabled or AccountStatus.Suspended)
        {
            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: AuditActionType.Login,
                TargetEntityType: nameof(UserAccount),
                TargetEntityId: userAccount.Id.ToString(),
                Outcome: AuditOutcome.Failed,
                Details: $"Account is {userAccount.AccountStatus}"
            ), cancellationToken);

            return Result<AuthenticationResponse>.Unauthorized(InvalidCredentialsMessage);
        }

        if (userAccount.LockedUntilUtc is not null && userAccount.LockedUntilUtc > now)
        {
            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: AuditActionType.Login,
                TargetEntityType: nameof(UserAccount),
                TargetEntityId: userAccount.Id.ToString(),
                Outcome: AuditOutcome.Failed,
                Details: $"Account locked until {userAccount.LockedUntilUtc}"
            ), cancellationToken);

            return Result<AuthenticationResponse>.Unauthorized(InvalidCredentialsMessage);
        }

        if (!_passwordHasher.VerifyPassword(userAccount, request.Password))
        {
            userAccount.FailedLoginAttempts += 1;

            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: AuditActionType.Login,
                TargetEntityType: nameof(UserAccount),
                TargetEntityId: userAccount.Id.ToString(),
                Outcome: AuditOutcome.Failed,
                Details: $"Invalid password attempt {userAccount.FailedLoginAttempts}"
            ), cancellationToken);

            if (userAccount.FailedLoginAttempts >= 5)
            {
                userAccount.AccountStatus = AccountStatus.Locked;
                userAccount.LockedUntilUtc = now.AddMinutes(15);

                await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                    ActionType: AuditActionType.Block,
                    TargetEntityType: nameof(UserAccount),
                    TargetEntityId: userAccount.Id.ToString(),
                    Outcome: AuditOutcome.Succeeded,
                    Details: $"Account temporarily locked after {userAccount.FailedLoginAttempts} failed attempts until {userAccount.LockedUntilUtc}"
                ), cancellationToken);
            }

            await _userAccountRepository.UpdateAsync(userAccount, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<AuthenticationResponse>.Unauthorized(InvalidCredentialsMessage);
        }

        // If the account was previously locked, log unlock event
        if (userAccount.AccountStatus == AccountStatus.Locked)
        {
            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: AuditActionType.Unblock,
                TargetEntityType: nameof(UserAccount),
                TargetEntityId: userAccount.Id.ToString(),
                Outcome: AuditOutcome.Succeeded,
                Details: "Account automatically unlocked after lockout period expired"
            ), cancellationToken);
        }

        userAccount.FailedLoginAttempts = 0;
        userAccount.LockedUntilUtc = null;
        userAccount.AccountStatus = AccountStatus.Active;
        userAccount.LastLoginAtUtc = now;

        await _userAccountRepository.UpdateAsync(userAccount, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
            ActionType: AuditActionType.Login,
            TargetEntityType: nameof(UserAccount),
            TargetEntityId: userAccount.Id.ToString(),
            Outcome: AuditOutcome.Succeeded,
            Details: "User logged in successfully"
        ), cancellationToken);

        var token = _authTokenGenerator.CreateToken(userAccount);

        return Result<AuthenticationResponse>.Success(new AuthenticationResponse(userAccount.ToUserAccountDto(), token));
    }
}
