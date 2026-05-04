using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Entities.Carts;
using Backend.Domain.Entities.Catalog;
using Backend.Domain.Entities.IdentityAccess;
using Backend.Domain.Entities.Location;
using Backend.Domain.Entities.Operations;
using Backend.Domain.Entities.Orders;

namespace Backend.UnitTests.Application.Fakes;

internal sealed class FakeCustomerRepository : ICustomerRepository
{
    public Customer? Customer { get; private set; }

    public Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        Customer = customer;
        return Task.CompletedTask;
    }
    public Task DeleteAsync(Customer customer, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<PagedResult<Customer>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult(new PagedResult<Customer>([], page, pageSize, 0));
    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Customer?>(null);
    public Task<Customer?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult<Customer?>(null);
    public Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

internal sealed class FakeAddressRepository : IAddressRepository
{
    public Address? Address { get; private set; }

    public Task<Address?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Address is not null && Address.Id == id ? Address : null);

    public Task AddAsync(Address address, CancellationToken cancellationToken = default)
    {
        Address = address;
        return Task.CompletedTask;
    }
}

internal sealed class FakeSellerRepository : ISellerRepository
{
    public Seller? Seller { get; set; }
    public int UpdateCalls { get; private set; }

    public Task AddAsync(Seller seller, CancellationToken cancellationToken = default)
    {
        Seller = seller;
        return Task.CompletedTask;
    }
    public Task DeleteAsync(Seller seller, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<IReadOnlyList<Seller>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Seller>>([]);
    public Task<Seller?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Seller is not null && Seller.Id == id ? Seller : null);
    public Task<Seller?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult(Seller is not null && Seller.UserId == userId ? Seller : null);
    public Task UpdateAsync(Seller seller, CancellationToken cancellationToken = default)
    {
        Seller = seller;
        UpdateCalls += 1;
        return Task.CompletedTask;
    }
}

