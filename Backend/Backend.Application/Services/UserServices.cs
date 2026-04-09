using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Entities.IdentityAccess;
using Backend.Domain.Enums;
using Backend.Domain.ValueObjects;

namespace Backend.Application.Services;

public sealed class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        ArgumentNullException.ThrowIfNull(customerRepository);
        _customerRepository = customerRepository;
    }

    public async Task<Result<CustomerDto>> GetByIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        if (customerId == Guid.Empty)
        {
            return Result<CustomerDto>.Failure("Customer id is required.");
        }

        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
        return customer is null
            ? Result<CustomerDto>.Failure("Customer was not found.")
            : Result<CustomerDto>.Success(customer.ToDto());
    }

    public async Task<Result<PagedResult<CustomerDto>>> GetCustomersAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Page <= 0 || request.PageSize <= 0)
        {
            return Result<PagedResult<CustomerDto>>.Failure("Page and page size must be greater than zero.");
        }

        var customers = await _customerRepository.GetAllAsync(request.Page, request.PageSize, cancellationToken);
        var items = customers.Select(static customer => customer.ToDto()).ToArray();
        return Result<PagedResult<CustomerDto>>.Success(new PagedResult<CustomerDto>(items, request.Page, request.PageSize, items.Length));
    }
}

public sealed class SellerService : ISellerService
{
    private readonly ISellerRepository _sellerRepository;

    public SellerService(ISellerRepository sellerRepository)
    {
        ArgumentNullException.ThrowIfNull(sellerRepository);
        _sellerRepository = sellerRepository;
    }

    public async Task<Result<SellerDto>> GetByIdAsync(Guid sellerId, CancellationToken cancellationToken = default)
    {
        if (sellerId == Guid.Empty)
        {
            return Result<SellerDto>.Failure("Seller id is required.");
        }

        var seller = await _sellerRepository.GetByIdAsync(sellerId, cancellationToken);
        return seller is null
            ? Result<SellerDto>.Failure("Seller was not found.")
            : Result<SellerDto>.Success(seller.ToDto());
    }

    public async Task<Result<PagedResult<SellerDto>>> GetSellersAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Page <= 0 || request.PageSize <= 0)
        {
            return Result<PagedResult<SellerDto>>.Failure("Page and page size must be greater than zero.");
        }

        var sellers = await _sellerRepository.GetAllAsync(request.Page, request.PageSize, cancellationToken);
        var items = sellers.Select(static seller => seller.ToDto()).ToArray();
        return Result<PagedResult<SellerDto>>.Success(new PagedResult<SellerDto>(items, request.Page, request.PageSize, items.Length));
    }
}

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
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Result<AuthenticationResponse>.Failure("Email and password are required.");
        }

        var userAccount = await _userAccountRepository.GetByEmailAsync(request.Email.Trim(), cancellationToken);
        if (userAccount is null || !_passwordHasher.VerifyPassword(userAccount, request.Password))
        {
            return Result<AuthenticationResponse>.Failure("Invalid email or password.");
        }

        userAccount.LastLoginAtUtc = _dateTimeProvider.UtcNow;
        await _userAccountRepository.UpdateAsync(userAccount, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new AuthenticationResponse(userAccount.ToDto(), _authTokenGenerator.CreateToken(userAccount));
        return Result<AuthenticationResponse>.Success(response);
    }
}

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
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Result<RegistrationResponse>.Failure("Email and password are required.");
        }

        if (await _userAccountRepository.GetByEmailAsync(request.Email.Trim(), cancellationToken) is not null)
        {
            return Result<RegistrationResponse>.Failure("A user with the same email already exists.");
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RegistrationResponse>.Success(new RegistrationResponse(userAccount.ToDto(), customer.ToDto(), null));
    }

    public async Task<Result<RegistrationResponse>> RegisterSellerAsync(RegisterSellerRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Result<RegistrationResponse>.Failure("Email and password are required.");
        }

        if (await _userAccountRepository.GetByEmailAsync(request.Email.Trim(), cancellationToken) is not null)
        {
            return Result<RegistrationResponse>.Failure("A user with the same email already exists.");
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RegistrationResponse>.Success(new RegistrationResponse(userAccount.ToDto(), null, seller.ToDto()));
    }
}

