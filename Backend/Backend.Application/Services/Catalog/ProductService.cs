using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Entities.Catalog;
using Backend.Domain.Enums;

namespace Backend.Application.Services;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IProductListingRepository _productListingRepository;
    private readonly ISellerRepository _sellerRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrencyConversionService _currencyConversionService;

    public ProductService(
        IProductRepository productRepository,
        IProductListingRepository productListingRepository,
        ISellerRepository sellerRepository,
        ICurrentUserProvider currentUserProvider,
        IUnitOfWork unitOfWork,
        ICurrencyConversionService currencyConversionService)
    {
        ArgumentNullException.ThrowIfNull(productRepository);
        ArgumentNullException.ThrowIfNull(productListingRepository);
        ArgumentNullException.ThrowIfNull(sellerRepository);
        ArgumentNullException.ThrowIfNull(currentUserProvider);
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(currencyConversionService);

        _productRepository = productRepository;
        _productListingRepository = productListingRepository;
        _sellerRepository = sellerRepository;
        _currentUserProvider = currentUserProvider;
        _unitOfWork = unitOfWork;
        _currencyConversionService = currencyConversionService;
    }

    public Task<Result<ProductDto>> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<ProductDto>.NotImplemented());
    }

    public async Task<Result<ProductDetailsDto>> GetDetailsAsync(Guid productId, Guid? listingId, string? currency, CancellationToken cancellationToken = default)
    {
        if (!_currencyConversionService.TryGetPriceConverter(currency, out var currencyCode, out var priceConverter))
        {
            return Result<ProductDetailsDto>.ValidationFailure("Currency must be one of BRL, USD, or DKK.");
        }

        var listing = await _productListingRepository.GetAvailableProductDetailAsync(productId, listingId, cancellationToken);

        if (listing is null)
        {
            return Result<ProductDetailsDto>.NotFound("Product was not found.");
        }

        return Result<ProductDetailsDto>.Success(
            listing.ToProductDetailsDto(currencyCode, priceConverter(listing.ListingPrice)));
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

        if (!_currencyConversionService.TryGetPriceConverter(request.Currency, out var currencyCode, out var priceConverter))
        {
            return Result<PagedResult<BrowseProductDto>>.ValidationFailure("Currency must be one of BRL, USD, or DKK.");
        }

        var listings = await _productListingRepository.GetAvailableForBrowseAsync(request, cancellationToken);
        var products = listings.Items
            .Select(listing => listing.ToBrowseDto(currencyCode, priceConverter(listing.ListingPrice)))
            .ToArray();

        return Result<PagedResult<BrowseProductDto>>.Success(
            new PagedResult<BrowseProductDto>(products, listings.Page, listings.PageSize, listings.TotalCount));
    }

    public Task<Result<PagedResult<ProductDto>>> GetByCategoryAsync(Guid categoryId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<PagedResult<ProductDto>>.NotImplemented());
    }

    public async Task<Result<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var validationError = ValidateProductMutation(
            request.ProductName,
            request.Description,
            request.Price,
            request.InventoryQuantity,
            visibilityStatus: null);
        if (validationError is not null)
        {
            return Result<ProductDto>.ValidationFailure(validationError);
        }

        var userId = _currentUserProvider.UserId;
        if (userId is null)
            return Result<ProductDto>.Unauthorized("User is not authenticated.");

        var seller = await _sellerRepository.GetByUserIdAsync(userId.Value, cancellationToken);
        if (seller is null)
            return Result<ProductDto>.NotFound("Seller profile not found.");

        var product = CreateProductEntity(request);
        await _productRepository.AddAsync(product, cancellationToken);

        var listing = new ProductListing
        {
            SellerId = seller.Id,
            ProductId = product.Id,
            Sku = product.Id.ToString("N")[..12],
            ListingPrice = request.Price,
            InventoryQuantity = request.InventoryQuantity,
            VisibilityStatus = ListingVisibilityStatus.Draft,
        };

        await _productListingRepository.AddAsync(listing, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ProductDto>.Success(product.ToProductDto());
    }

    public async Task<Result<IReadOnlyList<SellerListingDto>>> GetSellerListingsAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserProvider.UserId;
        if (userId is null)
            return Result<IReadOnlyList<SellerListingDto>>.Unauthorized("User is not authenticated.");

        var seller = await _sellerRepository.GetByUserIdAsync(userId.Value, cancellationToken);
        if (seller is null)
            return Result<IReadOnlyList<SellerListingDto>>.NotFound("Seller profile not found.");

        var listings = await _productListingRepository.GetBySellerIdAsync(seller.Id, 1, int.MaxValue, cancellationToken);
        return Result<IReadOnlyList<SellerListingDto>>.Success(listings.Select(l => l.ToSellerListingDto()).ToArray());
    }

    public async Task<Result<ProductDto>> UpdateAsync(UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var validationError = ValidateProductMutation(
            request.ProductName,
            request.Description,
            request.Price,
            request.InventoryQuantity,
            request.VisibilityStatus);
        if (validationError is not null)
        {
            return Result<ProductDto>.ValidationFailure(validationError);
        }

        var parsedStatus = Enum.Parse<ListingVisibilityStatus>(request.VisibilityStatus, ignoreCase: true);

        var userId = _currentUserProvider.UserId;
        if (userId is null)
            return Result<ProductDto>.Unauthorized("User is not authenticated.");

        var seller = await _sellerRepository.GetByUserIdAsync(userId.Value, cancellationToken);
        if (seller is null)
            return Result<ProductDto>.NotFound("Seller profile not found.");

        var listing = await _productListingRepository.GetByIdAsync(request.ListingId, cancellationToken);
        if (listing is null || listing.IsDeleted || listing.SellerId != seller.Id)
            return Result<ProductDto>.NotFound("Product listing not found or not owned by this seller.");

        var product = await _productRepository.GetByIdAsync(listing.ProductId, cancellationToken);
        if (product is null)
            return Result<ProductDto>.NotFound("Product not found.");

        var activeProductListings = await _productListingRepository.GetByProductIdAsync(product.Id, cancellationToken);
        if (activeProductListings.Any(l => l.Id != listing.Id))
        {
            product = CreateProductEntity(request);
            await _productRepository.AddAsync(product, cancellationToken);
            listing.ProductId = product.Id;
        }
        else
        {
            ApplyProductChanges(product, request);
            await _productRepository.UpdateAsync(product, cancellationToken);
        }

        listing.ListingPrice = request.Price;
        listing.InventoryQuantity = request.InventoryQuantity;
        listing.VisibilityStatus = parsedStatus;

        await _productListingRepository.UpdateAsync(listing, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ProductDto>.Success(product.ToProductDto());
    }

    public async Task<Result> DeleteListingAsync(Guid listingId, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserProvider.UserId;
        if (userId is null)
            return Result.Unauthorized("User is not authenticated.");

        var seller = await _sellerRepository.GetByUserIdAsync(userId.Value, cancellationToken);
        if (seller is null)
            return Result.NotFound("Seller profile not found.");

        var listing = await _productListingRepository.GetByIdAsync(listingId, cancellationToken);
        if (listing is null || listing.IsDeleted || listing.SellerId != seller.Id)
            return Result.NotFound("Product listing not found or not owned by this seller.");

        await _productListingRepository.DeleteAsync(listing, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static Product CreateProductEntity(CreateProductRequest request) =>
        new()
        {
            CategoryId = request.CategoryId,
            ProductName = request.ProductName.Trim(),
            Description = request.Description.Trim(),
            ProductNameLength = request.ProductName.Trim().Length,
            ProductDescriptionLength = request.Description.Trim().Length,
            ProductPhotosQty = request.ProductPhotosQty,
            ProductWeightG = request.ProductWeightG,
            ProductLengthCm = request.ProductLengthCm,
            ProductHeightCm = request.ProductHeightCm,
            ProductWidthCm = request.ProductWidthCm,
        };

    private static Product CreateProductEntity(UpdateProductRequest request) =>
        new()
        {
            CategoryId = request.CategoryId,
            ProductName = request.ProductName.Trim(),
            Description = request.Description.Trim(),
            ProductNameLength = request.ProductName.Trim().Length,
            ProductDescriptionLength = request.Description.Trim().Length,
            ProductPhotosQty = request.ProductPhotosQty,
            ProductWeightG = request.ProductWeightG,
            ProductLengthCm = request.ProductLengthCm,
            ProductHeightCm = request.ProductHeightCm,
            ProductWidthCm = request.ProductWidthCm,
        };

    private static void ApplyProductChanges(Product product, UpdateProductRequest request)
    {
        product.CategoryId = request.CategoryId;
        product.ProductName = request.ProductName.Trim();
        product.Description = request.Description.Trim();
        product.ProductNameLength = product.ProductName.Length;
        product.ProductDescriptionLength = product.Description.Length;
        product.ProductPhotosQty = request.ProductPhotosQty;
        product.ProductWeightG = request.ProductWeightG;
        product.ProductLengthCm = request.ProductLengthCm;
        product.ProductHeightCm = request.ProductHeightCm;
        product.ProductWidthCm = request.ProductWidthCm;
    }

    private static string? ValidateProductMutation(
        string productName,
        string description,
        decimal price,
        int inventoryQuantity,
        string? visibilityStatus)
    {
        if (string.IsNullOrWhiteSpace(productName))
        {
            return "Product name is required.";
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return "Description is required.";
        }

        if (price < 0)
        {
            return "Price must be non-negative.";
        }

        if (inventoryQuantity < 0)
        {
            return "Inventory quantity must be non-negative.";
        }

        if (visibilityStatus is not null &&
            (!Enum.TryParse<ListingVisibilityStatus>(visibilityStatus, ignoreCase: true, out var parsedStatus) ||
             parsedStatus is not (ListingVisibilityStatus.Draft or ListingVisibilityStatus.Published)))
        {
            return "Visibility status must be Draft or Published.";
        }

        return null;
    }
}
