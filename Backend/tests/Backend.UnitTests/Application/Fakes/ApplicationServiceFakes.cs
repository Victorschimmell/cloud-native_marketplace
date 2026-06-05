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
using Backend.Domain.Enums;

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
    public Task<Customer?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult(Customer is not null && Customer.UserId == userId ? Customer : null);
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
    private Product? _product;

    public Product? Product
    {
        get => _product;
        set
        {
            _product = value;
            Products.Clear();
            if (value is not null)
            {
                Products.Add(value);
            }
        }
    }

    public List<Product> Products { get; } = [];
    public int AddCalls { get; private set; }
    public int UpdateCalls { get; private set; }
    public int DeleteCalls { get; private set; }

    public Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        _product = product;
        Products.Add(product);
        AddCalls += 1;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Product product, CancellationToken cancellationToken = default)
    {
        Products.RemoveAll(existing => existing.Id == product.Id);
        DeleteCalls += 1;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Product>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Product>>(Products.Skip((page - 1) * pageSize).Take(pageSize).ToArray());
    public Task<IReadOnlyList<Product>> GetByCategoryIdAsync(Guid categoryId, int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Product>>(Products.Where(product => product.CategoryId == categoryId).Skip((page - 1) * pageSize).Take(pageSize).ToArray());
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Products.FirstOrDefault(product => product.Id == id));

    public Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        var index = Products.FindIndex(existing => existing.Id == product.Id);
        if (index >= 0)
        {
            Products[index] = product;
        }
        else
        {
            Products.Add(product);
        }

        _product = product;
        UpdateCalls += 1;
        return Task.CompletedTask;
    }
}

internal sealed class FakeProductCategoryRepository : IProductCategoryRepository
{
    private ProductCategory? _category;

    public ProductCategory? Category
    {
        get => _category;
        set
        {
            _category = value;
            Categories.Clear();
            if (value is not null)
            {
                Categories.Add(value);
            }
        }
    }

    public List<ProductCategory> Categories { get; } = [];

    public Task AddAsync(ProductCategory category, CancellationToken cancellationToken = default)
    {
        _category = category;
        Categories.Add(category);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ProductCategory category, CancellationToken cancellationToken = default)
    {
        Categories.RemoveAll(existing => existing.Id == category.Id);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ProductCategory>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<ProductCategory>>(Categories.ToArray());
    public Task<ProductCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Categories.FirstOrDefault(category => category.Id == id));

    public Task UpdateAsync(ProductCategory category, CancellationToken cancellationToken = default)
    {
        var index = Categories.FindIndex(existing => existing.Id == category.Id);
        if (index >= 0)
        {
            Categories[index] = category;
        }
        else
        {
            Categories.Add(category);
        }

        _category = category;
        return Task.CompletedTask;
    }
}

internal sealed class FakeProductListingRepository : IProductListingRepository
{
    private ProductListing? _listing;

    public ProductListing? Listing
    {
        get => _listing;
        set
        {
            _listing = value;
            Listings.Clear();
            if (value is not null)
            {
                Listings.Add(value);
            }
        }
    }

    public List<ProductListing> Listings { get; } = [];
    public int AddCalls { get; private set; }
    public int UpdateCalls { get; private set; }
    public int DeleteCalls { get; private set; }

