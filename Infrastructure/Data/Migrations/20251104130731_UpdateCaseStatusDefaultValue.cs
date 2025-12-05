using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CanLove_Backend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCaseStatusDefaultValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 只處理 Case.Status 預設值，避免環境差異導致 Staff/BlobStorage FK 變更衝突
            // 更新 Case.Status 的預設值從 "Draft" 改為 "PendingReview"
            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "Cases",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "PendingReview",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Draft");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 還原 Case.Status 的預設值為 "Draft"
            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "Cases",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Draft",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "PendingReview");
        }
    }
}
