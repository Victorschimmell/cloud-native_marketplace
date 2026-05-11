using Backend.Domain.Entities.Catalog;
using Backend.Domain.Entities.IdentityAccess;
using Backend.Domain.Entities.Location;
using Backend.Domain.Entities.Orders;
using Backend.Domain.Enums;
using Backend.Infrastructure.Persistence.Import;
using Backend.Infrastructure.Persistence.Import.Abstractions;
using Backend.Infrastructure.Persistence.Import.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Backend.Infrastructure.Persistence.Seeding;

public sealed class OlistDataSeeder : IOlistDataSeeder
{
    private const string ImportedPasswordHash = "olist-import";
    private const string FallbackCategoryNamePt = "olist_sem_categoria";
    private const string FallbackCategoryNameEn = "Olist uncategorized";
    private readonly ApplicationDbContext _dbContext;
    private readonly ICsvDatasetReader _csvDatasetReader;
    private readonly OlistSeedOptions _options;
    private readonly ILogger<OlistDataSeeder> _logger;

    public OlistDataSeeder(
        ApplicationDbContext dbContext,
        ICsvDatasetReader csvDatasetReader,
        IOptions<OlistSeedOptions> options,
        ILogger<OlistDataSeeder> logger)
    {
        _dbContext = dbContext;
        _csvDatasetReader = csvDatasetReader;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Olist import is disabled.");
            return;
        }

        var datasetRoot = ResolveDatasetRootPath();
        var fileSet = CreateFileSet(datasetRoot);
        ValidateRequiredFiles(fileSet);

        _logger.LogInformation("Starting Olist dataset import from '{DatasetRoot}'.", datasetRoot);

        var sourceData = await LoadSourceDataAsync(fileSet, cancellationToken);

        var currencyId = await EnsureCurrencyAsync(cancellationToken);
        await SeedCategoriesAsync(sourceData.CategoryTranslations, cancellationToken);
        await SeedCustomersAsync(sourceData.Customers, cancellationToken);
        await SeedSellersAsync(sourceData.Sellers, cancellationToken);
        await SeedProductsAsync(sourceData.Products, sourceData.CategoryTranslations, cancellationToken);
        await SeedListingsAsync(sourceData.Orders, sourceData.OrderItems, cancellationToken);
        await SeedOrdersAsync(sourceData.Orders, sourceData.OrderItems, cancellationToken);
        await SeedOrderItemsAsync(sourceData.OrderItems, cancellationToken);
        await SeedPaymentsAsync(sourceData.Payments, currencyId, cancellationToken);
        await SeedReviewsAsync(sourceData.Reviews, cancellationToken);

