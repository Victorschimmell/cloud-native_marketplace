using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
namespace Backend.Application.Services;

public sealed class SellerVerificationService : ISellerVerificationService
{
    private readonly ISellerVerificationRequestRepository _verificationRequestRepository;
    private readonly ISellerRepository _sellerRepository;
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SellerVerificationService(
        ISellerVerificationRequestRepository verificationRequestRepository,
        ISellerRepository sellerRepository,
        IUserAccountRepository userAccountRepository,
        ICurrentUserProvider currentUserProvider,
        IDateTimeProvider dateTimeProvider)
    {
        ArgumentNullException.ThrowIfNull(verificationRequestRepository);
        ArgumentNullException.ThrowIfNull(sellerRepository);
        ArgumentNullException.ThrowIfNull(userAccountRepository);
        ArgumentNullException.ThrowIfNull(currentUserProvider);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);

        _verificationRequestRepository = verificationRequestRepository;
        _sellerRepository = sellerRepository;
        _userAccountRepository = userAccountRepository;
        _currentUserProvider = currentUserProvider;
        _dateTimeProvider = dateTimeProvider;
    }

    public Task<Result<SellerVerificationResponse>> SubmitVerificationAsync(SubmitSellerVerificationRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<SellerVerificationResponse>> VerifySellerAsync(VerifySellerRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IReadOnlyList<SellerVerificationRequestDto>>> GetRequestsBySellerAsync(Guid sellerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
