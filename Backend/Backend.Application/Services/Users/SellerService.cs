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

    public async Task<Result<SellerDto>> GetByIdAsync(Guid sellerId, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (sellerId == Guid.Empty)
            {
                return Result<SellerDto>.ValidationFailure("Seller id is required.");
            }

            var seller = await _sellerRepository.GetByIdAsync(sellerId, cancellationToken);
            return seller is null
                ? Result<SellerDto>.NotFound("Seller was not found.")
                : Result<SellerDto>.Success(seller.ToDto());
        }, "Unable to get seller.");
    }

    public async Task<Result<PagedResult<SellerDto>>> GetSellersAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (request.Page <= 0 || request.PageSize <= 0)
            {
                return Result<PagedResult<SellerDto>>.ValidationFailure("Page and page size must be greater than zero.");
            }

            var sellers = await _sellerRepository.GetAllAsync(request.Page, request.PageSize, cancellationToken);
            var items = sellers.Select(static seller => seller.ToDto()).ToArray();
            return Result<PagedResult<SellerDto>>.Success(new PagedResult<SellerDto>(items, request.Page, request.PageSize, items.Length));
        }, "Unable to get sellers.");
    }
}
