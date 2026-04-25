using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
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

    public Task<Result<RegistrationResponse>> RegisterCustomerAsync(RegisterCustomerRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<RegistrationResponse>.NotImplemented());
    }

    public Task<Result<RegistrationResponse>> RegisterSellerAsync(RegisterSellerRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<RegistrationResponse>.NotImplemented());
    }
}
