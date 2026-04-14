using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Entities.Carts;
using Backend.Domain.Enums;

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

    public async Task<Result<CartDto>> GetCartAsync(GetCartRequest request, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            var cart = await ResolveCartAsync(request.CartId, request.UserId, request.SessionId, cancellationToken);
            return cart is null
                ? Result<CartDto>.NotFound("Cart was not found.")
                : Result<CartDto>.Success(cart.ToDto());
        }, "Unable to get cart.");
    }

    public async Task<Result<CartDto>> AddItemAsync(AddCartItemRequest request, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (request.ListingId == Guid.Empty || request.Quantity <= 0)
            {
                return Result<CartDto>.ValidationFailure("Listing id is required and quantity must be greater than zero.");
            }

            var listing = await _productListingRepository.GetByIdAsync(request.ListingId, cancellationToken);
            if (listing is null)
            {
                return Result<CartDto>.NotFound("Listing was not found.");
            }

            var cart = await ResolveCartAsync(request.CartId, request.UserId, request.SessionId, cancellationToken)
                ?? new ShoppingCart
                {
                    UserId = request.UserId,
                    SessionId = request.SessionId,
                    Status = CartStatus.Active,
                    ExpiresAtUtc = _dateTimeProvider.UtcNow.AddDays(7)
                };

            var existingItem = cart.Items.FirstOrDefault(item => item.ListingId == request.ListingId);
            if (existingItem is null)
            {
                cart.Items.Add(new CartItem
                {
                    CartId = cart.Id,
                    ListingId = listing.Id,
                    Quantity = request.Quantity,
                    UnitPriceAtAddition = listing.ListingPrice,
                    AddedAtUtc = _dateTimeProvider.UtcNow,
                    UpdatedAtUtc = _dateTimeProvider.UtcNow
                });
            }
            else
            {
                existingItem.Quantity += request.Quantity;
                existingItem.UpdatedAtUtc = _dateTimeProvider.UtcNow;
            }

            if (await _cartRepository.GetByIdAsync(cart.Id, cancellationToken) is null)
            {
                await _cartRepository.AddAsync(cart, cancellationToken);
            }
            else
            {
                await _cartRepository.UpdateAsync(cart, cancellationToken);
            }
            return Result<CartDto>.Success(cart.ToDto());
        }, "Unable to add item to cart.");
    }

    public async Task<Result<CartDto>> RemoveItemAsync(RemoveCartItemRequest request, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (request.ListingId == Guid.Empty)
            {
                return Result<CartDto>.ValidationFailure("Listing id is required.");
            }

            var cart = await ResolveCartAsync(request.CartId, request.UserId, request.SessionId, cancellationToken);
            if (cart is null)
            {
                return Result<CartDto>.NotFound("Cart was not found.");
            }

            var item = cart.Items.FirstOrDefault(cartItem => cartItem.ListingId == request.ListingId);
            if (item is null)
            {
                return Result<CartDto>.NotFound("Cart item was not found.");
            }

            cart.Items.Remove(item);
            await _cartRepository.UpdateAsync(cart, cancellationToken);
            return Result<CartDto>.Success(cart.ToDto());
        }, "Unable to remove item from cart.");
    }

    private async Task<ShoppingCart?> ResolveCartAsync(Guid? cartId, Guid? userId, Guid? sessionId, CancellationToken cancellationToken)
    {
        ShoppingCart? cart = null;

        if (cartId.HasValue && cartId.Value != Guid.Empty)
        {
            cart = await _cartRepository.GetByIdAsync(cartId.Value, cancellationToken);
        }
        else if (userId.HasValue && userId.Value != Guid.Empty)
        {
            cart = await _cartRepository.GetActiveByUserIdAsync(userId.Value, cancellationToken);
        }
        else if (sessionId.HasValue && sessionId.Value != Guid.Empty)
        {
            cart = await _cartRepository.GetActiveBySessionIdAsync(sessionId.Value, cancellationToken);
        }

        return cart is not null && cart.ExpiresAtUtc <= _dateTimeProvider.UtcNow
            ? null
            : cart;
    }
}
