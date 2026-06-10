using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;

namespace Backend.Application.Interfaces.Services;

public interface ISellerVerificationService
{
    Task<Result<SellerVerificationResponse>> VerifySellerAsync(VerifySellerRequest request, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<SellerVerificationRequestDto>>> GetAllRequestsAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