internal sealed class FakeProductRepository : IProductRepository
{
    public Task AddAsync(Product product, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task DeleteAsync(Product product, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<IReadOnlyList<Product>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Product>>([]);
    public Task<IReadOnlyList<Product>> GetByCategoryIdAsync(Guid categoryId, int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Product>>([]);
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Product?>(null);
    public Task UpdateAsync(Product product, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

internal sealed class FakeProductCategoryRepository : IProductCategoryRepository
{
    public Task AddAsync(ProductCategory category, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task DeleteAsync(ProductCategory category, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<IReadOnlyList<ProductCategory>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<ProductCategory>>([]);
    public Task<ProductCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<ProductCategory?>(null);
    public Task UpdateAsync(ProductCategory category, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

internal sealed class FakeProductListingRepository : IProductListingRepository
{
    public Task AddAsync(ProductListing listing, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task DeleteAsync(ProductListing listing, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<IReadOnlyList<ProductListing>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<ProductListing>>([]);
    public Task<PagedResult<ProductListing>> GetAvailableForBrowseAsync(BrowseProductsRequest request, CancellationToken cancellationToken = default) => Task.FromResult(new PagedResult<ProductListing>([], request.Page, request.PageSize, 0));
    public Task<ProductListing?> GetAvailableProductDetailAsync(Guid productId, Guid? listingId, CancellationToken cancellationToken = default) => Task.FromResult<ProductListing?>(null);
    public Task<ProductListing?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<ProductListing?>(null);
    public Task<IReadOnlyList<ProductListing>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<ProductListing>>([]);
    public Task<IReadOnlyList<ProductListing>> GetBySellerIdAsync(Guid sellerId, int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<ProductListing>>([]);
    public Task UpdateAsync(ProductListing listing, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

internal sealed class FakeCartRepository : ICartRepository
{
    public ShoppingCart? Cart { get; set; }

    public Task AddAsync(ShoppingCart cart, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task AddItemAsync(CartItem item, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task RemoveItemAsync(CartItem item, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task UpdateItemAsync(CartItem item, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task DeleteAsync(ShoppingCart cart, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<ShoppingCart?> GetActiveBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default) => Task.FromResult(Cart);
    public Task<ShoppingCart?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult(Cart);
    public Task<ShoppingCart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Cart);
    public Task UpdateAsync(ShoppingCart cart, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

internal sealed class FakeOrderRepository : IOrderRepository
{
    public Task AddAsync(Order order, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task DeleteAsync(Order order, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<IReadOnlyList<Order>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Order>>([]);
    public Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Order>>([]);
    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Order?>(null);
    public Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default) => Task.FromResult<Order?>(null);
    public Task UpdateAsync(Order order, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

internal sealed class FakeOrderNumberRepository : IOrderNumberGenerator
{
    public Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken = default) => Task.FromResult("ORDER-123456");
}

internal sealed class FakeOrderItemRepository : IOrderItemRepository
{
    public Task AddAsync(OrderItem orderItem, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task DeleteAsync(OrderItem orderItem, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<OrderItem?> GetByIdAsync(Guid orderId, int orderItemId, CancellationToken cancellationToken = default) => Task.FromResult<OrderItem?>(null);
    public Task<IReadOnlyList<OrderItem>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<OrderItem>>([]);
    public Task<IReadOnlyList<OrderItem>> GetBySellerIdAsync(Guid sellerId, int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<OrderItem>>([]);
    public Task UpdateAsync(OrderItem orderItem, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

internal sealed class FakePaymentService : IPaymentService
{
    public Task<Result<CurrencyDto>> GetCurrencyByCodeAsync(string currencyCode, CancellationToken cancellationToken = default) => Task.FromResult(Result<CurrencyDto>.NotImplemented());
    public Task<Result<IReadOnlyList<PaymentDto>>> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default) => Task.FromResult(Result<IReadOnlyList<PaymentDto>>.NotImplemented());
    public Task<Result<PaymentDto>> RecordPaymentAsync(RecordPaymentRequest request, CancellationToken cancellationToken = default) => Task.FromResult(Result<PaymentDto>.NotImplemented());
}

internal sealed class FakePaymentRepository : IPaymentRepository
{
    public Task AddAsync(OrderPayment payment, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task DeleteAsync(OrderPayment payment, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<OrderPayment?> GetByIdAsync(Guid orderId, int paymentSequential, CancellationToken cancellationToken = default) => Task.FromResult<OrderPayment?>(null);
    public Task<IReadOnlyList<OrderPayment>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<OrderPayment>>([]);
    public Task UpdateAsync(OrderPayment payment, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

internal sealed class FakeCurrencyRepository : ICurrencyRepository
{
    public Task<Currency?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Currency?>(null);
    public Task<Currency?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) => Task.FromResult<Currency?>(null);
    public Task<IReadOnlyList<Currency>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Currency>>([]);
    public Task AddAsync(Currency currency, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task UpdateAsync(Currency currency, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task DeleteAsync(Currency currency, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

internal sealed class FakeOrderReviewRepository : IOrderReviewRepository
{
    public Task AddAsync(OrderReview review, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task DeleteAsync(OrderReview review, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<OrderReview?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<OrderReview?>(null);
    public Task<IReadOnlyList<OrderReview>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<OrderReview>>([]);
    public Task<IReadOnlyList<OrderReview>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<OrderReview>>([]);
    public Task UpdateAsync(OrderReview review, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

internal sealed class FakeShipmentRepository : IShipmentRepository
{
    public Task AddAsync(Shipment shipment, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task DeleteAsync(Shipment shipment, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<Shipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Shipment?>(null);
    public Task<IReadOnlyList<Shipment>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Shipment>>([]);
    public Task<IReadOnlyList<Shipment>> GetBySellerIdAsync(Guid sellerId, int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Shipment>>([]);
    public Task UpdateAsync(Shipment shipment, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

internal sealed class FakeAuditLogRepository : IAuditLogRepository
{
    public Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<IReadOnlyList<AuditLog>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<AuditLog>>([]);
    public Task<IReadOnlyList<AuditLog>> GetByActorUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<AuditLog>>([]);
    public Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<AuditLog?>(null);
    public Task<IReadOnlyList<AuditLog>> GetByTargetEntityAsync(string entityType, string entityId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<AuditLog>>([]);
}

internal sealed class FakeSellerVerificationRequestRepository : ISellerVerificationRequestRepository
{
    public SellerVerificationRequest? Request { get; set; }
    public int UpdateCalls { get; private set; }

    public Task AddAsync(SellerVerificationRequest request, CancellationToken cancellationToken = default)
    {
        Request = request;
        return Task.CompletedTask;
    }
    public Task<IReadOnlyList<SellerVerificationRequest>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<SellerVerificationRequest>>([]);
    public Task<SellerVerificationRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Request is not null && Request.Id == id ? Request : null);
    public Task<IReadOnlyList<SellerVerificationRequest>> GetBySellerIdAsync(Guid sellerId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<SellerVerificationRequest>>(Request is not null && Request.SellerId == sellerId ? [Request] : []);
    public Task UpdateAsync(SellerVerificationRequest request, CancellationToken cancellationToken = default)
    {
        Request = request;
        UpdateCalls += 1;
        return Task.CompletedTask;
    }
}

internal sealed class FakeUserAccountRepository : IUserAccountRepository
{
    public UserAccount? UserAccount { get; set; }
    public int AddCalls { get; private set; }
    public int UpdateCalls { get; private set; }

    public Task AddAsync(UserAccount userAccount, CancellationToken cancellationToken = default)
    {
        UserAccount = userAccount;
        AddCalls += 1;
        return Task.CompletedTask;
    }
    public Task DeleteAsync(UserAccount userAccount, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<IReadOnlyList<UserAccount>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<UserAccount>>([]);
    public Task<UserAccount?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) => Task.FromResult(UserAccount is not null && string.Equals(UserAccount.Email.Value, email, StringComparison.OrdinalIgnoreCase) ? UserAccount : null);
    public Task<UserAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(UserAccount is not null && UserAccount.Id == id ? UserAccount : null);
    public Task UpdateAsync(UserAccount userAccount, CancellationToken cancellationToken = default)
    {
        UserAccount = userAccount;
        UpdateCalls += 1;
        return Task.CompletedTask;
    }
}

internal sealed class FakeDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => new(2026, 4, 9, 12, 0, 0, TimeSpan.Zero);
}

internal sealed class FakeCurrencyConversionService : ICurrencyConversionService
{
    public string BaseCurrency => "BRL";
    public string NormalizeOrDefault(string? currency) => string.IsNullOrWhiteSpace(currency) ? BaseCurrency : currency.Trim().ToUpperInvariant();
    public bool IsSupported(string currencyCode) => currencyCode is "BRL" or "USD" or "DKK";
    public decimal FromBaseCurrency(decimal amount, string currencyCode) => currencyCode == "BRL" ? amount : decimal.Round(amount * 0.5m, 2, MidpointRounding.AwayFromZero);
    public bool TryGetPriceConverter(string? displayCurrency, out string currencyCode, out Func<decimal, decimal> priceConverter)
    {
        currencyCode = displayCurrency ?? BaseCurrency;
        priceConverter = amount => FromBaseCurrency(amount, BaseCurrency);
        return true;
    }
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCalls { get; private set; }
    public Exception? ExceptionToThrow { get; set; }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCalls += 1;

        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }

        return Task.CompletedTask;
    }
}

internal sealed class FakeCurrentUserProvider : ICurrentUserProvider
{
    public Guid? UserId { get; set; } = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public bool IsAuthenticated { get; set; } = true;
    public bool IsAdmin { get; set; } = true;
}

internal sealed class FakePasswordHasher : IPasswordHasher
{
    public string HashPassword(string password) => $"hashed::{password}";
    public bool VerifyPassword(UserAccount userAccount, string password) => userAccount.PasswordHash == HashPassword(password);
}

internal sealed class FakeAuthTokenGenerator : IAuthTokenGenerator
{
    public AuthTokenDto CreateToken(UserAccount userAccount) =>
        new($"token-for-{userAccount.Id}", DateTimeOffset.UtcNow.AddHours(1));
}
