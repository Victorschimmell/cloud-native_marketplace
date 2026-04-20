using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;

namespace Backend.Application.Services;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository )
    {
        ArgumentNullException.ThrowIfNull(productRepository);
        _productRepository = productRepository;
    }

    public Task<Result<ProductDto>> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<PagedResult<ProductDto>>> GetByCategoryAsync(Guid categoryId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<ProductDto>> UpdateAsync(UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeleteAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

