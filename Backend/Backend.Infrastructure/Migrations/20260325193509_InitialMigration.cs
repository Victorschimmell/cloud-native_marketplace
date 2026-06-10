using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "address",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AddressLine1 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AddressLine2 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CountryCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_address", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "currency",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Symbol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_currency", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "product_category",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryNamePt = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CategoryNameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_category", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user_account",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    IsBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    AccountStatus = table.Column<int>(type: "integer", nullable: false),
                    FailedLoginAttempts = table.Column<int>(type: "integer", nullable: false),
                    LockedUntilUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastLoginAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_account", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "product",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ProductNameLength = table.Column<int>(type: "integer", nullable: false),
                    ProductDescriptionLength = table.Column<int>(type: "integer", nullable: false),
                    ProductPhotosQty = table.Column<int>(type: "integer", nullable: false),
                    ProductWeightG = table.Column<int>(type: "integer", nullable: false),
                    ProductLengthCm = table.Column<int>(type: "integer", nullable: false),
                    ProductHeightCm = table.Column<int>(type: "integer", nullable: false),
                    ProductWidthCm = table.Column<int>(type: "integer", nullable: false),
                    OlistProductId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product", x => x.Id);
                    table.ForeignKey(
                        name: "FK_product_product_category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "product_category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "audit_log",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActorIpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    TargetEntityType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TargetEntityId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Outcome = table.Column<int>(type: "integer", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_log", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_log_user_account_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "user_account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "customer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DefaultAddressId = table.Column<Guid>(type: "uuid", nullable: true),
                    OlistCustomerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OlistCustomerUniqueId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_customer_address_DefaultAddressId",
                        column: x => x.DefaultAddressId,
                        principalTable: "address",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_customer_user_account_UserId",
                        column: x => x.UserId,
                        principalTable: "user_account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "seller",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RegistrationNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PayoutInformation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DefaultAddressId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerificationStatus = table.Column<int>(type: "integer", nullable: false),
                    VerifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    OlistSellerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seller", x => x.Id);
                    table.ForeignKey(
                        name: "FK_seller_address_DefaultAddressId",
                        column: x => x.DefaultAddressId,
                        principalTable: "address",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_seller_user_account_UserId",
                        column: x => x.UserId,
                        principalTable: "user_account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_session",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_session", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_session_user_account_UserId",
                        column: x => x.UserId,
                        principalTable: "user_account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_listing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ListingPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    InventoryQuantity = table.Column<int>(type: "integer", nullable: false),
                    VisibilityStatus = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    PublishedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_listing", x => x.Id);
                    table.ForeignKey(
                        name: "FK_product_listing_product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_listing_seller_SellerId",
                        column: x => x.SellerId,
                        principalTable: "seller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "seller_verification_request",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    BusinessNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RegistrationNumberSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SubmittedDetails = table.Column<string>(type: "text", nullable: false),
                    ReviewNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seller_verification_request", x => x.Id);
                    table.ForeignKey(
                        name: "FK_seller_verification_request_seller_SellerId",
                        column: x => x.SellerId,
                        principalTable: "seller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_seller_verification_request_user_account_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "user_account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "shopping_cart",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RecoveredFromCartId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shopping_cart", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shopping_cart_shopping_cart_RecoveredFromCartId",
                        column: x => x.RecoveredFromCartId,
                        principalTable: "shopping_cart",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_shopping_cart_user_account_UserId",
                        column: x => x.UserId,
                        principalTable: "user_account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_shopping_cart_user_session_SessionId",
                        column: x => x.SessionId,
                        principalTable: "user_session",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "cart_item",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CartId = table.Column<Guid>(type: "uuid", nullable: false),
                    ListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPriceAtAddition = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    AddedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cart_item", x => x.Id);
                    table.ForeignKey(
                        name: "FK_cart_item_product_listing_ListingId",
                        column: x => x.ListingId,
                        principalTable: "product_listing",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cart_item_shopping_cart_CartId",
                        column: x => x.CartId,
                        principalTable: "shopping_cart",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShippingAddressId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderStatus = table.Column<int>(type: "integer", nullable: false),
                    OrderPurchaseTimestampUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    OrderApprovedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    OrderDeliveredCarrierDateUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    OrderDeliveredCustomerDateUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    OrderEstimatedDeliveryDateUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SubtotalAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    FreightAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    PlacedFromCartId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrderNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order", x => x.Id);
                    table.ForeignKey(
                        name: "FK_order_address_ShippingAddressId",
                        column: x => x.ShippingAddressId,
                        principalTable: "address",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_order_customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_order_shopping_cart_PlacedFromCartId",
                        column: x => x.PlacedFromCartId,
                        principalTable: "shopping_cart",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "order_item",
                columns: table => new
                {
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderItemId = table.Column<int>(type: "integer", nullable: false),
                    ListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    FreightValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ShippingLimitDateUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_item", x => new { x.OrderId, x.OrderItemId });
                    table.ForeignKey(
                        name: "FK_order_item_order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_order_item_product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_order_item_product_listing_ListingId",
                        column: x => x.ListingId,
                        principalTable: "product_listing",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_order_item_seller_SellerId",
                        column: x => x.SellerId,
                        principalTable: "seller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_payment",
                columns: table => new
                {
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentSequential = table.Column<int>(type: "integer", nullable: false),
                    CurrencyId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentType = table.Column<int>(type: "integer", nullable: false),
                    PaymentInstallments = table.Column<int>(type: "integer", nullable: false),
                    PaymentValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    PaymentStatus = table.Column<int>(type: "integer", nullable: false),
                    ExternalPaymentReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PaidAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_payment", x => new { x.OrderId, x.PaymentSequential });
                    table.ForeignKey(
                        name: "FK_order_payment_currency_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "currency",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_order_payment_order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_review",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewScore = table.Column<int>(type: "integer", nullable: false),
                    ReviewCommentTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ReviewCommentMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReviewCreationDateUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReviewAnswerTimestampUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_review", x => x.Id);
                    table.ForeignKey(
                        name: "FK_order_review_order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shipment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CarrierName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TrackingNumber = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ShipmentStatus = table.Column<int>(type: "integer", nullable: false),
                    ShippedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeliveredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReturnedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shipment_order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_shipment_seller_SellerId",
                        column: x => x.SellerId,
                        principalTable: "seller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_ActorUserId",
                table: "audit_log",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_TargetEntityType_TargetEntityId",
                table: "audit_log",
                columns: new[] { "TargetEntityType", "TargetEntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_cart_item_CartId",
                table: "cart_item",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_cart_item_ListingId",
                table: "cart_item",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_currency_Code",
                table: "currency",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customer_DefaultAddressId",
                table: "customer",
                column: "DefaultAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_customer_UserId",
                table: "customer",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_order_CustomerId",
                table: "order",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_order_OrderNumber",
                table: "order",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_order_PlacedFromCartId",
                table: "order",
                column: "PlacedFromCartId");

            migrationBuilder.CreateIndex(
                name: "IX_order_ShippingAddressId",
                table: "order",
                column: "ShippingAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_order_item_ListingId",
                table: "order_item",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_order_item_ProductId",
                table: "order_item",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_order_item_SellerId",
                table: "order_item",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_order_payment_CurrencyId",
                table: "order_payment",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_order_review_OrderId",
                table: "order_review",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_product_CategoryId",
                table: "product",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_product_OlistProductId",
                table: "product",
                column: "OlistProductId");

            migrationBuilder.CreateIndex(
                name: "IX_product_listing_ProductId",
                table: "product_listing",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_product_listing_SellerId",
                table: "product_listing",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_product_listing_Sku",
                table: "product_listing",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_seller_DefaultAddressId",
                table: "seller",
                column: "DefaultAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_seller_UserId",
                table: "seller",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_seller_verification_request_ReviewedByUserId",
                table: "seller_verification_request",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_seller_verification_request_SellerId",
                table: "seller_verification_request",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_shipment_OrderId",
                table: "shipment",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_shipment_SellerId",
                table: "shipment",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_shipment_TrackingNumber",
                table: "shipment",
                column: "TrackingNumber");

            migrationBuilder.CreateIndex(
                name: "IX_shopping_cart_RecoveredFromCartId",
                table: "shopping_cart",
                column: "RecoveredFromCartId");

            migrationBuilder.CreateIndex(
                name: "IX_shopping_cart_SessionId",
                table: "shopping_cart",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_shopping_cart_UserId",
                table: "shopping_cart",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_account_Email",
                table: "user_account",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_session_UserId",
                table: "user_session",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_log");

            migrationBuilder.DropTable(
                name: "cart_item");

            migrationBuilder.DropTable(
                name: "order_item");

            migrationBuilder.DropTable(
                name: "order_payment");

            migrationBuilder.DropTable(
                name: "order_review");

            migrationBuilder.DropTable(
                name: "seller_verification_request");

            migrationBuilder.DropTable(
                name: "shipment");

            migrationBuilder.DropTable(
                name: "product_listing");

            migrationBuilder.DropTable(
                name: "currency");

            migrationBuilder.DropTable(
                name: "order");

            migrationBuilder.DropTable(
                name: "product");

            migrationBuilder.DropTable(
                name: "seller");

            migrationBuilder.DropTable(
                name: "customer");

            migrationBuilder.DropTable(
                name: "shopping_cart");

            migrationBuilder.DropTable(
                name: "product_category");

            migrationBuilder.DropTable(
                name: "address");

            migrationBuilder.DropTable(
                name: "user_session");

            migrationBuilder.DropTable(
                name: "user_account");
        }
    }
}
