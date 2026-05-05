using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Entities.IdentityAccess;
using Backend.Domain.Enums;
using Backend.Domain.ValueObjects;

namespace Backend.Application.Services;

public sealed class RegistrationService : IRegistrationService
{
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ISellerRepository _sellerRepository;
    private readonly ISellerVerificationRequestRepository _sellerVerificationRequestRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthTokenGenerator _authTokenGenerator;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuditLogService _auditLogService;
    private readonly IUnitOfWork _unitOfWork;

    public RegistrationService(
        IUserAccountRepository userAccountRepository,
        ICustomerRepository customerRepository,
        ISellerRepository sellerRepository,
        ISellerVerificationRequestRepository sellerVerificationRequestRepository,
        IPasswordHasher passwordHasher,
        IAuthTokenGenerator authTokenGenerator,
        IDateTimeProvider dateTimeProvider,
        IAuditLogService auditLogService,
        IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(userAccountRepository);
        ArgumentNullException.ThrowIfNull(customerRepository);
        ArgumentNullException.ThrowIfNull(sellerRepository);
        ArgumentNullException.ThrowIfNull(sellerVerificationRequestRepository);
        ArgumentNullException.ThrowIfNull(passwordHasher);
        ArgumentNullException.ThrowIfNull(authTokenGenerator);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        ArgumentNullException.ThrowIfNull(auditLogService);
        ArgumentNullException.ThrowIfNull(unitOfWork);

        _userAccountRepository = userAccountRepository;
        _customerRepository = customerRepository;
        _sellerRepository = sellerRepository;
        _sellerVerificationRequestRepository = sellerVerificationRequestRepository;
        _passwordHasher = passwordHasher;
        _authTokenGenerator = authTokenGenerator;
        _dateTimeProvider = dateTimeProvider;
        _auditLogService = auditLogService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RegistrationResponse>> RegisterCustomerAsync(RegisterCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var existingUser = await _userAccountRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (existingUser is not null)
        {
            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: AuditActionType.Created,
                TargetEntityType: nameof(UserAccount),
                TargetEntityId: normalizedEmail,
                Outcome: AuditOutcome.Failed,
                Details: $"Customer registration failed, email already in use: {normalizedEmail}"
            ), cancellationToken);

            return Result<RegistrationResponse>.Conflict("An account with this email already exists.");
        }

        var userAccount = CreateUserAccount(normalizedEmail, request.Password);
        var customer = new Customer
        {
            UserId = userAccount.Id,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Phone = request.Phone.Trim(),
            DefaultAddressId = request.DefaultAddressId
        };

        try
        {
            await _userAccountRepository.AddAsync(userAccount, cancellationToken);
            await _customerRepository.AddAsync(customer, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: AuditActionType.Created,
                TargetEntityType: nameof(UserAccount),
                TargetEntityId: customer.Id.ToString(),
                Outcome: AuditOutcome.Succeeded,
                Details: $"New customer account registered with email {request.Email}"
            ), cancellationToken);
        }
        catch (UniqueConstraintViolationException exception) when (exception.Target == UniqueConstraintTarget.UserAccountEmail)
        {
            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: AuditActionType.Created,
                TargetEntityType: nameof(UserAccount),
                TargetEntityId: normalizedEmail,
                Outcome: AuditOutcome.Failed,
                Details: $"Customer registration failed, email already in use: {normalizedEmail}"
            ), cancellationToken);

            return Result<RegistrationResponse>.Conflict("An account with this email already exists.");
        }

        return Result<RegistrationResponse>.Success(new RegistrationResponse(
            userAccount.ToUserAccountDto(),
            customer.ToCustomerDto(),
            null,
            _authTokenGenerator.CreateToken(userAccount)));
    }

    public async Task<Result<RegistrationResponse>> RegisterSellerAsync(RegisterSellerRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var existingUser = await _userAccountRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (existingUser is not null)
        {
            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: AuditActionType.Created,
                TargetEntityType: nameof(UserAccount),
                TargetEntityId: normalizedEmail,
                Outcome: AuditOutcome.Failed,
                Details: $"Seller registration failed, email already in use: {normalizedEmail}"
            ), cancellationToken);

            return Result<RegistrationResponse>.Conflict("Registration failed. Please check your information and try again.");
        }

        var userAccount = CreateUserAccount(normalizedEmail, request.Password);
        var seller = new Seller
        {
            UserId = userAccount.Id,
            BusinessName = request.BusinessName.Trim(),
            RegistrationNumber = request.RegistrationNumber.Trim(),
            PayoutInformation = request.PayoutInformation.Trim(),
            DefaultAddressId = request.DefaultAddressId,
            VerificationStatus = VerificationStatus.Pending
        };
        var verificationRequest = new SellerVerificationRequest
        {
            SellerId = seller.Id,
            SubmittedAtUtc = _dateTimeProvider.UtcNow,
            Status = SellerVerificationRequestStatus.Submitted,
            BusinessNameSnapshot = seller.BusinessName,
            RegistrationNumberSnapshot = seller.RegistrationNumber,
            SubmittedDetails = "Created during seller registration."
        };

        try
        {
            await _userAccountRepository.AddAsync(userAccount, cancellationToken);
            await _sellerRepository.AddAsync(seller, cancellationToken);
            await _sellerVerificationRequestRepository.AddAsync(verificationRequest, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: AuditActionType.Created,
                TargetEntityType: nameof(UserAccount),
                TargetEntityId: seller.Id.ToString(),
                Outcome: AuditOutcome.Succeeded,
                Details: $"New seller account registered with email {request.Email}"
            ), cancellationToken);
        }
        catch (UniqueConstraintViolationException exception) when (exception.Target == UniqueConstraintTarget.UserAccountEmail)
        {
            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: AuditActionType.Created,
                TargetEntityType: nameof(UserAccount),
                TargetEntityId: normalizedEmail,
                Outcome: AuditOutcome.Failed,
                Details: $"Seller registration failed, email already in use: {normalizedEmail}"
            ), cancellationToken);

            return Result<RegistrationResponse>.Conflict("Registration failed. Please check your information and try again.");
        }

        return Result<RegistrationResponse>.Success(new RegistrationResponse(
            userAccount.ToUserAccountDto(),
            null,
            seller.ToSellerDto()));
    }

    private UserAccount CreateUserAccount(string normalizedEmail, string password) =>
        new()
        {
            Email = new EmailAddress(normalizedEmail),
            PasswordHash = _passwordHasher.HashPassword(password),
            AccountStatus = AccountStatus.Active
        };

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
