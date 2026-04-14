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

    public RegistrationService(
        IUserAccountRepository userAccountRepository,
        ICustomerRepository customerRepository,
        ISellerRepository sellerRepository,
        IPasswordHasher passwordHasher)
    {
        ArgumentNullException.ThrowIfNull(userAccountRepository);
        ArgumentNullException.ThrowIfNull(customerRepository);
        ArgumentNullException.ThrowIfNull(sellerRepository);
        ArgumentNullException.ThrowIfNull(passwordHasher);

        _userAccountRepository = userAccountRepository;
        _customerRepository = customerRepository;
        _sellerRepository = sellerRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<RegistrationResponse>> RegisterCustomerAsync(RegisterCustomerRequest request, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Result<RegistrationResponse>.ValidationFailure("Email and password are required.");
            }

            if (await _userAccountRepository.GetByEmailAsync(request.Email.Trim(), cancellationToken) is not null)
            {
                return Result<RegistrationResponse>.Conflict("A user with the same email already exists.");
            }

            var userAccount = new UserAccount
            {
                Email = new EmailAddress(request.Email.Trim()),
                PasswordHash = _passwordHasher.HashPassword(request.Password),
                AccountStatus = AccountStatus.Active
            };

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

            return Result<RegistrationResponse>.Success(new RegistrationResponse(userAccount.ToDto(), customer.ToDto(), null));
        }, "Unable to register customer.");
    }

    public async Task<Result<RegistrationResponse>> RegisterSellerAsync(RegisterSellerRequest request, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Result<RegistrationResponse>.ValidationFailure("Email and password are required.");
            }

            if (await _userAccountRepository.GetByEmailAsync(request.Email.Trim(), cancellationToken) is not null)
            {
                return Result<RegistrationResponse>.Conflict("A user with the same email already exists.");
            }

            var userAccount = new UserAccount
            {
                Email = new EmailAddress(request.Email.Trim()),
                PasswordHash = _passwordHasher.HashPassword(request.Password),
                AccountStatus = AccountStatus.PendingActivation
            };

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

            return Result<RegistrationResponse>.Success(new RegistrationResponse(userAccount.ToDto(), null, seller.ToDto()));
        }, "Unable to register seller.");
    }
}
