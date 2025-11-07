using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CanLove_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCaseReviewItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CaseReviewItem",
                columns: table => new
                {
                    review_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    case_id = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    target_id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "PendingReview"),
                    submitted_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    submitted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    reviewed_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    review_comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseReviewItem", x => x.review_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaseReviewItem_CaseId",
                table: "CaseReviewItem",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "IX_CaseReviewItem_Status",
                table: "CaseReviewItem",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_CaseReviewItem_Type_Status",
                table: "CaseReviewItem",
                columns: new[] { "type", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaseReviewItem");
        }
    }
}
