using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderItemProductReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_order_review_OrderId",
                table: "order_review");

            migrationBuilder.AddColumn<int>(
                name: "OrderItemId",
                table: "order_review",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_order_review_OrderId_OrderItemId",
                table: "order_review",
                columns: new[] { "OrderId", "OrderItemId" },
                unique: true,
                filter: "\"OrderItemId\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_order_review_order_item_OrderId_OrderItemId",
                table: "order_review",
                columns: new[] { "OrderId", "OrderItemId" },
                principalTable: "order_item",
                principalColumns: new[] { "OrderId", "OrderItemId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_order_review_order_item_OrderId_OrderItemId",
                table: "order_review");

            migrationBuilder.DropIndex(
                name: "IX_order_review_OrderId_OrderItemId",
                table: "order_review");

            migrationBuilder.DropColumn(
                name: "OrderItemId",
                table: "order_review");

            migrationBuilder.CreateIndex(
                name: "IX_order_review_OrderId",
                table: "order_review",
                column: "OrderId");
        }
    }
}
