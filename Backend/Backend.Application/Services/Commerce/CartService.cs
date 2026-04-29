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

        var currencyCode = _currencyConversionService.NormalizeOrDefault(displayCurrency);
        if (!_currencyConversionService.IsSupported(currencyCode))
        {
            return Result<CartDto>.ValidationFailure("Currency must be one of BRL, USD, or DKK.");
        }
        var priceConverter = new Func<decimal, decimal>(price => _currencyConversionService.FromBaseCurrency(price, currencyCode));

        return Result<CartDto>.Success(cart.ToCartDto(currencyCode, priceConverter));
    }

    public async Task<Result<CartDto>> AddItemAsync(AddCartItemRequest request, string displayCurrency, CancellationToken cancellationToken = default)
    {
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

        var cart = await GetOrCreateActiveCartAsync(request.CartId, request.UserId, request.SessionId, cancellationToken);
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
                UpdatedAtUtc = now
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

        var currencyCode = _currencyConversionService.NormalizeOrDefault(displayCurrency);
        if (!_currencyConversionService.IsSupported(currencyCode))
        {
            return Result<CartDto>.ValidationFailure("Currency must be one of BRL, USD, or DKK.");
        }
        var priceConverter = new Func<decimal, decimal>(price => _currencyConversionService.FromBaseCurrency(price, currencyCode));

        return Result<CartDto>.Success(cart.ToCartDto(currencyCode, priceConverter));
    }

    public async Task<Result<CartDto>> RemoveItemAsync(RemoveCartItemRequest request, string displayCurrency, CancellationToken cancellationToken = default)
    {
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

        var cart = await GetOrCreateActiveCartAsync(request.CartId, request.UserId, request.SessionId, cancellationToken);
        var existingCartItem = cart.Items.FirstOrDefault(item => item.ListingId == request.ListingId);
        var now = _dateTimeProvider.UtcNow;

        if (existingCartItem is null)
        {
            return Result<CartDto>.NotFound("Cart item was not found in the cart.");
        }

        if (request.Quantity >= existingCartItem.Quantity)
        {
            await _cartRepository.RemoveItemAsync(existingCartItem, cancellationToken);
        }
        else
        {
            existingCartItem.Quantity -= request.Quantity;
            existingCartItem.UpdatedAtUtc = now;

            await _cartRepository.UpdateItemAsync(existingCartItem, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var currencyCode = _currencyConversionService.NormalizeOrDefault(displayCurrency);
        if (!_currencyConversionService.IsSupported(currencyCode))
        {
            return Result<CartDto>.ValidationFailure("Currency must be one of BRL, USD, or DKK.");
        }
        var priceConverter = new Func<decimal, decimal>(price => _currencyConversionService.FromBaseCurrency(price, currencyCode));

        return Result<CartDto>.Success(cart.ToCartDto(currencyCode, priceConverter));
    }

    private async Task<ShoppingCart?> GetCartAsync(Guid? CartId, Guid? UserId, Guid? SessionId, CancellationToken cancellationToken)
    {
        ShoppingCart? cart = null;

        if (CartId.HasValue)
        {
            cart = await _cartRepository.GetByIdAsync(CartId.Value, cancellationToken);
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

    private async Task<ShoppingCart> GetOrCreateActiveCartAsync(Guid? CartId, Guid? UserId, Guid? SessionId, CancellationToken cancellationToken)
    {
        ShoppingCart? cart = await GetActiveCartAsync(CartId, UserId, SessionId, cancellationToken);

        if (cart is not null)
        {
            return cart;
        }

        var now = _dateTimeProvider.UtcNow;
        // Temporary anonymous cart for the checkout flow until real auth/session cart ownership is wired up.
        // TODO: Remember to validate the userid and sessionid is valid
        cart = new ShoppingCart
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
