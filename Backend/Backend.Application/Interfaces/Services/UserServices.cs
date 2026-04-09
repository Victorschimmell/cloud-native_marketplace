using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;

namespace Backend.Application.Interfaces.Services;

public interface ICustomerService
{
    Task<Result<CustomerDto>> GetByIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<CustomerDto>>> GetCustomersAsync(PagedRequest request, CancellationToken cancellationToken = default);
}

public interface ISellerService
{
    Task<Result<SellerDto>> GetByIdAsync(Guid sellerId, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<SellerDto>>> GetSellersAsync(PagedRequest request, CancellationToken cancellationToken = default);
}

public interface IAuthService
{
    Task<Result<AuthenticationResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}

public interface IRegistrationService
{
    Task<Result<RegistrationResponse>> RegisterCustomerAsync(RegisterCustomerRequest request, CancellationToken cancellationToken = default);
    Task<Result<RegistrationResponse>> RegisterSellerAsync(RegisterSellerRequest request, CancellationToken cancellationToken = default);
}

public interface ISellerVerificationService
{
    Task<Result<SellerVerificationResponse>> SubmitVerificationAsync(SubmitSellerVerificationRequest request, CancellationToken cancellationToken = default);
    Task<Result<SellerVerificationResponse>> VerifySellerAsync(VerifySellerRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<SellerVerificationRequestDto>>> GetRequestsBySellerAsync(Guid sellerId, CancellationToken cancellationToken = default);
}
