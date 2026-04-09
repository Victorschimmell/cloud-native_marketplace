using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOlistReviewIdToOrderReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OlistReviewId",
                table: "order_review",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_order_review_OlistReviewId",
                table: "order_review",
                column: "OlistReviewId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_order_review_OlistReviewId",
                table: "order_review");

            migrationBuilder.DropColumn(
                name: "OlistReviewId",
                table: "order_review");
        }
    }
}
