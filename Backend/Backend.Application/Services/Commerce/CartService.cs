using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
namespace Backend.Application.Services;

public sealed class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductListingRepository _productListingRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CartService(ICartRepository cartRepository, IProductListingRepository productListingRepository, IDateTimeProvider dateTimeProvider)
    {
        ArgumentNullException.ThrowIfNull(cartRepository);
        ArgumentNullException.ThrowIfNull(productListingRepository);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);

        _cartRepository = cartRepository;
        _productListingRepository = productListingRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public Task<Result<CartDto>> GetCartAsync(GetCartRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<CartDto>> AddItemAsync(AddCartItemRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<CartDto>> RemoveItemAsync(RemoveCartItemRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
