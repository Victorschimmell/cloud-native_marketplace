using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;

namespace Backend.Application.Services;

public sealed class SellerService : ISellerService
{
    private readonly ISellerRepository _sellerRepository;

    public SellerService(ISellerRepository sellerRepository)
    {
        ArgumentNullException.ThrowIfNull(sellerRepository);
        _sellerRepository = sellerRepository;
    }

    public Task<Result<SellerDto>> GetByIdAsync(Guid sellerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<PagedResult<SellerDto>>> GetSellersAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
