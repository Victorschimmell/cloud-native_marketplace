using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Services;
using Backend.Domain.Entities.Catalog;
using Backend.Domain.Entities.IdentityAccess;
using Backend.Domain.Enums;
using Backend.UnitTests.Application.Fakes;

namespace Backend.UnitTests.Application.Services;

public sealed class ProductServiceTests
{
    private static readonly Guid CurrentUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task GetBrowseProductsAsync_WhenPageIsInvalid_ReturnsValidationFailure()
    {
        var fixture = CreateFixture();

        var result = await fixture.Service.GetBrowseProductsAsync(
            new BrowseProductsRequest(null, null, "newest", "BRL", 0, 10),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.ValidationFailure, result.FailureType);
    }

    [Fact]
    public async Task GetBrowseProductsAsync_WhenSortIsInvalid_ReturnsValidationFailure()
    {
        var fixture = CreateFixture();

        var result = await fixture.Service.GetBrowseProductsAsync(
            new BrowseProductsRequest(null, null, "oldest", "BRL", 1, 10),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.ValidationFailure, result.FailureType);
    }

    [Fact]
    public async Task GetBrowseProductsAsync_WhenCurrencyIsUnsupported_ReturnsValidationFailure()
    {
        var fixture = CreateFixture(currencyConversionService: new FakeCurrencyConversionService { ForceUnsupported = true });

        var result = await fixture.Service.GetBrowseProductsAsync(
            new BrowseProductsRequest(null, null, "newest", "EUR", 1, 10),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.ValidationFailure, result.FailureType);
    }

    [Fact]
    public async Task GetBrowseProductsAsync_WhenRequestIsValid_ReturnsConvertedProducts()
    {
        var category = CreateCategory();
        var product = CreateProduct(category);
        var seller = CreateSeller();
        var listing = CreateListing(product, seller, price: 100m, visibilityStatus: ListingVisibilityStatus.Published);
        var listingRepository = new FakeProductListingRepository();
        listingRepository.Listings.Add(listing);
        var fixture = CreateFixture(productListingRepository: listingRepository);

        var result = await fixture.Service.GetBrowseProductsAsync(
            new BrowseProductsRequest(category.Id, "coffee", "price-asc", "USD", 1, 10),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value!.Items);
        Assert.Equal(product.Id, item.ProductId);
        Assert.Equal(50m, item.Price);
        Assert.Equal("USD", item.CurrencyCode);
    }

