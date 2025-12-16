using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CanLove_Backend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDeletedColumnsToCases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "deleted",
                table: "Cases",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "Cases",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deleted_by",
                table: "Cases",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deleted",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                table: "Cases");
        }
    }
}
