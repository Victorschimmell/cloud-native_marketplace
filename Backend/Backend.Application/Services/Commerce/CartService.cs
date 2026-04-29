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
    private readonly IUnitOfWork _unitOfWork;

    public CartService(
        ICartRepository cartRepository,
        IProductListingRepository productListingRepository,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(cartRepository);
        ArgumentNullException.ThrowIfNull(productListingRepository);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        ArgumentNullException.ThrowIfNull(unitOfWork);

        _cartRepository = cartRepository;
        _productListingRepository = productListingRepository;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public Task<Result<CartDto>> GetCartAsync(GetCartRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<CartDto>.NotImplemented());
    }

    public async Task<Result<CartDto>> AddItemAsync(AddCartItemRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Quantity < 1)
        {
            return Result<CartDto>.ValidationFailure("Quantity must be greater than zero.");
        }

        var listing = await _productListingRepository.GetByIdAsync(request.ListingId, cancellationToken);

        if (listing is null || listing.IsDeleted || listing.VisibilityStatus != ListingVisibilityStatus.Published)
        {
            return Result<CartDto>.NotFound("Product listing was not found.");
        }

        if (listing.InventoryQuantity <= 0)
        {
            return Result<CartDto>.ValidationFailure("Product listing is out of stock.");
        }

        if (request.Quantity > listing.InventoryQuantity)
        {
            return Result<CartDto>.ValidationFailure("Requested quantity exceeds available stock.");
        }

        var cart = await GetOrCreateActiveCartAsync(request, cancellationToken);
        var now = _dateTimeProvider.UtcNow;
        var existingItem = cart.Items.FirstOrDefault(item => item.ListingId == request.ListingId);

        if (existingItem is null)
        {
            var item = new CartItem
            {
                CartId = cart.Id,
                ListingId = request.ListingId,
                Quantity = request.Quantity,
                UnitPriceAtAddition = listing.ListingPrice,
                AddedAtUtc = now,
                UpdatedAtUtc = now
            };

            await _cartRepository.AddItemAsync(item, cancellationToken);
        }
        else
        {
            var newQuantity = existingItem.Quantity + request.Quantity;
            if (newQuantity > listing.InventoryQuantity)
            {
                return Result<CartDto>.ValidationFailure("Requested quantity exceeds available stock.");
            }

            existingItem.Quantity = newQuantity;
            existingItem.UnitPriceAtAddition = listing.ListingPrice;
            existingItem.UpdatedAtUtc = now;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CartDto>.Success(cart.ToCartDto());
    }

    public Task<Result<CartDto>> RemoveItemAsync(RemoveCartItemRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<CartDto>.NotImplemented());
    }

    private async Task<ShoppingCart> GetOrCreateActiveCartAsync(AddCartItemRequest request, CancellationToken cancellationToken)
    {
        ShoppingCart? cart = null;

        if (request.CartId.HasValue)
        {
            cart = await _cartRepository.GetByIdAsync(request.CartId.Value, cancellationToken);
        }

        if (cart is null && request.UserId.HasValue)
        {
            cart = await _cartRepository.GetActiveByUserIdAsync(request.UserId.Value, cancellationToken);
        }

        if (cart is null && request.SessionId.HasValue)
        {
            cart = await _cartRepository.GetActiveBySessionIdAsync(request.SessionId.Value, cancellationToken);
        }

        if (cart is { Status: CartStatus.Active })
        {
            return cart;
        }

        var now = _dateTimeProvider.UtcNow;
        // Temporary anonymous cart for the checkout flow until real auth/session cart ownership is wired up.
        cart = new ShoppingCart
        {
            UserId = request.UserId,
            SessionId = request.SessionId,
            Status = CartStatus.Active,
            ExpiresAtUtc = now.AddDays(14)
        };

        await _cartRepository.AddAsync(cart, cancellationToken);
        return cart;
    }
}