public sealed class SellerVerificationService : ISellerVerificationService
{
    private readonly ISellerVerificationRequestRepository _verificationRequestRepository;
    private readonly ISellerRepository _sellerRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public SellerVerificationService(
        ISellerVerificationRequestRepository verificationRequestRepository,
        ISellerRepository sellerRepository,
        ICurrentUserProvider currentUserProvider,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(verificationRequestRepository);
        ArgumentNullException.ThrowIfNull(sellerRepository);
        ArgumentNullException.ThrowIfNull(currentUserProvider);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        ArgumentNullException.ThrowIfNull(unitOfWork);

        _verificationRequestRepository = verificationRequestRepository;
        _sellerRepository = sellerRepository;
        _currentUserProvider = currentUserProvider;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SellerVerificationResponse>> SubmitVerificationAsync(SubmitSellerVerificationRequest request, CancellationToken cancellationToken = default)
    {
        if (request.SellerId == Guid.Empty || string.IsNullOrWhiteSpace(request.SubmittedDetails))
        {
            return Result<SellerVerificationResponse>.Failure("Seller id and submitted details are required.");
        }

        var seller = await _sellerRepository.GetByIdAsync(request.SellerId, cancellationToken);
        if (seller is null)
        {
            return Result<SellerVerificationResponse>.Failure("Seller was not found.");
        }

        var verificationRequest = new SellerVerificationRequest
        {
            SellerId = seller.Id,
            SubmittedAtUtc = _dateTimeProvider.UtcNow,
            Status = SellerVerificationRequestStatus.Submitted,
            BusinessNameSnapshot = seller.BusinessName,
            RegistrationNumberSnapshot = seller.RegistrationNumber,
            SubmittedDetails = request.SubmittedDetails.Trim()
        };

        await _verificationRequestRepository.AddAsync(verificationRequest, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<SellerVerificationResponse>.Success(new SellerVerificationResponse(verificationRequest.ToDto(), seller.ToDto()));
    }

    public async Task<Result<SellerVerificationResponse>> VerifySellerAsync(VerifySellerRequest request, CancellationToken cancellationToken = default)
    {
        if (request.VerificationRequestId == Guid.Empty || request.SellerId == Guid.Empty)
        {
            return Result<SellerVerificationResponse>.Failure("Verification request id and seller id are required.");
        }

        var verificationRequest = await _verificationRequestRepository.GetByIdAsync(request.VerificationRequestId, cancellationToken);
        var seller = await _sellerRepository.GetByIdAsync(request.SellerId, cancellationToken);

        if (verificationRequest is null || seller is null)
        {
            return Result<SellerVerificationResponse>.Failure("Seller verification request was not found.");
        }

        verificationRequest.Status = request.Approve ? SellerVerificationRequestStatus.Approved : SellerVerificationRequestStatus.Rejected;
        verificationRequest.ReviewNotes = request.ReviewNotes;
        verificationRequest.RejectionReason = request.Approve ? null : request.RejectionReason;
        verificationRequest.ReviewedAtUtc = _dateTimeProvider.UtcNow;
        verificationRequest.ReviewedByUserId = _currentUserProvider.UserId;

        seller.VerificationStatus = request.Approve ? VerificationStatus.Verified : VerificationStatus.Rejected;
        seller.VerifiedAtUtc = request.Approve ? _dateTimeProvider.UtcNow : null;

        await _verificationRequestRepository.UpdateAsync(verificationRequest, cancellationToken);
        await _sellerRepository.UpdateAsync(seller, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<SellerVerificationResponse>.Success(new SellerVerificationResponse(verificationRequest.ToDto(), seller.ToDto()));
    }

    public async Task<Result<IReadOnlyList<SellerVerificationRequestDto>>> GetRequestsBySellerAsync(Guid sellerId, CancellationToken cancellationToken = default)
    {
        if (sellerId == Guid.Empty)
        {
            return Result<IReadOnlyList<SellerVerificationRequestDto>>.Failure("Seller id is required.");
        }

        var requests = await _verificationRequestRepository.GetBySellerIdAsync(sellerId, cancellationToken);
        return Result<IReadOnlyList<SellerVerificationRequestDto>>.Success(requests.Select(static request => request.ToDto()).ToArray());
    }
}