    [Fact]
    public async Task GetDetailsAsync_WhenListingDoesNotExist_ReturnsNotFound()
    {
        var fixture = CreateFixture();

        var result = await fixture.Service.GetDetailsAsync(Guid.NewGuid(), null, "BRL", TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task GetDetailsAsync_WhenListingExists_ReturnsConvertedDetails()
    {
        var category = CreateCategory();
        var product = CreateProduct(category);
        var seller = CreateSeller();
        var listing = CreateListing(product, seller, price: 80m, visibilityStatus: ListingVisibilityStatus.Published);
        var listingRepository = new FakeProductListingRepository();
        listingRepository.Listings.Add(listing);
        var fixture = CreateFixture(productListingRepository: listingRepository);

        var result = await fixture.Service.GetDetailsAsync(product.Id, listing.Id, "DKK", TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(listing.Id, result.Value!.ListingId);
        Assert.Equal(40m, result.Value.Price);
        Assert.Equal("DKK", result.Value.CurrencyCode);
        Assert.Equal(VerificationStatus.Verified.ToString(), result.Value.SellerVerificationStatus);
    }

    [Theory]
    [InlineData("", "Description", 10, 1, "Product name is required.")]
    [InlineData("Product", " ", 10, 1, "Description is required.")]
    [InlineData("Product", "Description", -1, 1, "Price must be non-negative.")]
    [InlineData("Product", "Description", 10, -1, "Inventory quantity must be non-negative.")]
    public async Task CreateAsync_WhenRequestIsInvalid_ReturnsValidationFailure(
        string productName,
        string description,
        decimal price,
        int inventoryQuantity,
        string expectedError)
    {
        var fixture = CreateFixture();

        var result = await fixture.Service.CreateAsync(
            CreateCreateRequest(productName: productName, description: description, price: price, inventoryQuantity: inventoryQuantity),
            "BRL",
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.ValidationFailure, result.FailureType);
        Assert.Equal(expectedError, result.Error);
        Assert.Equal(0, fixture.ProductRepository.AddCalls);
    }

    [Fact]
    public async Task CreateAsync_WhenUserIsNotAuthenticated_ReturnsUnauthorized()
    {
        var fixture = CreateFixture(currentUserProvider: new FakeCurrentUserProvider { UserId = null, IsAuthenticated = false });

        var result = await fixture.Service.CreateAsync(CreateCreateRequest(), "BRL", TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.Unauthorized, result.FailureType);
    }

    [Fact]
    public async Task CreateAsync_WhenSellerIsNotVerified_ReturnsForbidden()
    {
        var sellerRepository = new FakeSellerRepository
        {
            Seller = CreateSeller(verificationStatus: VerificationStatus.Pending)
        };
        var fixture = CreateFixture(sellerRepository: sellerRepository);

        var result = await fixture.Service.CreateAsync(CreateCreateRequest(), "BRL", TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.Forbidden, result.FailureType);
    }

    [Fact]
    public async Task CreateAsync_WhenCategoryDoesNotExist_ReturnsNotFound()
    {
        var fixture = CreateFixture(categoryRepository: new FakeProductCategoryRepository());

        var result = await fixture.Service.CreateAsync(CreateCreateRequest(), "BRL", TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
        Assert.Equal(0, fixture.ProductRepository.AddCalls);
    }

    [Fact]
    public async Task CreateAsync_WhenRequestIsValid_CreatesDraftListingAndWritesAuditLog()
    {
        var unitOfWork = new FakeUnitOfWork();
        var auditLogService = new FakeAuditLogService();
        var fixture = CreateFixture(unitOfWork: unitOfWork, auditLogService: auditLogService);

        var result = await fixture.Service.CreateAsync(
            CreateCreateRequest(fixture.CategoryRepository.Category!.Id, productName: "  Fresh coffee  ", description: "  Roasted beans  "),
            "BRL",
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("Fresh coffee", result.Value!.ProductName);
        Assert.Equal("Roasted beans", result.Value.Description);
        Assert.Equal(1, fixture.ProductRepository.AddCalls);
        Assert.Equal(1, fixture.ProductListingRepository.AddCalls);
        Assert.Equal(ListingVisibilityStatus.Draft, Assert.Single(fixture.ProductListingRepository.Listings).VisibilityStatus);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.Single(auditLogService.Entries);
    }

    [Fact]
    public async Task CreateAsync_WhenCurrencyIsBRL_ConvertsListingPrice()
    {
        var unitOfWork = new FakeUnitOfWork();
        var auditLogService = new FakeAuditLogService();
        var fixture = CreateFixture(unitOfWork: unitOfWork, auditLogService: auditLogService);

        var result = await fixture.Service.CreateAsync(
            CreateCreateRequest(fixture.CategoryRepository.Category!.Id, price: 100m),
            "BRL",
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, fixture.ProductListingRepository.AddCalls);
        Assert.Equal(100m, Assert.Single(fixture.ProductListingRepository.Listings).ListingPrice);
        Assert.Single(auditLogService.Entries);
    }

    [Fact]
    public async Task CreateAsync_WhenCurrencyIsUSD_ConvertsListingPrice()
    {
        var unitOfWork = new FakeUnitOfWork();
        var auditLogService = new FakeAuditLogService();
        var fixture = CreateFixture(unitOfWork: unitOfWork, auditLogService: auditLogService);

        var result = await fixture.Service.CreateAsync(
            CreateCreateRequest(fixture.CategoryRepository.Category!.Id, price: 100m),
            "USD",
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, fixture.ProductListingRepository.AddCalls);
        Assert.Equal(200m, Assert.Single(fixture.ProductListingRepository.Listings).ListingPrice);
        Assert.Single(auditLogService.Entries);
    }

    [Fact]
    public async Task CreateAsync_WhenCurrencyIsDKK_ConvertsListingPrice()
    {
        var unitOfWork = new FakeUnitOfWork();
        var auditLogService = new FakeAuditLogService();
        var fixture = CreateFixture(unitOfWork: unitOfWork, auditLogService: auditLogService);

        var result = await fixture.Service.CreateAsync(
            CreateCreateRequest(fixture.CategoryRepository.Category!.Id, price: 100m),
            "DKK",
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, fixture.ProductListingRepository.AddCalls);
        Assert.Equal(200m, Assert.Single(fixture.ProductListingRepository.Listings).ListingPrice);
        Assert.Single(auditLogService.Entries);
    }

    [Fact]
    public async Task GetSellerListingsAsync_WhenSellerIsVerified_ReturnsListings()
    {
        var category = CreateCategory();
        var product = CreateProduct(category);
        var seller = CreateSeller();
        var listingRepository = new FakeProductListingRepository();
        listingRepository.Listings.Add(CreateListing(product, seller, visibilityStatus: ListingVisibilityStatus.Published));
        var fixture = CreateFixture(productListingRepository: listingRepository, sellerRepository: new FakeSellerRepository { Seller = seller });

        var result = await fixture.Service.GetSellerListingsAsync("BRL", TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        var listing = Assert.Single(result.Value!);
        Assert.Equal(product.Id, listing.ProductId);
        Assert.Equal("Coffee", listing.ProductName);
    }

    [Fact]
    public async Task UpdateAsync_WhenVisibilityStatusIsInvalid_ReturnsValidationFailure()
    {
        var fixture = CreateFixture();

        var result = await fixture.Service.UpdateAsync(
            CreateUpdateRequest(visibilityStatus: "Archived"),
            "BRL",
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.ValidationFailure, result.FailureType);
    }

    [Fact]
    public async Task UpdateAsync_WhenListingIsMissingOrNotOwned_ReturnsNotFound()
    {
        var category = CreateCategory();
        var product = CreateProduct(category);
        var seller = CreateSeller();
        var listing = CreateListing(product, seller, sellerId: Guid.NewGuid());
        var listingRepository = new FakeProductListingRepository();
        listingRepository.Listings.Add(listing);
        var fixture = CreateFixture(productListingRepository: listingRepository, sellerRepository: new FakeSellerRepository { Seller = seller });

        var result = await fixture.Service.UpdateAsync(
            CreateUpdateRequest(listingId: listing.Id, categoryId: category.Id),
            "BRL",
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
        Assert.Equal(0, listingRepository.UpdateCalls);
    }

    [Fact]
    public async Task UpdateAsync_WhenProductIsShared_ForksProductForListing()
    {
        var category = CreateCategory();
        var product = CreateProduct(category);
        var seller = CreateSeller();
        var listing = CreateListing(product, seller);
        var otherListing = CreateListing(product, CreateSeller(userId: Guid.NewGuid()), sellerId: Guid.NewGuid());
        var productRepository = new FakeProductRepository { Product = product };
        var listingRepository = new FakeProductListingRepository();
        listingRepository.Listings.Add(listing);
        listingRepository.Listings.Add(otherListing);
        var fixture = CreateFixture(
            productRepository: productRepository,
            productListingRepository: listingRepository,
            categoryRepository: new FakeProductCategoryRepository { Category = category },
            sellerRepository: new FakeSellerRepository { Seller = seller });

        var result = await fixture.Service.UpdateAsync(
            CreateUpdateRequest(listingId: listing.Id, categoryId: category.Id, productName: "New coffee", visibilityStatus: "Published"),
            "BRL",
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("New coffee", result.Value!.ProductName);
        Assert.NotEqual(product.Id, listing.ProductId);
        Assert.Equal(1, productRepository.AddCalls);
        Assert.Equal(0, productRepository.UpdateCalls);
        Assert.Equal(ListingVisibilityStatus.Published, listing.VisibilityStatus);
    }

    [Fact]
    public async Task UpdateAsync_WhenProductIsOnlyUsedByListing_UpdatesExistingProduct()
    {
        var category = CreateCategory();
        var product = CreateProduct(category);
        var seller = CreateSeller();
        var listing = CreateListing(product, seller);
        var productRepository = new FakeProductRepository { Product = product };
        var listingRepository = new FakeProductListingRepository();
        listingRepository.Listings.Add(listing);
        var unitOfWork = new FakeUnitOfWork();
        var auditLogService = new FakeAuditLogService();
        var fixture = CreateFixture(
            productRepository: productRepository,
            productListingRepository: listingRepository,
            categoryRepository: new FakeProductCategoryRepository { Category = category },
            sellerRepository: new FakeSellerRepository { Seller = seller },
            unitOfWork: unitOfWork,
            auditLogService: auditLogService);

        var result = await fixture.Service.UpdateAsync(
            CreateUpdateRequest(listingId: listing.Id, categoryId: category.Id, productName: "New coffee", price: 75m),
            "BRL",
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(product.Id, result.Value!.Id);
        Assert.Equal("New coffee", product.ProductName);
        Assert.Equal(75m, listing.ListingPrice);
        Assert.Equal(1, productRepository.UpdateCalls);
        Assert.Equal(1, listingRepository.UpdateCalls);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.Single(auditLogService.Entries);
    }

    [Fact]
    public async Task DeleteListingAsync_WhenListingIsOwned_DeletesListingAndWritesAuditLog()
    {
        var product = CreateProduct(CreateCategory());
        var seller = CreateSeller();
        var listing = CreateListing(product, seller);
        var listingRepository = new FakeProductListingRepository();
        listingRepository.Listings.Add(listing);
        var unitOfWork = new FakeUnitOfWork();
        var auditLogService = new FakeAuditLogService();
        var fixture = CreateFixture(
            productListingRepository: listingRepository,
            sellerRepository: new FakeSellerRepository { Seller = seller },
            unitOfWork: unitOfWork,
            auditLogService: auditLogService);

        var result = await fixture.Service.DeleteListingAsync(listing.Id, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.True(listing.IsDeleted);
        Assert.Equal(1, listingRepository.DeleteCalls);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.Single(auditLogService.Entries);
    }

    private static ProductServiceFixture CreateFixture(
        FakeProductRepository? productRepository = null,
        FakeProductListingRepository? productListingRepository = null,
        FakeProductCategoryRepository? categoryRepository = null,
        FakeSellerRepository? sellerRepository = null,
        FakeCurrentUserProvider? currentUserProvider = null,
        FakeAuditLogService? auditLogService = null,
        FakeUnitOfWork? unitOfWork = null,
        FakeCurrencyConversionService? currencyConversionService = null)
    {
        var category = CreateCategory();
        categoryRepository ??= new FakeProductCategoryRepository { Category = category };
        sellerRepository ??= new FakeSellerRepository { Seller = CreateSeller() };

        productRepository ??= new FakeProductRepository();
        productListingRepository ??= new FakeProductListingRepository();
        currentUserProvider ??= new FakeCurrentUserProvider { UserId = CurrentUserId, IsAuthenticated = true };
        auditLogService ??= new FakeAuditLogService();
        unitOfWork ??= new FakeUnitOfWork();
        currencyConversionService ??= new FakeCurrencyConversionService();

        var service = new ProductService(
            productRepository,
            productListingRepository,
            categoryRepository,
            sellerRepository,
            currentUserProvider,
            auditLogService,
            unitOfWork,
            currencyConversionService);

        return new ProductServiceFixture(
            service,
            productRepository,
            productListingRepository,
            categoryRepository,
            sellerRepository,
            currentUserProvider,
            auditLogService,
            unitOfWork,
            currencyConversionService);
    }

    private static ProductCategory CreateCategory() =>
        new()
        {
            CategoryNamePt = "cafe",
            CategoryNameEn = "Coffee"
        };

    private static Product CreateProduct(ProductCategory category, string productName = "Coffee") =>
        new()
        {
            CategoryId = category.Id,
            ProductName = productName,
            Description = "Roasted beans",
            ImageUrl = "https://example.com/coffee.jpg",
            ProductNameLength = productName.Length,
            ProductDescriptionLength = "Roasted beans".Length,
            ProductPhotosQty = 1,
            ProductWeightG = 250,
            ProductLengthCm = 10,
            ProductHeightCm = 20,
            ProductWidthCm = 8,
            Category = category
        };

    private static Seller CreateSeller(Guid? userId = null, VerificationStatus verificationStatus = VerificationStatus.Verified) =>
        new()
        {
            UserId = userId ?? CurrentUserId,
            BusinessName = "Coffee Seller",
            RegistrationNumber = "123456",
            PayoutInformation = "bank",
            VerificationStatus = verificationStatus
        };

    private static ProductListing CreateListing(
        Product product,
        Seller seller,
        Guid? sellerId = null,
        decimal price = 120m,
        ListingVisibilityStatus visibilityStatus = ListingVisibilityStatus.Draft) =>
        new()
        {
            SellerId = sellerId ?? seller.Id,
            ProductId = product.Id,
            Sku = "sku-1",
            ListingPrice = price,
            InventoryQuantity = 5,
            VisibilityStatus = visibilityStatus,
            Product = product,
            Seller = seller
        };

    private static CreateProductRequest CreateCreateRequest(
        Guid? categoryId = null,
        string productName = "Coffee",
        string description = "Roasted beans",
        decimal price = 120m,
        int inventoryQuantity = 5) =>
        new(
            categoryId ?? Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            productName,
            description,
            "https://example.com/coffee.jpg",
            1,
            250,
            10,
            20,
            8,
            price,
            inventoryQuantity);

    private static UpdateProductRequest CreateUpdateRequest(
        Guid? listingId = null,
        Guid? categoryId = null,
        string productName = "Coffee",
        string description = "Roasted beans",
        decimal price = 120m,
        int inventoryQuantity = 5,
        string visibilityStatus = "Draft") =>
        new(
            listingId ?? Guid.NewGuid(),
            categoryId ?? Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            productName,
            description,
            "https://example.com/coffee.jpg",
            1,
            250,
            10,
            20,
            8,
            price,
            inventoryQuantity,
            visibilityStatus);

    private sealed record ProductServiceFixture(
        ProductService Service,
        FakeProductRepository ProductRepository,
        FakeProductListingRepository ProductListingRepository,
        FakeProductCategoryRepository CategoryRepository,
        FakeSellerRepository SellerRepository,
        FakeCurrentUserProvider CurrentUserProvider,
        FakeAuditLogService AuditLogService,
        FakeUnitOfWork UnitOfWork,
        FakeCurrencyConversionService CurrencyConversionService);
}