    public Task AddAsync(ProductListing listing, CancellationToken cancellationToken = default)
    {
        _listing = listing;
        Listings.Add(listing);
        AddCalls += 1;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ProductListing listing, CancellationToken cancellationToken = default)
    {
        listing.IsDeleted = true;
        DeleteCalls += 1;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ProductListing>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<ProductListing>>(Listings.Skip((page - 1) * pageSize).Take(pageSize).ToArray());

    public Task<PagedResult<ProductListing>> GetAvailableForBrowseAsync(BrowseProductsRequest request, CancellationToken cancellationToken = default)
    {
        var filtered = Listings.Where(listing => !listing.IsDeleted && listing.VisibilityStatus == ListingVisibilityStatus.Published);
        if (request.CategoryId.HasValue)
        {
            filtered = filtered.Where(listing => listing.Product?.CategoryId == request.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            filtered = filtered.Where(listing => listing.Product?.ProductName.Contains(request.Search, StringComparison.OrdinalIgnoreCase) == true);
        }

        filtered = request.Sort switch
        {
            "price-asc" => filtered.OrderBy(listing => listing.ListingPrice),
            "price-desc" => filtered.OrderByDescending(listing => listing.ListingPrice),
            "name-asc" => filtered.OrderBy(listing => listing.Product?.ProductName),
            _ => filtered.OrderByDescending(listing => listing.CreatedAtUtc)
        };

        var totalCount = filtered.Count();
        var items = filtered.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToArray();
        return Task.FromResult(new PagedResult<ProductListing>(items, request.Page, request.PageSize, totalCount));
    }

    public Task<ProductListing?> GetAvailableProductDetailAsync(Guid productId, Guid? listingId, CancellationToken cancellationToken = default) => Task.FromResult(Listings.FirstOrDefault(listing =>
        !listing.IsDeleted &&
        listing.VisibilityStatus == ListingVisibilityStatus.Published &&
        listing.ProductId == productId &&
        (!listingId.HasValue || listing.Id == listingId.Value)));

    public Task<ProductListing?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Listings.FirstOrDefault(listing => listing.Id == id));
    public Task<IReadOnlyList<ProductListing>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<ProductListing>>(Listings.Where(listing => listing.ProductId == productId && !listing.IsDeleted).ToArray());
    public Task<IReadOnlyList<ProductListing>> GetBySellerIdAsync(Guid sellerId, int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<ProductListing>>(Listings.Where(listing => listing.SellerId == sellerId && !listing.IsDeleted).Skip((page - 1) * pageSize).Take(pageSize).ToArray());

    public Task UpdateAsync(ProductListing listing, CancellationToken cancellationToken = default)
    {
        var index = Listings.FindIndex(existing => existing.Id == listing.Id);
        if (index >= 0)
        {
            Listings[index] = listing;
        }
        else
        {
            Listings.Add(listing);
        }

        _listing = listing;
        UpdateCalls += 1;
        return Task.CompletedTask;
    }
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
    public Task<ShoppingCart?> GetActiveBySessionIdWithProductDetailsAsync(Guid sessionId, CancellationToken cancellationToken = default) => Task.FromResult(Cart);
    public Task<ShoppingCart?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult(Cart);
    public Task<ShoppingCart?> GetActiveByUserIdWithProductDetailsAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult(Cart);
    public Task<ShoppingCart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Cart);
    public Task<ShoppingCart?> GetByIdWithProductDetailsAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Cart);
    public Task UpdateAsync(ShoppingCart cart, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

internal sealed class FakeOrderRepository : IOrderRepository
{
    private Order? _order;

    public Order? Order
    {
        get => _order;
        set
        {
            _order = value;
            Orders.Clear();
            if (value is not null)
            {
                Orders.Add(value);
            }
        }
    }

    public List<Order> Orders { get; } = [];
    public int AddCalls { get; private set; }
    public int UpdateCalls { get; private set; }
    public int DeleteCalls { get; private set; }

    public Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        _order = order;
        Orders.Add(order);
        AddCalls += 1;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Order order, CancellationToken cancellationToken = default)
    {
        Orders.RemoveAll(existing => existing.Id == order.Id);
        DeleteCalls += 1;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Order>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Order>>(Orders.Skip((page - 1) * pageSize).Take(pageSize).ToArray());
    public Task<PagedResult<Order>> GetByCustomerIdAsync(Guid customerId, int page, int pageSize, OrderStatus? status = null, CustomerOrderSort sort = CustomerOrderSort.Newest, CancellationToken cancellationToken = default) => Task.FromResult(new PagedResult<Order>(Orders.Where(order => order.CustomerId == customerId).Skip((page - 1) * pageSize).Take(pageSize).ToArray(), page, pageSize, Orders.Count(order => order.CustomerId == customerId)));
    public Task<PagedResult<Order>> GetByCustomerIdWithDetailsAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult(new PagedResult<Order>(Orders.Where(order => order.CustomerId == customerId).Skip((page - 1) * pageSize).Take(pageSize).ToArray(), page, pageSize, Orders.Count(order => order.CustomerId == customerId)));
    public Task<PagedResult<Order>> GetBySellerIdAsync(Guid sellerId, int page, int pageSize, OrderStatus? status = null, SellerOrderSort sort = SellerOrderSort.Newest, CancellationToken cancellationToken = default) => Task.FromResult(new PagedResult<Order>(Orders.Where(order => order.Items.Any(item => item.SellerId == sellerId)).Skip((page - 1) * pageSize).Take(pageSize).ToArray(), page, pageSize, Orders.Count(order => order.Items.Any(item => item.SellerId == sellerId))));
    public Task<SellerOrderAggregate> GetSellerOrderAggregateAsync(Guid sellerId, CancellationToken cancellationToken = default) => Task.FromResult(new SellerOrderAggregate(0, 0, 0m));
    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Orders.FirstOrDefault(order => order.Id == id));
    public Task<Order?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Orders.FirstOrDefault(order => order.Id == id));
    public Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default) => Task.FromResult(Orders.FirstOrDefault(order => order.OrderNumber == orderNumber));
    public Task<SalesAggregate> GetSalesAggregateAsync(DateTimeOffset? fromUtc, DateTimeOffset? toUtc, CancellationToken cancellationToken = default) => Task.FromResult(new SalesAggregate(0, 0m));
    public Task<OrderStatusAggregate> GetOrderStatusAggregateAsync(DateTimeOffset? fromUtc, DateTimeOffset? toUtc, CancellationToken cancellationToken = default) => Task.FromResult(new OrderStatusAggregate(0, 0, 0));

    public Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        var index = Orders.FindIndex(existing => existing.Id == order.Id);
        if (index >= 0)
        {
            Orders[index] = order;
        }
        else
        {
            Orders.Add(order);
        }

        _order = order;
        UpdateCalls += 1;
        return Task.CompletedTask;
    }
}

