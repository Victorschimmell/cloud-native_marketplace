using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewCustomerProductConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "order_review",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductId",
                table: "order_review",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(
                """
                WITH ranked_reviews AS (
                    SELECT
                        r."Id",
                        r."OrderId",
                        ROW_NUMBER() OVER (
                            PARTITION BY r."OrderId"
                            ORDER BY r."ReviewCreationDateUtc", r."Id"
                        ) AS rn
                    FROM order_review r
                    WHERE r."OrderItemId" IS NULL
                ),
                ranked_items AS (
                    SELECT
                        oi."OrderId",
                        oi."OrderItemId",
                        oi."ProductId",
                        ROW_NUMBER() OVER (
                            PARTITION BY oi."OrderId"
                            ORDER BY oi."OrderItemId"
                        ) AS rn
                    FROM order_item oi
                )
                UPDATE order_review r
                SET
                    "OrderItemId" = ranked_items."OrderItemId",
                    "CustomerId" = o."CustomerId",
                    "ProductId" = ranked_items."ProductId"
                FROM ranked_reviews
                JOIN ranked_items
                    ON ranked_items."OrderId" = ranked_reviews."OrderId"
                    AND ranked_items.rn = ranked_reviews.rn
                JOIN "order" o
                    ON o."Id" = ranked_reviews."OrderId"
                WHERE r."Id" = ranked_reviews."Id";
                """);

            migrationBuilder.Sql(
                """
                UPDATE order_review r
                SET
                    "CustomerId" = o."CustomerId",
                    "ProductId" = oi."ProductId"
                FROM "order" o
                JOIN order_item oi
                    ON oi."OrderId" = o."Id"
                WHERE r."OrderId" = o."Id"
                    AND r."OrderId" = oi."OrderId"
                    AND r."OrderItemId" = oi."OrderItemId";
                """);

            migrationBuilder.Sql(
                """
                DELETE FROM order_review
                WHERE "CustomerId" IS NULL
                    OR "ProductId" IS NULL
                    OR "OrderItemId" IS NULL;
                """);

            migrationBuilder.Sql(
                """
                DELETE FROM order_review r
                USING (
                    SELECT "Id"
                    FROM (
                        SELECT
                            "Id",
                            ROW_NUMBER() OVER (
                                PARTITION BY "CustomerId", "ProductId"
                                ORDER BY "ReviewCreationDateUtc", "Id"
                            ) AS rn
                        FROM order_review
                    ) ranked_reviews
                    WHERE ranked_reviews.rn > 1
                ) duplicates
                WHERE r."Id" = duplicates."Id";
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "order_review",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "order_review",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_order_review_CustomerId_ProductId",
                table: "order_review",
                columns: new[] { "CustomerId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_order_review_ProductId",
                table: "order_review",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_order_review_customer_CustomerId",
                table: "order_review",
                column: "CustomerId",
                principalTable: "customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_order_review_product_ProductId",
                table: "order_review",
                column: "ProductId",
                principalTable: "product",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_order_review_customer_CustomerId",
                table: "order_review");

            migrationBuilder.DropForeignKey(
                name: "FK_order_review_product_ProductId",
                table: "order_review");

            migrationBuilder.DropIndex(
                name: "IX_order_review_CustomerId_ProductId",
                table: "order_review");

            migrationBuilder.DropIndex(
                name: "IX_order_review_ProductId",
                table: "order_review");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "order_review");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "order_review");
        }
    }
}
