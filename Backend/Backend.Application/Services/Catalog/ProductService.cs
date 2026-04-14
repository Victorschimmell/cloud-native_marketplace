using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Entities.Catalog;

namespace Backend.Application.Services;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository )
    {
        ArgumentNullException.ThrowIfNull(productRepository);
        _productRepository = productRepository;
    }

    public async Task<Result<ProductDto>> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        if (productId == Guid.Empty)
        {
            return Result<ProductDto>.ValidationFailure("Product id is required.");
        }

        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        return product is null
            ? Result<ProductDto>.NotFound("Product was not found.")
            : Result<ProductDto>.Success(product.ToDto());
    }

    public async Task<Result<PagedResult<ProductDto>>> GetByCategoryAsync(Guid categoryId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        if (categoryId == Guid.Empty)
        {
            return Result<PagedResult<ProductDto>>.ValidationFailure("Category id is required.");
        }

        if (request.Page <= 0 || request.PageSize <= 0)
        {
            return Result<PagedResult<ProductDto>>.ValidationFailure("Page and page size must be greater than zero.");
        }

        var products = await _productRepository.GetByCategoryIdAsync(categoryId, request.Page, request.PageSize, cancellationToken);
        var items = products.Select(static product => product.ToDto()).ToArray();
        return Result<PagedResult<ProductDto>>.Success(new PagedResult<ProductDto>(items, request.Page, request.PageSize, items.Length));
    }

    public async Task<Result<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (request.CategoryId == Guid.Empty || string.IsNullOrWhiteSpace(request.ProductName) || string.IsNullOrWhiteSpace(request.Description))
            {
                return Result<ProductDto>.ValidationFailure("Category id, product name, and description are required.");
            }

            var product = new Product
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
                ProductWidthCm = request.ProductWidthCm
            };

            await _productRepository.AddAsync(product, cancellationToken);
            return Result<ProductDto>.Success(product.ToDto());
        }, "Unable to create product.");
    }

    public async Task<Result<ProductDto>> UpdateAsync(UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (request.ProductId == Guid.Empty || request.CategoryId == Guid.Empty || string.IsNullOrWhiteSpace(request.ProductName) || string.IsNullOrWhiteSpace(request.Description))
            {
                return Result<ProductDto>.ValidationFailure("Product id, category id, product name, and description are required.");
            }

            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product is null)
            {
                return Result<ProductDto>.NotFound("Product was not found.");
            }

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

            await _productRepository.UpdateAsync(product, cancellationToken);
            return Result<ProductDto>.Success(product.ToDto());
        }, "Unable to update product.");
    }

    public async Task<Result> DeleteAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (productId == Guid.Empty)
            {
                return Result.ValidationFailure("Product id is required.");
            }

            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            if (product is null)
            {
                return Result.NotFound("Product was not found.");
            }

            await _productRepository.DeleteAsync(product, cancellationToken);
            return Result.Success();
        }, "Unable to delete product.");
    }
}