internal sealed class FakeAdminDashboardRepository : IAdminDashboardRepository
{
    public AdminDashboardSnapshot Snapshot { get; set; } = new(0, 0, 0m, 0);

    public Task<AdminDashboardSnapshot> GetSnapshotAsync(
        DateTimeOffset ordersFromUtc,
        DateTimeOffset ordersToUtc,
        CancellationToken cancellationToken = default) => Task.FromResult(Snapshot);
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
    public Task<Result<CurrencyDto>> GetCurrencyByCodeAsync(string currencyCode, CancellationToken cancellationToken = default) => Task.FromResult(Result<CurrencyDto>.Success(new CurrencyDto(Guid.NewGuid(), currencyCode, currencyCode, currencyCode)));
    public Task<Result<IReadOnlyList<PaymentDto>>> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default) => Task.FromResult(Result<IReadOnlyList<PaymentDto>>.Success([]));
    public Task<Result<PaymentDto>> RecordCheckoutPaymentAsync(RecordPaymentRequest request, CancellationToken cancellationToken = default) => Task.FromResult(Result<PaymentDto>.Success(ToPaymentDto(request)));
    public Task<Result<PaymentDto>> RecordPaymentAsync(RecordPaymentRequest request, CancellationToken cancellationToken = default) => Task.FromResult(Result<PaymentDto>.Success(ToPaymentDto(request)));

    private static PaymentDto ToPaymentDto(RecordPaymentRequest request) =>
        new(
            request.OrderId,
            1,
            request.PaymentDetails.CurrencyId,
            request.PaymentDetails.PaymentType,
            request.PaymentDetails.PaymentInstallments,
            request.PaymentDetails.PaymentValue,
            PaymentStatus.Pending,
            request.PaymentDetails.ExternalPaymentReference,
            null);
}

internal sealed class FakePaymentRepository : IPaymentRepository
{
    public List<OrderPayment> Payments { get; } = [];
    public int AddCalls { get; private set; }
    public int UpdateCalls { get; private set; }
    public int DeleteCalls { get; private set; }

