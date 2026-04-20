using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;

namespace Backend.Application.Interfaces.Services;

public interface ISellerService
{
    Task<Result<SellerDto>> GetByIdAsync(Guid sellerId, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<SellerDto>>> GetSellersAsync(PagedRequest request, CancellationToken cancellationToken = default);
}
