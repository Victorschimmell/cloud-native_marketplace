using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
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
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public RegistrationService(
        IUserAccountRepository userAccountRepository,
        ICustomerRepository customerRepository,
        ISellerRepository sellerRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(userAccountRepository);
        ArgumentNullException.ThrowIfNull(customerRepository);
        ArgumentNullException.ThrowIfNull(sellerRepository);
        ArgumentNullException.ThrowIfNull(passwordHasher);
        ArgumentNullException.ThrowIfNull(unitOfWork);

        _userAccountRepository = userAccountRepository;
        _customerRepository = customerRepository;
        _sellerRepository = sellerRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RegistrationResponse>> RegisterCustomerAsync(RegisterCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var existingUser = await _userAccountRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (existingUser is not null)
        {
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

        await _userAccountRepository.AddAsync(userAccount, cancellationToken);
        await _customerRepository.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RegistrationResponse>.Success(new RegistrationResponse(
            userAccount.ToUserAccountDto(),
            customer.ToCustomerDto(),
            null));
    }

    public async Task<Result<RegistrationResponse>> RegisterSellerAsync(RegisterSellerRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var existingUser = await _userAccountRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (existingUser is not null)
        {
            return Result<RegistrationResponse>.Conflict("An account with this email already exists.");
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

        await _userAccountRepository.AddAsync(userAccount, cancellationToken);
        await _sellerRepository.AddAsync(seller, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