    public Task AddAsync(OrderPayment payment, CancellationToken cancellationToken = default)
    {
        if (payment.PaymentSequential == 0)
        {
            payment.PaymentSequential = Payments.Count(existing => existing.OrderId == payment.OrderId) + 1;
        }

        Payments.Add(payment);
        AddCalls += 1;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(OrderPayment payment, CancellationToken cancellationToken = default)
    {
        Payments.RemoveAll(existing => existing.OrderId == payment.OrderId && existing.PaymentSequential == payment.PaymentSequential);
        DeleteCalls += 1;
        return Task.CompletedTask;
    }

    public Task<OrderPayment?> GetByIdAsync(Guid orderId, int paymentSequential, CancellationToken cancellationToken = default) => Task.FromResult(Payments.FirstOrDefault(payment => payment.OrderId == orderId && payment.PaymentSequential == paymentSequential));
    public Task<IReadOnlyList<OrderPayment>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<OrderPayment>>(Payments.Where(payment => payment.OrderId == orderId).ToArray());
    public Task<PagedResult<OrderPayment>> GetRecentAsync(AdminPaymentStatusFilter status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var filtered = Payments.AsEnumerable();
        filtered = status switch
        {
            AdminPaymentStatusFilter.Completed => filtered.Where(payment => payment.PaymentStatus is PaymentStatus.Paid or PaymentStatus.Refunded),
            AdminPaymentStatusFilter.Pending => filtered.Where(payment => payment.PaymentStatus is PaymentStatus.Pending or PaymentStatus.Authorized),
            AdminPaymentStatusFilter.Failed => filtered.Where(payment => payment.PaymentStatus is PaymentStatus.Failed or PaymentStatus.Cancelled),
            _ => filtered
        };

        var items = filtered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArray();

        return Task.FromResult(new PagedResult<OrderPayment>(items, page, pageSize, filtered.Count()));
    }

    public Task UpdateAsync(OrderPayment payment, CancellationToken cancellationToken = default)
    {
        var index = Payments.FindIndex(existing => existing.OrderId == payment.OrderId && existing.PaymentSequential == payment.PaymentSequential);
        if (index >= 0)
        {
            Payments[index] = payment;
        }
        else
        {
            Payments.Add(payment);
        }

        UpdateCalls += 1;
        return Task.CompletedTask;
    }
}

internal sealed class FakeCurrencyRepository : ICurrencyRepository
{
    private Currency? _currency;

    public Currency? Currency
    {
        get => _currency;
        set
        {
            _currency = value;
            Currencies.Clear();
            if (value is not null)
            {
                Currencies.Add(value);
            }
        }
    }

    public List<Currency> Currencies { get; } = [];

    public Task<Currency?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Currencies.FirstOrDefault(currency => currency.Id == id));
    public Task<Currency?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) => Task.FromResult(Currencies.FirstOrDefault(currency => string.Equals(currency.Code, code, StringComparison.OrdinalIgnoreCase)));
    public Task<IReadOnlyList<Currency>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Currency>>(Currencies.ToArray());

    public Task AddAsync(Currency currency, CancellationToken cancellationToken = default)
    {
        _currency = currency;
        Currencies.Add(currency);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Currency currency, CancellationToken cancellationToken = default)
    {
        var index = Currencies.FindIndex(existing => existing.Id == currency.Id);
        if (index >= 0)
        {
            Currencies[index] = currency;
        }
        else
        {
            Currencies.Add(currency);
        }

        _currency = currency;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Currency currency, CancellationToken cancellationToken = default)
    {
        Currencies.RemoveAll(existing => existing.Id == currency.Id);
        return Task.CompletedTask;
    }
}

