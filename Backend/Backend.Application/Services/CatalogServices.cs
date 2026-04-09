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
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(productRepository);
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductDto>> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        if (productId == Guid.Empty)
        {
            return Result<ProductDto>.Failure("Product id is required.");
        }

        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        return product is null
            ? Result<ProductDto>.Failure("Product was not found.")
            : Result<ProductDto>.Success(product.ToDto());
    }

    public async Task<Result<PagedResult<ProductDto>>> GetByCategoryAsync(Guid categoryId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        if (categoryId == Guid.Empty)
        {
            return Result<PagedResult<ProductDto>>.Failure("Category id is required.");
        }

        if (request.Page <= 0 || request.PageSize <= 0)
        {
            return Result<PagedResult<ProductDto>>.Failure("Page and page size must be greater than zero.");
        }

        var products = await _productRepository.GetByCategoryIdAsync(categoryId, request.Page, request.PageSize, cancellationToken);
        var items = products.Select(static product => product.ToDto()).ToArray();
        return Result<PagedResult<ProductDto>>.Success(new PagedResult<ProductDto>(items, request.Page, request.PageSize, items.Length));
    }

    public async Task<Result<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        if (request.CategoryId == Guid.Empty || string.IsNullOrWhiteSpace(request.ProductName) || string.IsNullOrWhiteSpace(request.Description))
        {
            return Result<ProductDto>.Failure("Category id, product name, and description are required.");
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<ProductDto>.Success(product.ToDto());
    }

    public async Task<Result<ProductDto>> UpdateAsync(UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        if (request.ProductId == Guid.Empty || request.CategoryId == Guid.Empty || string.IsNullOrWhiteSpace(request.ProductName) || string.IsNullOrWhiteSpace(request.Description))
        {
            return Result<ProductDto>.Failure("Product id, category id, product name, and description are required.");
        }

        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result<ProductDto>.Failure("Product was not found.");
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<ProductDto>.Success(product.ToDto());
    }

    public async Task<Result> DeleteAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        if (productId == Guid.Empty)
        {
            return Result.Failure("Product id is required.");
        }

        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        if (product is null)
        {
            return Result.Failure("Product was not found.");
        }

        await _productRepository.DeleteAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class CategoryService : ICategoryService
{
    private readonly IProductCategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IProductCategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(categoryRepository);
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CategoryDto>> GetByIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        if (categoryId == Guid.Empty)
        {
            return Result<CategoryDto>.Failure("Category id is required.");
        }

        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
        return category is null
            ? Result<CategoryDto>.Failure("Category was not found.")
            : Result<CategoryDto>.Success(category.ToDto());
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<CategoryDto>>.Success(categories.Select(static category => category.ToDto()).ToArray());
    }

    public async Task<Result<CategoryDto>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.CategoryNamePt))
        {
            return Result<CategoryDto>.Failure("Category name is required.");
        }

        var category = new ProductCategory
        {
            CategoryNamePt = request.CategoryNamePt.Trim(),
            CategoryNameEn = string.IsNullOrWhiteSpace(request.CategoryNameEn) ? null : request.CategoryNameEn.Trim()
        };

        await _categoryRepository.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<CategoryDto>.Success(category.ToDto());
    }

    public async Task<Result<CategoryDto>> UpdateAsync(UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        if (request.CategoryId == Guid.Empty || string.IsNullOrWhiteSpace(request.CategoryNamePt))
        {
            return Result<CategoryDto>.Failure("Category id and category name are required.");
        }

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
        {
            return Result<CategoryDto>.Failure("Category was not found.");
        }

        category.CategoryNamePt = request.CategoryNamePt.Trim();
        category.CategoryNameEn = string.IsNullOrWhiteSpace(request.CategoryNameEn) ? null : request.CategoryNameEn.Trim();

        await _categoryRepository.UpdateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<CategoryDto>.Success(category.ToDto());
    }

    public async Task<Result> DeleteAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        if (categoryId == Guid.Empty)
        {
            return Result.Failure("Category id is required.");
        }

        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
        if (category is null)
        {
            return Result.Failure("Category was not found.");
        }

        await _categoryRepository.DeleteAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
