using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminIssues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admin_issue",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReportedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Resolution = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_issue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_admin_issue_user_account_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "user_account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_admin_issue_user_account_ReportedByUserId",
                        column: x => x.ReportedByUserId,
                        principalTable: "user_account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_admin_issue_user_account_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalTable: "user_account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_admin_issue_AssignedToUserId",
                table: "admin_issue",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_admin_issue_Priority",
                table: "admin_issue",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_admin_issue_ReportedByUserId",
                table: "admin_issue",
                column: "ReportedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_admin_issue_ResolvedByUserId",
                table: "admin_issue",
                column: "ResolvedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_admin_issue_Status",
                table: "admin_issue",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_issue");
        }
    }
}
