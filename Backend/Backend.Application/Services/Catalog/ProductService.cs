using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;

namespace Backend.Application.Services;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IProductListingRepository _productListingRepository;
    private readonly ICurrencyConversionService _currencyConversionService;

    public ProductService(
        IProductRepository productRepository,
        IProductListingRepository productListingRepository,
        ICurrencyConversionService currencyConversionService)
    {
        ArgumentNullException.ThrowIfNull(productRepository);
        ArgumentNullException.ThrowIfNull(productListingRepository);
        ArgumentNullException.ThrowIfNull(currencyConversionService);

        _productRepository = productRepository;
        _productListingRepository = productListingRepository;
        _currencyConversionService = currencyConversionService;
    }

    public Task<Result<ProductDto>> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<ProductDto>.NotImplemented());
    }

    public async Task<Result<ProductDetailsDto>> GetDetailsAsync(Guid productId, Guid? listingId, string? currency, CancellationToken cancellationToken = default)
    {
        var currencyCode = _currencyConversionService.NormalizeOrDefault(currency);
        if (!_currencyConversionService.IsSupported(currencyCode))
        {
            return Result<ProductDetailsDto>.ValidationFailure("Currency must be one of BRL, USD, or DKK.");
        }

        var listing = await _productListingRepository.GetAvailableProductDetailAsync(productId, listingId, cancellationToken);

        if (listing is null)
        {
            return Result<ProductDetailsDto>.NotFound("Product was not found.");
        }

        return Result<ProductDetailsDto>.Success(
            listing.ToProductDetailsDto(currencyCode, _currencyConversionService.FromBaseCurrency(listing.ListingPrice, currencyCode)));
    }

    public async Task<Result<PagedResult<BrowseProductDto>>> GetBrowseProductsAsync(BrowseProductsRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Page < 1 || request.PageSize < 1)
        {
            return Result<PagedResult<BrowseProductDto>>.ValidationFailure("Page and page size must be greater than zero.");
        }

        if (request.Sort is not "newest" and not "price-asc" and not "price-desc" and not "name-asc")
        {
            return Result<PagedResult<BrowseProductDto>>.ValidationFailure("Sort must be one of newest, price-asc, price-desc, or name-asc.");
        }

        var currencyCode = _currencyConversionService.NormalizeOrDefault(request.Currency);
        if (!_currencyConversionService.IsSupported(currencyCode))
        {
            return Result<PagedResult<BrowseProductDto>>.ValidationFailure("Currency must be one of BRL, USD, or DKK.");
        }

        var listings = await _productListingRepository.GetAvailableForBrowseAsync(request, cancellationToken);
        var products = listings.Items
            .Select(listing => listing.ToBrowseDto(currencyCode, _currencyConversionService.FromBaseCurrency(listing.ListingPrice, currencyCode)))
            .ToArray();

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