internal sealed class FakeOrderReviewRepository : IOrderReviewRepository
{
    public Task AddAsync(OrderReview review, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task DeleteAsync(OrderReview review, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<bool> ExistsForCustomerProductAsync(Guid customerId, Guid productId, CancellationToken cancellationToken = default) => Task.FromResult(false);
    public Task<bool> ExistsForOrderItemAsync(Guid orderId, int orderItemId, CancellationToken cancellationToken = default) => Task.FromResult(false);
    public Task<OrderReview?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<OrderReview?>(null);
    public Task<IReadOnlyList<OrderReview>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<OrderReview>>([]);
    public Task<IReadOnlyList<OrderReview>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<OrderReview>>([]);
    public Task UpdateAsync(OrderReview review, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

internal sealed class FakeShipmentRepository : IShipmentRepository
{
    public List<Shipment> Shipments { get; } = [];
    public int AddCalls { get; private set; }
    public int UpdateCalls { get; private set; }
    public int DeleteCalls { get; private set; }

    public Task AddAsync(Shipment shipment, CancellationToken cancellationToken = default)
    {
        Shipments.Add(shipment);
        AddCalls += 1;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Shipment shipment, CancellationToken cancellationToken = default)
    {
        Shipments.RemoveAll(existing => existing.Id == shipment.Id);
        DeleteCalls += 1;
        return Task.CompletedTask;
    }

    public Task<Shipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Shipments.FirstOrDefault(shipment => shipment.Id == id));
    public Task<IReadOnlyList<Shipment>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Shipment>>(Shipments.Where(shipment => shipment.OrderId == orderId).ToArray());
    public Task<IReadOnlyList<Shipment>> GetBySellerIdAsync(Guid sellerId, int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Shipment>>(Shipments.Where(shipment => shipment.SellerId == sellerId).Skip((page - 1) * pageSize).Take(pageSize).ToArray());

    public Task UpdateAsync(Shipment shipment, CancellationToken cancellationToken = default)
    {
        var index = Shipments.FindIndex(existing => existing.Id == shipment.Id);
        if (index >= 0)
        {
            Shipments[index] = shipment;
        }
        else
        {
            Shipments.Add(shipment);
        }

        UpdateCalls += 1;
        return Task.CompletedTask;
    }
}

internal sealed class FakeAuditLogService : IAuditLogService
{
    public List<WriteAuditLogEntryRequest> Entries { get; } = [];

    public Task<Result> WriteEntryAsync(WriteAuditLogEntryRequest request, CancellationToken cancellationToken = default)
    {
        Entries.Add(request);
        return Task.FromResult(Result.Success());
    }
}

internal sealed class FakeAdminIssueRepository : IAdminIssueRepository
{
    private AdminIssue? _issue;

    public AdminIssue? Issue
    {
        get => _issue;
        set
        {
            _issue = value;
            Issues.Clear();
            if (value is not null)
            {
                Issues.Add(value);
            }
        }
    }

    public List<AdminIssue> Issues { get; } = [];
    public int AddCalls { get; private set; }
    public int UpdateCalls { get; private set; }

    public Task<AdminIssue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Issues.FirstOrDefault(issue => issue.Id == id));

    public Task<PagedResult<AdminIssue>> GetByFilterAsync(IssueStatus? status, IssuePriority? priority, bool unresolvedOnly, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var filtered = Issues.AsEnumerable();
        if (status.HasValue)
        {
            filtered = filtered.Where(issue => issue.Status == status.Value);
        }

        if (priority.HasValue)
        {
            filtered = filtered.Where(issue => issue.Priority == priority.Value);
        }

        if (unresolvedOnly)
        {
            filtered = filtered.Where(issue => issue.Status != IssueStatus.Resolved);
        }

        var items = filtered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArray();

        return Task.FromResult(new PagedResult<AdminIssue>(items, page, pageSize, filtered.Count()));
    }

    public Task AddAsync(AdminIssue issue, CancellationToken cancellationToken = default)
    {
        Issues.Add(issue);
        _issue = issue;
        AddCalls += 1;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(AdminIssue issue, CancellationToken cancellationToken = default)
    {
        var index = Issues.FindIndex(existing => existing.Id == issue.Id);
        if (index >= 0)
        {
            Issues[index] = issue;
        }
        else
        {
            Issues.Add(issue);
        }

        _issue = issue;
        UpdateCalls += 1;
        return Task.CompletedTask;
    }
}

internal sealed class FakeAuditLogRepository : IAuditLogRepository
{
    public List<AuditLog> AddedLogs { get; } = [];
    public int AddCalls { get; private set; }

    public Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        AddedLogs.Add(auditLog);
        AddCalls += 1;
        return Task.CompletedTask;
    }
    public Task<IReadOnlyList<AuditLog>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<AuditLog>>(AddedLogs.Skip((page - 1) * pageSize).Take(pageSize).ToArray());
    public Task<IReadOnlyList<AuditLog>> GetByActorUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<AuditLog>>(AddedLogs.Where(log => log.ActorUserId == userId).Skip((page - 1) * pageSize).Take(pageSize).ToArray());
    public Task<IReadOnlyList<AuditLog>> GetByTargetEntityAsync(string entityType, string entityId, int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<AuditLog>>(AddedLogs.Where(log => log.TargetEntityType == entityType && log.TargetEntityId == entityId).Skip((page - 1) * pageSize).Take(pageSize).ToArray());
    public Task<PagedResult<AuditLog>> GetByFilterAsync(Guid? userId, string? entityType, string? entityId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var filtered = AddedLogs.AsEnumerable();
        if (userId.HasValue)
        {
            filtered = filtered.Where(log => log.ActorUserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            filtered = filtered.Where(log => log.TargetEntityType == entityType);
        }

        if (!string.IsNullOrWhiteSpace(entityId))
        {
            filtered = filtered.Where(log => log.TargetEntityId == entityId);
        }

        var items = filtered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArray();

        return Task.FromResult(new PagedResult<AuditLog>(items, page, pageSize, filtered.Count()));
    }

    public Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<AuditLog?>(null);
}

internal sealed class FakeSellerVerificationRequestRepository : ISellerVerificationRequestRepository
{
    private SellerVerificationRequest? _request;

    public SellerVerificationRequest? Request
    {
        get => _request;
        set
        {
            _request = value;
            Requests.Clear();
            if (value is not null)
            {
                Requests.Add(value);
            }
        }
    }

    public List<SellerVerificationRequest> Requests { get; } = [];
    public int AddCalls { get; private set; }
    public int UpdateCalls { get; private set; }

    public Task AddAsync(SellerVerificationRequest request, CancellationToken cancellationToken = default)
    {
        _request = request;
        Requests.Add(request);
        AddCalls += 1;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<SellerVerificationRequest>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<SellerVerificationRequest>>(Requests.Skip((page - 1) * pageSize).Take(pageSize).ToArray());
    public Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default) => Task.FromResult(Requests.Count);
    public Task<SellerVerificationRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Requests.FirstOrDefault(request => request.Id == id));
    public Task<IReadOnlyList<SellerVerificationRequest>> GetBySellerIdAsync(Guid sellerId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<SellerVerificationRequest>>(Requests.Where(request => request.SellerId == sellerId).ToArray());

    public Task UpdateAsync(SellerVerificationRequest request, CancellationToken cancellationToken = default)
    {
        var index = Requests.FindIndex(existing => existing.Id == request.Id);
        if (index >= 0)
        {
            Requests[index] = request;
        }
        else
        {
            Requests.Add(request);
        }

        _request = request;
        UpdateCalls += 1;
        return Task.CompletedTask;
    }
}

internal sealed class FakeUserAccountRepository : IUserAccountRepository
{
    private UserAccount? _userAccount;

    public UserAccount? UserAccount
    {
        get => _userAccount;
        set
        {
            _userAccount = value;
            UserAccounts.Clear();
            if (value is not null)
            {
                UserAccounts.Add(value);
            }
        }
    }

    public List<UserAccount> UserAccounts { get; } = [];
    public int AddCalls { get; private set; }
    public int UpdateCalls { get; private set; }

    public Task AddAsync(UserAccount userAccount, CancellationToken cancellationToken = default)
    {
        _userAccount = userAccount;
        UserAccounts.Add(userAccount);
        AddCalls += 1;
        return Task.CompletedTask;
    }
    public Task DeleteAsync(UserAccount userAccount, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<IReadOnlyList<UserAccount>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<UserAccount>>(UserAccounts.Skip((page - 1) * pageSize).Take(pageSize).ToArray());
    public Task<PagedResult<UserAccount>> GetByFilterAsync(AdminUserRoleFilter role, AdminUserStatusFilter status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var filtered = UserAccounts.AsEnumerable();
        filtered = role switch
        {
            AdminUserRoleFilter.Admin => filtered.Where(user => user.IsAdmin),
            AdminUserRoleFilter.Seller => filtered.Where(user => user.SellerProfile is not null),
            AdminUserRoleFilter.Customer => filtered.Where(user => user.CustomerProfile is not null && user.SellerProfile is null && !user.IsAdmin),
            _ => filtered
        };

        filtered = status switch
        {
            AdminUserStatusFilter.Active => filtered.Where(user => !user.IsBlocked && user.AccountStatus != AccountStatus.Suspended && user.SellerProfile?.VerificationStatus != VerificationStatus.Pending),
            AdminUserStatusFilter.PendingVerification => filtered.Where(user => user.SellerProfile?.VerificationStatus == VerificationStatus.Pending),
            AdminUserStatusFilter.Blocked => filtered.Where(user => user.IsBlocked || user.AccountStatus == AccountStatus.Suspended),
            _ => filtered
        };

        var items = filtered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArray();

        return Task.FromResult(new PagedResult<UserAccount>(items, page, pageSize, filtered.Count()));
    }

    public Task<int> CountActiveAsync(CancellationToken cancellationToken = default) => Task.FromResult(UserAccounts.Count(user => !user.IsBlocked));
    public Task<UserAccount?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) => Task.FromResult(UserAccounts.FirstOrDefault(user => string.Equals(user.Email.Value, email, StringComparison.OrdinalIgnoreCase)));
    public Task<UserAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(UserAccounts.FirstOrDefault(user => user.Id == id));
    public Task UpdateAsync(UserAccount userAccount, CancellationToken cancellationToken = default)
    {
        var index = UserAccounts.FindIndex(existing => existing.Id == userAccount.Id);
        if (index >= 0)
        {
            UserAccounts[index] = userAccount;
        }
        else
        {
            UserAccounts.Add(userAccount);
        }

        _userAccount = userAccount;
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
    public bool ForceUnsupported { get; set; }
    public string BaseCurrency => "BRL";
    public string NormalizeOrDefault(string? currency) => string.IsNullOrWhiteSpace(currency) ? BaseCurrency : currency.Trim().ToUpperInvariant();
    public bool IsSupported(string currencyCode) => currencyCode is "BRL" or "USD" or "DKK";
    public decimal FromBaseCurrency(decimal amount, string currencyCode) => currencyCode == "BRL" ? amount : decimal.Round(amount * 0.5m, 2, MidpointRounding.AwayFromZero);
    public bool TryGetPriceFromBaseConverter(string? displayCurrency, out string currencyCode, out Func<decimal, decimal> priceConverter)
    {
        currencyCode = NormalizeOrDefault(displayCurrency);
        var selectedCurrencyCode = currencyCode;
        priceConverter = amount => FromBaseCurrency(amount, selectedCurrencyCode);
        return !ForceUnsupported && IsSupported(currencyCode);
    }
    public decimal ToBaseCurrency(decimal amount, string currencyCode) => currencyCode == "BRL" ? amount : decimal.Round(amount / 0.5m, 2, MidpointRounding.AwayFromZero);
    public bool TryGetPriceToBaseConverter(string? displayCurrency, out string currencyCode, out Func<decimal, decimal> priceConverter)
    {
        currencyCode = NormalizeOrDefault(displayCurrency);
        var selectedCurrencyCode = currencyCode;
        priceConverter = amount => ToBaseCurrency(amount, selectedCurrencyCode);
        return !ForceUnsupported && IsSupported(currencyCode);
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

internal sealed class FakeCheckoutObservability : ICheckoutObservability
{
    public List<(string Operation, string ErrorType, CheckoutObservabilityContext Context)> Failures { get; } = [];

    public long GetTimestamp() => 0;
    public void Started(long startedAt, CheckoutObservabilityContext context) { }
    public void Succeeded(string operation, long startedAt, CheckoutObservabilityContext context) { }
    public void Failed(string operation, string errorType, long startedAt, CheckoutObservabilityContext context) => Failures.Add((operation, errorType, context));
    public void InventoryFailed(long startedAt, CheckoutObservabilityContext context, Guid listingId, int requestedQuantity, int availableQuantity) => Failures.Add(("Checkout.InventoryValidated", "InsufficientInventory", context));
    public void PaymentAmountFailed(long startedAt, CheckoutObservabilityContext context, decimal requestedPaymentAmount, decimal expectedPaymentAmount) => Failures.Add(("Checkout.PaymentValidated", "PaymentAmountMismatch", context));
    public void Unexpected(Exception exception, long startedAt, CheckoutObservabilityContext context) => Failures.Add(("Checkout.Process", "UnexpectedException", context));
}

internal sealed class FakeCurrentUserProvider : ICurrentUserProvider
{
    public Guid? UserId { get; set; } = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public bool IsAuthenticated { get; set; } = true;
    public bool IsAdmin { get; set; } = true;
    public string? IpAddress { get; set; } = "0.0.0.0";
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
