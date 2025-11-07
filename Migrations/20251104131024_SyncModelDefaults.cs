using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CanLove_Backend.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
