using Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260520162000_AddOrderItemFulfillmentStatus")]
    public partial class AddOrderItemFulfillmentStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FulfillmentStatus",
                table: "order_item",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FulfillmentApprovedAtUtc",
                table: "order_item",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FulfillmentProcessingAtUtc",
                table: "order_item",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FulfillmentShippedAtUtc",
                table: "order_item",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE order_item AS oi
                SET
                    "FulfillmentStatus" = CASE
                        WHEN o."OrderStatus" = 5 THEN 4
                        ELSE o."OrderStatus"
                    END,
                    "FulfillmentApprovedAtUtc" = CASE
                        WHEN o."OrderStatus" IN (2, 3, 4, 5) THEN o."OrderApprovedAtUtc"
                        ELSE NULL
                    END,
                    "FulfillmentShippedAtUtc" = CASE
                        WHEN o."OrderStatus" IN (4, 5) THEN o."OrderDeliveredCarrierDateUtc"
                        ELSE NULL
                    END
                FROM "order" AS o
                WHERE oi."OrderId" = o."Id";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FulfillmentApprovedAtUtc",
                table: "order_item");

            migrationBuilder.DropColumn(
                name: "FulfillmentProcessingAtUtc",
                table: "order_item");

            migrationBuilder.DropColumn(
                name: "FulfillmentShippedAtUtc",
                table: "order_item");

            migrationBuilder.DropColumn(
                name: "FulfillmentStatus",
                table: "order_item");
        }
    }
}