        _logger.LogInformation("Olist dataset import completed successfully.");
    }

    private string ResolveDatasetRootPath()
    {
        if (string.IsNullOrWhiteSpace(_options.DatasetRootPath))
        {
            throw new InvalidOperationException(
                $"Olist dataset root path is not configured. Set '{OlistSeedOptions.SectionName}:DatasetRootPath' before enabling import.");
        }

        return Path.GetFullPath(_options.DatasetRootPath);
    }

    private OlistFileSet CreateFileSet(string datasetRootPath) =>
        new(
            Path.Combine(datasetRootPath, _options.ProductCategoryTranslationFileName),
            Path.Combine(datasetRootPath, _options.CustomersFileName),
            Path.Combine(datasetRootPath, _options.SellersFileName),
            Path.Combine(datasetRootPath, _options.ProductsFileName),
            Path.Combine(datasetRootPath, _options.OrdersFileName),
            Path.Combine(datasetRootPath, _options.OrderItemsFileName),
            Path.Combine(datasetRootPath, _options.OrderPaymentsFileName),
            Path.Combine(datasetRootPath, _options.OrderReviewsFileName));

    private static void ValidateRequiredFiles(OlistFileSet fileSet)
    {
        var missingFiles = fileSet.Paths
            .Where(path => !File.Exists(path))
            .ToArray();

        if (missingFiles.Length > 0)
        {
            throw new InvalidOperationException(
                $"Olist import could not start because required files are missing:{Environment.NewLine}{string.Join(Environment.NewLine, missingFiles)}");
        }
    }

    private async Task<OlistSourceData> LoadSourceDataAsync(OlistFileSet fileSet, CancellationToken cancellationToken)
    {
        var categoryTranslations = await _csvDatasetReader.ReadAsync(
            fileSet.ProductCategoryTranslationsPath,
            record => new ProductCategoryTranslationRow(
                record.GetRequiredString("product_category_name"),
                record.GetRequiredString("product_category_name_english")),
            cancellationToken);

        var customers = await _csvDatasetReader.ReadAsync(
            fileSet.CustomersPath,
            record => new CustomerRow(
                record.GetRequiredString("customer_id"),
                record.GetRequiredString("customer_unique_id"),
                record.GetRequiredString("customer_zip_code_prefix"),
                record.GetRequiredString("customer_city"),
                record.GetRequiredString("customer_state")),
            cancellationToken);

        var sellers = await _csvDatasetReader.ReadAsync(
            fileSet.SellersPath,
            record => new SellerRow(
                record.GetRequiredString("seller_id"),
                record.GetRequiredString("seller_zip_code_prefix"),
                record.GetRequiredString("seller_city"),
                record.GetRequiredString("seller_state")),
            cancellationToken);

        var products = await _csvDatasetReader.ReadAsync(
            fileSet.ProductsPath,
            record => new ProductRow(
                record.GetRequiredString("product_id"),
                record.GetOptionalString("product_category_name"),
                record.GetOptionalInt32("product_name_lenght"),
                record.GetOptionalInt32("product_description_lenght"),
                record.GetOptionalInt32("product_photos_qty"),
                record.GetOptionalInt32("product_weight_g"),
                record.GetOptionalInt32("product_length_cm"),
                record.GetOptionalInt32("product_height_cm"),
                record.GetOptionalInt32("product_width_cm")),
            cancellationToken);

        var orders = await _csvDatasetReader.ReadAsync(
            fileSet.OrdersPath,
            record => new OrderRow(
                record.GetRequiredString("order_id"),
                record.GetRequiredString("customer_id"),
                record.GetRequiredString("order_status"),
                record.GetOptionalDateTimeOffset("order_purchase_timestamp")
                    ?? throw new InvalidOperationException("Order purchase timestamp is required."),
                record.GetOptionalDateTimeOffset("order_approved_at"),
                record.GetOptionalDateTimeOffset("order_delivered_carrier_date"),
                record.GetOptionalDateTimeOffset("order_delivered_customer_date"),
                record.GetOptionalDateTimeOffset("order_estimated_delivery_date")),
            cancellationToken);

        var orderItems = await _csvDatasetReader.ReadAsync(
            fileSet.OrderItemsPath,
            record => new OrderItemRow(
                record.GetRequiredString("order_id"),
                record.GetRequiredInt32("order_item_id"),
                record.GetRequiredString("product_id"),
                record.GetRequiredString("seller_id"),
                record.GetOptionalDateTimeOffset("shipping_limit_date"),
                record.GetRequiredDecimal("price"),
                record.GetRequiredDecimal("freight_value")),
            cancellationToken);

        var payments = await _csvDatasetReader.ReadAsync(
            fileSet.OrderPaymentsPath,
            record => new OrderPaymentRow(
                record.GetRequiredString("order_id"),
                record.GetRequiredInt32("payment_sequential"),
                record.GetRequiredString("payment_type"),
                record.GetRequiredInt32("payment_installments"),
                record.GetRequiredDecimal("payment_value")),
            cancellationToken);

        var reviews = await _csvDatasetReader.ReadAsync(
            fileSet.OrderReviewsPath,
            record => new OrderReviewRow(
                record.GetRequiredString("review_id"),
                record.GetRequiredString("order_id"),
                record.GetRequiredInt32("review_score"),
                record.GetOptionalString("review_comment_title"),
                record.GetOptionalString("review_comment_message"),
                record.GetOptionalDateTimeOffset("review_creation_date")
                    ?? throw new InvalidOperationException("Review creation date is required."),
                record.GetOptionalDateTimeOffset("review_answer_timestamp")),
            cancellationToken);

        return new OlistSourceData(categoryTranslations, customers, sellers, products, orders, orderItems, payments, reviews);
    }

    private async Task<Guid> EnsureCurrencyAsync(CancellationToken cancellationToken)
    {
        var existing = await _dbContext.Currencies
            .AsNoTracking()
            .SingleOrDefaultAsync(currency => currency.Code == "BRL", cancellationToken);

        if (existing is not null)
        {
            return existing.Id;
        }

        var currency = new Currency
        {
            Code = "BRL",
            Name = "Brazilian Real",
            Symbol = "R$"
        };

        _dbContext.Currencies.Add(currency);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Inserted 1 currency row for BRL.");
        return currency.Id;
    }

    private async Task SeedCategoriesAsync(
        IReadOnlyList<ProductCategoryTranslationRow> categoryTranslations,
        CancellationToken cancellationToken)
    {
        var existingNames = await _dbContext.ProductCategories
            .AsNoTracking()
            .Select(category => category.CategoryNamePt)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, cancellationToken);

        var categoriesToInsert = categoryTranslations
            .Where(row => !existingNames.Contains(row.CategoryNamePt))
            .Select(row => new ProductCategory
            {
                CategoryNamePt = row.CategoryNamePt,
                CategoryNameEn = row.CategoryNameEn
            })
            .ToList();

        if (categoriesToInsert.Count == 0)
        {
            _logger.LogInformation("No product categories needed importing.");
            return;
        }

        _dbContext.ProductCategories.AddRange(categoriesToInsert);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Inserted {InsertedCount} product categories.", categoriesToInsert.Count);
    }

    private async Task SeedCustomersAsync(IReadOnlyList<CustomerRow> customers, CancellationToken cancellationToken)
    {
        var existingCustomerIds = await _dbContext.Customers
            .AsNoTracking()
            .Where(customer => customer.OlistCustomerId != null)
            .Select(customer => customer.OlistCustomerId!)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, cancellationToken);

        var addressesToInsert = new List<Address>();
        var userAccountsToInsert = new List<UserAccount>();
        var customersToInsert = new List<Customer>();

        foreach (var row in customers)
        {
            if (existingCustomerIds.Contains(row.CustomerId))
            {
                continue;
            }

            var address = new Address
            {
                PostalCode = row.ZipCodePrefix,
                City = row.City,
                State = row.State,
                CountryCode = "BR"
            };

            var userAccount = new UserAccount
            {
                Email = OlistImportValueMapper.CreateCustomerEmail(row.CustomerId, row.CustomerUniqueId),
                PasswordHash = ImportedPasswordHash,
                AccountStatus = AccountStatus.Active
            };

            var customer = new Customer
            {
                UserId = userAccount.Id,
                FirstName = "Imported",
                LastName = OlistImportValueMapper.CreateCustomerLastName(row.CustomerUniqueId),
                Phone = "+550000000000",
                DefaultAddressId = address.Id,
                OlistCustomerId = row.CustomerId,
                OlistCustomerUniqueId = row.CustomerUniqueId
            };

            addressesToInsert.Add(address);
            userAccountsToInsert.Add(userAccount);
            customersToInsert.Add(customer);
        }

        if (customersToInsert.Count == 0)
        {
            _logger.LogInformation("No customers needed importing.");
            return;
        }

        _dbContext.Addresses.AddRange(addressesToInsert);
        _dbContext.UserAccounts.AddRange(userAccountsToInsert);
        _dbContext.Customers.AddRange(customersToInsert);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Inserted {InsertedCount} customers.", customersToInsert.Count);
    }

    private async Task SeedSellersAsync(IReadOnlyList<SellerRow> sellers, CancellationToken cancellationToken)
    {
        var existingSellerIds = await _dbContext.Sellers
            .AsNoTracking()
            .Where(seller => seller.OlistSellerId != null)
            .Select(seller => seller.OlistSellerId!)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, cancellationToken);

        var addressesToInsert = new List<Address>();
        var userAccountsToInsert = new List<UserAccount>();
        var sellersToInsert = new List<Seller>();

        foreach (var row in sellers)
        {
            if (existingSellerIds.Contains(row.SellerId))
            {
                continue;
            }

            var address = new Address
            {
                PostalCode = row.ZipCodePrefix,
                City = row.City,
                State = row.State,
                CountryCode = "BR"
            };

            var userAccount = new UserAccount
            {
                Email = OlistImportValueMapper.CreateSellerEmail(row.SellerId),
                PasswordHash = ImportedPasswordHash,
                AccountStatus = AccountStatus.Active
            };

            var seller = new Seller
            {
                UserId = userAccount.Id,
                BusinessName = OlistImportValueMapper.CreateSellerBusinessName(row.SellerId),
                RegistrationNumber = OlistImportValueMapper.CreateRegistrationNumber(row.SellerId),
                PayoutInformation = $"olist-import:{row.SellerId.ToLowerInvariant()}",
                DefaultAddressId = address.Id,
                VerificationStatus = VerificationStatus.Verified,
                VerifiedAtUtc = DateTimeOffset.UtcNow,
                OlistSellerId = row.SellerId
            };

            addressesToInsert.Add(address);
            userAccountsToInsert.Add(userAccount);
            sellersToInsert.Add(seller);
        }

        if (sellersToInsert.Count == 0)
        {
            _logger.LogInformation("No sellers needed importing.");
            return;
        }

        _dbContext.Addresses.AddRange(addressesToInsert);
        _dbContext.UserAccounts.AddRange(userAccountsToInsert);
        _dbContext.Sellers.AddRange(sellersToInsert);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Inserted {InsertedCount} sellers.", sellersToInsert.Count);
    }

    private async Task SeedProductsAsync(
        IReadOnlyList<ProductRow> products,
        IReadOnlyList<ProductCategoryTranslationRow> categoryTranslations,
        CancellationToken cancellationToken)
    {
        var existingProductIds = await _dbContext.Products
            .AsNoTracking()
            .Where(product => product.OlistProductId != null)
            .Select(product => product.OlistProductId!)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, cancellationToken);

        var categoryLookup = await _dbContext.ProductCategories
            .AsNoTracking()
            .ToDictionaryAsync(category => category.CategoryNamePt, category => category, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var translations = categoryTranslations
            .ToDictionary(row => row.CategoryNamePt, row => row.CategoryNameEn, StringComparer.OrdinalIgnoreCase);

        ProductCategory? fallbackCategory = null;
        var productsToInsert = new List<Product>();

        foreach (var row in products)
        {
            if (existingProductIds.Contains(row.ProductId))
            {
                continue;
            }

            if (row.CategoryNamePt is null || !categoryLookup.TryGetValue(row.CategoryNamePt, out var category))
            {
                fallbackCategory ??= await EnsureFallbackCategoryAsync(categoryLookup, cancellationToken);
                category = fallbackCategory;
            }

            var categoryNamePt = row.CategoryNamePt ?? category.CategoryNamePt;
            var categoryNameEn = row.CategoryNamePt is null
                ? category.CategoryNameEn
                : translations.GetValueOrDefault(row.CategoryNamePt) ?? category.CategoryNameEn;

            productsToInsert.Add(new Product
            {
                CategoryId = category.Id,
                ProductName = OlistImportValueMapper.CreateImportedProductName(row.ProductId, categoryNameEn, categoryNamePt),
                Description = OlistImportValueMapper.CreateImportedProductDescription(row.ProductId, categoryNameEn, categoryNamePt),
                ProductNameLength = row.ProductNameLength ?? 0,
                ProductDescriptionLength = row.ProductDescriptionLength ?? 0,
                ProductPhotosQty = row.ProductPhotosQty ?? 0,
                ProductWeightG = row.ProductWeightG ?? 0,
                ProductLengthCm = row.ProductLengthCm ?? 0,
                ProductHeightCm = row.ProductHeightCm ?? 0,
                ProductWidthCm = row.ProductWidthCm ?? 0,
                OlistProductId = row.ProductId
            });
        }

        if (productsToInsert.Count == 0)
        {
            _logger.LogInformation("No products needed importing.");
            return;
        }

        _dbContext.Products.AddRange(productsToInsert);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Inserted {InsertedCount} products.", productsToInsert.Count);
    }

    private async Task<ProductCategory> EnsureFallbackCategoryAsync(
        IDictionary<string, ProductCategory> categoryLookup,
        CancellationToken cancellationToken)
    {
        if (categoryLookup.TryGetValue(FallbackCategoryNamePt, out var existingCategory))
        {
            return existingCategory;
        }

        var fallbackCategory = new ProductCategory
        {
            CategoryNamePt = FallbackCategoryNamePt,
            CategoryNameEn = FallbackCategoryNameEn
        };

        _dbContext.ProductCategories.Add(fallbackCategory);
        await _dbContext.SaveChangesAsync(cancellationToken);
        categoryLookup[FallbackCategoryNamePt] = fallbackCategory;

        _logger.LogInformation("Inserted fallback product category '{CategoryNamePt}'.", FallbackCategoryNamePt);
        return fallbackCategory;
    }

    private async Task SeedListingsAsync(
        IReadOnlyList<OrderRow> orders,
        IReadOnlyList<OrderItemRow> orderItems,
        CancellationToken cancellationToken)
    {
        var productLookup = await _dbContext.Products
            .AsNoTracking()
            .Where(product => product.OlistProductId != null)
            .ToDictionaryAsync(product => product.OlistProductId!, product => product.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var sellerLookup = await _dbContext.Sellers
            .AsNoTracking()
            .Where(seller => seller.OlistSellerId != null)
            .ToDictionaryAsync(seller => seller.OlistSellerId!, seller => seller.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var existingListings = await _dbContext.ProductListings
            .ToDictionaryAsync(listing => listing.Sku, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var orderPurchaseLookup = orders
            .ToDictionary(order => order.OrderId, order => order.OrderPurchaseTimestampUtc, StringComparer.OrdinalIgnoreCase);

        var listingCandidates = orderItems
            .Where(row =>
                productLookup.ContainsKey(row.ProductId) &&
                sellerLookup.ContainsKey(row.SellerId) &&
                orderPurchaseLookup.ContainsKey(row.OrderId))
            .Select(row => new
            {
                Row = row,
                PurchaseTimestampUtc = orderPurchaseLookup[row.OrderId],
                Sku = OlistImportValueMapper.BuildListingSku(row.SellerId, row.ProductId)
            })
            .GroupBy(candidate => candidate.Sku, StringComparer.OrdinalIgnoreCase)
            .Select(group => group
                .OrderByDescending(candidate => candidate.PurchaseTimestampUtc)
                .ThenByDescending(candidate => candidate.Row.OrderItemId)
                .First())
            .ToList();

        var listingsToInsert = new List<ProductListing>();
        var updatedListingsCount = 0;

        foreach (var candidate in listingCandidates)
        {
            var row = candidate.Row;
            var productId = productLookup[row.ProductId];
            var sellerId = sellerLookup[row.SellerId];

            if (existingListings.TryGetValue(candidate.Sku, out var existingListing))
            {
                var listingChanged = false;

                if (existingListing.ListingPrice != row.Price)
                {
                    existingListing.ListingPrice = row.Price;
                    listingChanged = true;
                }

                if (listingChanged)
                {
                    updatedListingsCount++;
                }

                continue;
            }

            listingsToInsert.Add(new ProductListing
            {
                SellerId = sellerId,
                ProductId = productId,
                Sku = candidate.Sku,
                ListingPrice = row.Price,
                InventoryQuantity = Random.Shared.Next(0, 11),
                VisibilityStatus = ListingVisibilityStatus.Published,
                PublishedAtUtc = DateTimeOffset.UtcNow
            });
        }

        if (listingsToInsert.Count == 0 && updatedListingsCount == 0)
        {
            _logger.LogInformation("No product listings needed importing.");
            return;
        }

        if (listingsToInsert.Count > 0)
        {
            _dbContext.ProductListings.AddRange(listingsToInsert);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            "Inserted {InsertedCount} product listings and updated {UpdatedCount}.",
            listingsToInsert.Count,
            updatedListingsCount);
    }

    private async Task SeedOrdersAsync(
        IReadOnlyList<OrderRow> orders,
        IReadOnlyList<OrderItemRow> orderItems,
        CancellationToken cancellationToken)
    {
        var customerLookup = await _dbContext.Customers
            .AsNoTracking()
            .Where(customer => customer.OlistCustomerId != null)
            .ToDictionaryAsync(customer => customer.OlistCustomerId!, customer => customer, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var productLookup = await _dbContext.Products
            .AsNoTracking()
            .Where(product => product.OlistProductId != null)
            .Select(product => product.OlistProductId!)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, cancellationToken);

        var sellerLookup = await _dbContext.Sellers
            .AsNoTracking()
            .Where(seller => seller.OlistSellerId != null)
            .Select(seller => seller.OlistSellerId!)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, cancellationToken);

        var listingLookup = await _dbContext.ProductListings
            .AsNoTracking()
            .Select(listing => listing.Sku)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, cancellationToken);

        var itemTotals = orderItems
            .Where(row =>
                productLookup.Contains(row.ProductId) &&
                sellerLookup.Contains(row.SellerId) &&
                listingLookup.Contains(OlistImportValueMapper.BuildListingSku(row.SellerId, row.ProductId)))
            .GroupBy(row => row.OrderId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => new
                {
                    Subtotal = group.Sum(item => item.Price),
                    Freight = group.Sum(item => item.FreightValue)
                },
                StringComparer.OrdinalIgnoreCase);

        var existingOrders = await _dbContext.Orders
            .ToDictionaryAsync(order => order.OrderNumber, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var ordersToInsert = new List<Order>();
        var updatedOrdersCount = 0;

        foreach (var row in orders)
        {
            if (!customerLookup.TryGetValue(row.CustomerId, out var customer) || customer.DefaultAddressId is null)
            {
                continue;
            }

            var totals = itemTotals.GetValueOrDefault(row.OrderId);
            var subtotal = totals?.Subtotal ?? 0m;
            var freight = totals?.Freight ?? 0m;

            if (existingOrders.TryGetValue(row.OrderId, out var existingOrder))
            {
                existingOrder.CustomerId = customer.Id;
                existingOrder.ShippingAddressId = customer.DefaultAddressId.Value;
                existingOrder.OrderStatus = OlistImportValueMapper.MapOrderStatus(row.OrderStatus);
                existingOrder.OrderPurchaseTimestampUtc = row.OrderPurchaseTimestampUtc;
                existingOrder.OrderApprovedAtUtc = row.OrderApprovedAtUtc;
                existingOrder.OrderDeliveredCarrierDateUtc = row.OrderDeliveredCarrierDateUtc;
                existingOrder.OrderDeliveredCustomerDateUtc = row.OrderDeliveredCustomerDateUtc;
                existingOrder.OrderEstimatedDeliveryDateUtc = row.OrderEstimatedDeliveryDateUtc;
                existingOrder.SubtotalAmount = subtotal;
                existingOrder.FreightAmount = freight;
                existingOrder.TotalAmount = subtotal + freight;
                updatedOrdersCount++;
                continue;
            }

            ordersToInsert.Add(new Order
            {
                CustomerId = customer.Id,
                ShippingAddressId = customer.DefaultAddressId.Value,
                OrderStatus = OlistImportValueMapper.MapOrderStatus(row.OrderStatus),
                OrderPurchaseTimestampUtc = row.OrderPurchaseTimestampUtc,
                OrderApprovedAtUtc = row.OrderApprovedAtUtc,
                OrderDeliveredCarrierDateUtc = row.OrderDeliveredCarrierDateUtc,
                OrderDeliveredCustomerDateUtc = row.OrderDeliveredCustomerDateUtc,
                OrderEstimatedDeliveryDateUtc = row.OrderEstimatedDeliveryDateUtc,
                SubtotalAmount = subtotal,
                FreightAmount = freight,
                TotalAmount = subtotal + freight,
                OrderNumber = row.OrderId
            });
        }

        if (ordersToInsert.Count == 0 && updatedOrdersCount == 0)
        {
            _logger.LogInformation("No orders needed importing.");
            return;
        }

        if (ordersToInsert.Count > 0)
        {
            _dbContext.Orders.AddRange(ordersToInsert);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            "Inserted {InsertedCount} orders and updated {UpdatedCount}.",
            ordersToInsert.Count,
            updatedOrdersCount);
    }

    private async Task SeedOrderItemsAsync(IReadOnlyList<OrderItemRow> orderItems, CancellationToken cancellationToken)
    {
        var orderLookup = await _dbContext.Orders
            .AsNoTracking()
            .ToDictionaryAsync(order => order.OrderNumber, order => order.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var productLookup = await _dbContext.Products
            .AsNoTracking()
            .Where(product => product.OlistProductId != null)
            .ToDictionaryAsync(product => product.OlistProductId!, product => product.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var sellerLookup = await _dbContext.Sellers
            .AsNoTracking()
            .Where(seller => seller.OlistSellerId != null)
            .ToDictionaryAsync(seller => seller.OlistSellerId!, seller => seller.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var listingLookup = await _dbContext.ProductListings
            .AsNoTracking()
            .ToDictionaryAsync(listing => listing.Sku, listing => listing.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var existingKeys = await _dbContext.OrderItems
            .AsNoTracking()
            .Select(item => new { item.OrderId, item.OrderItemId })
            .ToListAsync(cancellationToken);

        var existingKeySet = existingKeys
            .Select(item => $"{item.OrderId:N}:{item.OrderItemId}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var orderItemsToInsert = new List<OrderItem>();

        foreach (var row in orderItems)
        {
            if (!orderLookup.TryGetValue(row.OrderId, out var orderId) ||
                !productLookup.TryGetValue(row.ProductId, out var productId) ||
                !sellerLookup.TryGetValue(row.SellerId, out var sellerId))
            {
                continue;
            }

            var key = $"{orderId:N}:{row.OrderItemId}";
            if (existingKeySet.Contains(key))
            {
                continue;
            }

            var sku = OlistImportValueMapper.BuildListingSku(row.SellerId, row.ProductId);
            if (!listingLookup.TryGetValue(sku, out var listingId))
            {
                continue;
            }

            orderItemsToInsert.Add(new OrderItem
            {
                OrderId = orderId,
                OrderItemId = row.OrderItemId,
                ListingId = listingId,
                ProductId = productId,
                SellerId = sellerId,
                Quantity = 1,
                UnitPrice = row.Price,
                FreightValue = row.FreightValue,
                ShippingLimitDateUtc = row.ShippingLimitDateUtc
            });
        }

        if (orderItemsToInsert.Count == 0)
        {
            _logger.LogInformation("No order items needed importing.");
            return;
        }

        _dbContext.OrderItems.AddRange(orderItemsToInsert);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Inserted {InsertedCount} order items.", orderItemsToInsert.Count);
    }

    private async Task SeedPaymentsAsync(
        IReadOnlyList<OrderPaymentRow> payments,
        Guid currencyId,
        CancellationToken cancellationToken)
    {
        var orderLookup = await _dbContext.Orders
            .AsNoTracking()
            .ToDictionaryAsync(order => order.OrderNumber, order => order.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var existingKeys = await _dbContext.OrderPayments
            .AsNoTracking()
            .Select(payment => new { payment.OrderId, payment.PaymentSequential })
            .ToListAsync(cancellationToken);

        var existingKeySet = existingKeys
            .Select(payment => $"{payment.OrderId:N}:{payment.PaymentSequential}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var paymentsToInsert = new List<OrderPayment>();

        foreach (var row in payments)
        {
            if (!orderLookup.TryGetValue(row.OrderId, out var orderId))
            {
                continue;
            }

            var key = $"{orderId:N}:{row.PaymentSequential}";
            if (existingKeySet.Contains(key))
            {
                continue;
            }

            paymentsToInsert.Add(new OrderPayment
            {
                OrderId = orderId,
                PaymentSequential = row.PaymentSequential,
                CurrencyId = currencyId,
                PaymentType = OlistImportValueMapper.MapPaymentType(row.PaymentType),
                PaymentInstallments = row.PaymentInstallments,
                PaymentValue = row.PaymentValue,
                PaymentStatus = PaymentStatus.Paid,
                ExternalPaymentReference = $"{row.OrderId}:{row.PaymentSequential}"
            });
        }

        if (paymentsToInsert.Count == 0)
        {
            _logger.LogInformation("No payments needed importing.");
            return;
        }

        _dbContext.OrderPayments.AddRange(paymentsToInsert);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Inserted {InsertedCount} order payments.", paymentsToInsert.Count);
    }

    private async Task SeedReviewsAsync(IReadOnlyList<OrderReviewRow> reviews, CancellationToken cancellationToken)
    {
        var reviewTargets = await _dbContext.OrderItems
            .AsNoTracking()
            .Include(item => item.Order)
            .Where(item => item.Order != null)
            .OrderBy(item => item.OrderId)
            .ThenBy(item => item.OrderItemId)
            .Select(item => new ReviewImportTarget(
                item.Order!.OrderNumber,
                item.OrderId,
                item.OrderItemId,
                item.Order.CustomerId,
                item.ProductId))
            .ToListAsync(cancellationToken);

        var reviewTargetsByOrderNumber = reviewTargets
            .GroupBy(target => target.OrderNumber, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.OrdinalIgnoreCase);

        var existingReviewIds = await _dbContext.OrderReviews
            .AsNoTracking()
            .Where(review => review.OlistReviewId != null)
            .Select(review => review.OlistReviewId!)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, cancellationToken);

        var existingReviewFingerprints = await _dbContext.OrderReviews
            .AsNoTracking()
            .Select(review => new
            {
                review.OrderId,
                review.CustomerId,
                review.ProductId,
                review.OrderItemId,
                review.ReviewScore,
                review.ReviewCommentTitle,
                review.ReviewCommentMessage,
                review.ReviewCreationDateUtc,
                review.ReviewAnswerTimestampUtc
            })
            .ToListAsync(cancellationToken);

        var existingFingerprintSet = existingReviewFingerprints
            .Select(review => CreateReviewFingerprint(
                review.OrderId,
                review.ReviewScore,
                review.ReviewCommentTitle,
                review.ReviewCommentMessage,
                review.ReviewCreationDateUtc,
                review.ReviewAnswerTimestampUtc))
            .ToHashSet(StringComparer.Ordinal);

        var existingOrderItemReviewKeys = existingReviewFingerprints
            .Where(review => review.OrderItemId.HasValue)
            .Select(review => $"{review.OrderId:N}:{review.OrderItemId!.Value}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var existingCustomerProductReviewKeys = existingReviewFingerprints
            .Select(review => $"{review.CustomerId:N}:{review.ProductId:N}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var reviewsToInsert = new List<OrderReview>();

        foreach (var row in reviews)
        {
            if (!reviewTargetsByOrderNumber.TryGetValue(row.OrderId, out var targets))
            {
                continue;
            }

            var target = targets.FirstOrDefault(candidate =>
                !existingOrderItemReviewKeys.Contains($"{candidate.OrderId:N}:{candidate.OrderItemId}") &&
                !existingCustomerProductReviewKeys.Contains($"{candidate.CustomerId:N}:{candidate.ProductId:N}"));

            if (target is null)
            {
                continue;
            }

            var fingerprint = CreateReviewFingerprint(
                target.OrderId,
                row.ReviewScore,
                row.ReviewCommentTitle,
                row.ReviewCommentMessage,
                row.ReviewCreationDateUtc,
                row.ReviewAnswerTimestampUtc);

            if (existingReviewIds.Contains(row.ReviewId) || existingFingerprintSet.Contains(fingerprint))
            {
                continue;
            }

            reviewsToInsert.Add(new OrderReview
            {
                OrderId = target.OrderId,
                OrderItemId = target.OrderItemId,
                CustomerId = target.CustomerId,
                ProductId = target.ProductId,
                OlistReviewId = row.ReviewId,
                ReviewScore = row.ReviewScore,
                ReviewCommentTitle = row.ReviewCommentTitle,
                ReviewCommentMessage = row.ReviewCommentMessage,
                ReviewCreationDateUtc = row.ReviewCreationDateUtc,
                ReviewAnswerTimestampUtc = row.ReviewAnswerTimestampUtc
            });

            existingReviewIds.Add(row.ReviewId);
            existingFingerprintSet.Add(fingerprint);
            existingOrderItemReviewKeys.Add($"{target.OrderId:N}:{target.OrderItemId}");
            existingCustomerProductReviewKeys.Add($"{target.CustomerId:N}:{target.ProductId:N}");
        }

        if (reviewsToInsert.Count == 0)
        {
            _logger.LogInformation("No reviews needed importing.");
            return;
        }

        _dbContext.OrderReviews.AddRange(reviewsToInsert);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Inserted {InsertedCount} order reviews.", reviewsToInsert.Count);
    }

    private static string CreateReviewFingerprint(
        Guid orderId,
        int reviewScore,
        string? reviewCommentTitle,
        string? reviewCommentMessage,
        DateTimeOffset reviewCreationDateUtc,
        DateTimeOffset? reviewAnswerTimestampUtc) =>
        string.Join(
            "|",
            orderId.ToString("N"),
            reviewScore.ToString(),
            reviewCreationDateUtc.ToUniversalTime().ToString("O"),
            reviewAnswerTimestampUtc?.ToUniversalTime().ToString("O") ?? string.Empty,
            reviewCommentTitle?.Trim() ?? string.Empty,
            reviewCommentMessage?.Trim() ?? string.Empty);

    private sealed record OlistFileSet(
        string ProductCategoryTranslationsPath,
        string CustomersPath,
        string SellersPath,
        string ProductsPath,
        string OrdersPath,
        string OrderItemsPath,
        string OrderPaymentsPath,
        string OrderReviewsPath)
    {
        public IReadOnlyList<string> Paths =>
        [
            ProductCategoryTranslationsPath,
            CustomersPath,
            SellersPath,
            ProductsPath,
            OrdersPath,
            OrderItemsPath,
            OrderPaymentsPath,
            OrderReviewsPath
        ];
    }

    private sealed record OlistSourceData(
        IReadOnlyList<ProductCategoryTranslationRow> CategoryTranslations,
        IReadOnlyList<CustomerRow> Customers,
        IReadOnlyList<SellerRow> Sellers,
        IReadOnlyList<ProductRow> Products,
        IReadOnlyList<OrderRow> Orders,
        IReadOnlyList<OrderItemRow> OrderItems,
        IReadOnlyList<OrderPaymentRow> Payments,
        IReadOnlyList<OrderReviewRow> Reviews);

    private sealed record ReviewImportTarget(
        string OrderNumber,
        Guid OrderId,
        int OrderItemId,
        Guid CustomerId,
        Guid ProductId);
}
