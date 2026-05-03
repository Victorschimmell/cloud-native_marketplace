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
    private readonly ICurrencyConversionService _currencyConversionService;
    private readonly IUnitOfWork _unitOfWork;

    public CartService(
        ICartRepository cartRepository,
        IProductListingRepository productListingRepository,
        IDateTimeProvider dateTimeProvider,
        ICurrencyConversionService currencyConversionService,
        IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(cartRepository);
        ArgumentNullException.ThrowIfNull(productListingRepository);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        ArgumentNullException.ThrowIfNull(currencyConversionService);
        ArgumentNullException.ThrowIfNull(unitOfWork);

        _cartRepository = cartRepository;
        _productListingRepository = productListingRepository;
        _dateTimeProvider = dateTimeProvider;
        _currencyConversionService = currencyConversionService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CartDto>> GetCartAsync(GetCartRequest request, string displayCurrency, CancellationToken cancellationToken = default)
    {
        var cart = await GetCartAsync(request.CartId, request.UserId, request.SessionId, cancellationToken);

        if (cart is null)
        {
            return Result<CartDto>.NotFound("Cart was not found for the provided identifiers.");
        }

        if (!_currencyConversionService.TryGetPriceConverter(displayCurrency, out var currencyCode, out var priceConverter))
        {
            return Result<CartDto>.ValidationFailure("Currency must be one of BRL, USD, or DKK.");
        }

        return Result<CartDto>.Success(cart.ToCartDto(currencyCode, priceConverter));
    }

    public async Task<Result<CartDto>> AddItemAsync(AddCartItemRequest request, string displayCurrency, CancellationToken cancellationToken = default)
    {
        if (!_currencyConversionService.TryGetPriceConverter(displayCurrency, out var currencyCode, out var priceConverter))
        {
            return Result<CartDto>.ValidationFailure("Currency must be one of BRL, USD, or DKK.");
        }

        if (!request.CartId.HasValue && !request.UserId.HasValue && !request.SessionId.HasValue)
        {
            return Result<CartDto>.ValidationFailure("At least one of CartId, UserId, or SessionId must be provided.");
        }

        if (request.Quantity <= 0)
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

        var cart = await GetActiveCartAsync(request.CartId, request.UserId, request.SessionId, cancellationToken);
        if (cart is null)
        {
            if (request.CartId.HasValue)
            {
                return Result<CartDto>.NotFound("Cart was not found for the provided identifiers.");
            }

            cart = await CreateActiveCartAsync(request.UserId, request.SessionId, cancellationToken);
        }

        var existingCartItem = cart.Items.FirstOrDefault(item => item.ListingId == request.ListingId);
        var now = _dateTimeProvider.UtcNow;

        if (existingCartItem is null)
        {
            var item = new CartItem
            {
                CartId = cart.Id,
                ListingId = request.ListingId,
                Quantity = request.Quantity,
                UnitPriceAtAddition = listing.ListingPrice,
                AddedAtUtc = now,
                UpdatedAtUtc = now,
                Listing = listing
            };

            await _cartRepository.AddItemAsync(item, cancellationToken);
        }
        else
        {
            var newQuantity = existingCartItem.Quantity + request.Quantity;
            if (newQuantity > listing.InventoryQuantity)
            {
                return Result<CartDto>.ValidationFailure("Requested quantity exceeds available stock.");
            }

            existingCartItem.Quantity = newQuantity;
            existingCartItem.UpdatedAtUtc = now;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CartDto>.Success(cart.ToCartDto(currencyCode, priceConverter));
    }

    public async Task<Result<CartDto>> UpdateItemAsync(UpdateCartItemRequest request, string displayCurrency, CancellationToken cancellationToken = default)
    {
        if (!_currencyConversionService.TryGetPriceConverter(displayCurrency, out var currencyCode, out var priceConverter))
        {
            return Result<CartDto>.ValidationFailure("Currency must be one of BRL, USD, or DKK.");
        }

        if (!request.CartId.HasValue && !request.UserId.HasValue && !request.SessionId.HasValue)
        {
            return Result<CartDto>.ValidationFailure("At least one of CartId, UserId, or SessionId must be provided.");
        }

        var listing = await _productListingRepository.GetByIdAsync(request.ListingId, cancellationToken);

        if (listing is null || listing.IsDeleted || listing.VisibilityStatus != ListingVisibilityStatus.Published)
        {
            return Result<CartDto>.NotFound("Product listing was not found.");
        }

        var cart = await GetActiveCartAsync(request.CartId, request.UserId, request.SessionId, cancellationToken);
        if (cart is null)
        {
            return Result<CartDto>.NotFound("Cart was not found for the provided identifiers.");
        }

        var existingCartItem = cart.Items.FirstOrDefault(item => item.ListingId == request.ListingId);
        var now = _dateTimeProvider.UtcNow;

        if (existingCartItem is null)
        {
            return Result<CartDto>.NotFound("Cart item was not found in the cart.");
        }

        if (request.Quantity > listing.InventoryQuantity)
        {
            return Result<CartDto>.ValidationFailure("Requested quantity exceeds available stock.");
        }

        if (request.Quantity <= 0)
        {
            await _cartRepository.RemoveItemAsync(existingCartItem, cancellationToken);
        }
        else
        {
            existingCartItem.Quantity = request.Quantity;
            existingCartItem.UpdatedAtUtc = now;

            await _cartRepository.UpdateItemAsync(existingCartItem, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CartDto>.Success(cart.ToCartDto(currencyCode, priceConverter));
    }

    private async Task<ShoppingCart?> GetCartAsync(Guid? CartId, Guid? UserId, Guid? SessionId, CancellationToken cancellationToken)
    {
        ShoppingCart? cart = null;

        if (CartId.HasValue)
        {
            cart = await _cartRepository.GetByIdAsync(CartId.Value, cancellationToken);
            if (cart is not null && !CartAccessPolicy.CanAccess(cart, UserId, SessionId))
            {
                return null;
            }
        }

        if (cart is null && UserId.HasValue)
        {
            cart = await _cartRepository.GetActiveByUserIdAsync(UserId.Value, cancellationToken);
        }

        if (cart is null && SessionId.HasValue)
        {
            cart = await _cartRepository.GetActiveBySessionIdAsync(SessionId.Value, cancellationToken);
        }

        return cart;
    }

    private async Task<ShoppingCart?> GetActiveCartAsync(Guid? CartId, Guid? UserId, Guid? SessionId, CancellationToken cancellationToken)
    {
        ShoppingCart? cart = await GetCartAsync(CartId, UserId, SessionId, cancellationToken);

        if (cart is { Status: CartStatus.Active })
        {
            // cart expired
            if (cart.ExpiresAtUtc < _dateTimeProvider.UtcNow)
            {
                cart.Status = CartStatus.Expired;
                await _cartRepository.UpdateAsync(cart, cancellationToken);
                return null;
            }

            var now = _dateTimeProvider.UtcNow;
            cart.ExpiresAtUtc = now.AddDays(14);
            await _cartRepository.UpdateAsync(cart, cancellationToken);
            return cart;
        }

        return null;
    }

    private async Task<ShoppingCart> CreateActiveCartAsync(Guid? UserId, Guid? SessionId, CancellationToken cancellationToken)
    {
        var now = _dateTimeProvider.UtcNow;
        var cart = new ShoppingCart
        {
            UserId = UserId,
            SessionId = SessionId,
            Status = CartStatus.Active,
            ExpiresAtUtc = now.AddDays(14)
        };
        await _cartRepository.AddAsync(cart, cancellationToken);

        return cart;
    }
}
