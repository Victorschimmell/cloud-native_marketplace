using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;

namespace Backend.Application.Services;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IProductListingRepository _productListingRepository;

    public ProductService(
        IProductRepository productRepository,
        IProductListingRepository productListingRepository)
    {
        ArgumentNullException.ThrowIfNull(productRepository);
        ArgumentNullException.ThrowIfNull(productListingRepository);

        _productRepository = productRepository;
        _productListingRepository = productListingRepository;
    }

    public Task<Result<ProductDto>> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<ProductDto>.NotImplemented());
    }

    public async Task<Result<PagedResult<BrowseProductDto>>> GetBrowseProductsAsync(Guid? categoryId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Page < 1 || request.PageSize < 1)
        {
            return Result<PagedResult<BrowseProductDto>>.ValidationFailure("Page and page size must be greater than zero.");
        }

        var listings = await _productListingRepository.GetAvailableForBrowseAsync(categoryId, request, cancellationToken);
        var products = listings.Items.Select(ApplicationMappings.ToBrowseDto).ToArray();

        return Result<PagedResult<BrowseProductDto>>.Success(
            new PagedResult<BrowseProductDto>(products, listings.Page, listings.PageSize, listings.TotalCount));
    }

    public Task<Result<PagedResult<ProductDto>>> GetByCategoryAsync(Guid categoryId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<PagedResult<ProductDto>>.NotImplemented());
    }

    public Task<Result<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<ProductDto>.NotImplemented());
    }

    public Task<Result<ProductDto>> UpdateAsync(UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<ProductDto>.NotImplemented());
    }

    public Task<Result> DeleteAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result.NotImplemented());
    }
}

