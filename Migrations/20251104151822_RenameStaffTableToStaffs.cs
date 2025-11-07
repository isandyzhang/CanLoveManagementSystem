using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CanLove_Backend.Migrations
{
    /// <inheritdoc />
    public partial class RenameStaffTableToStaffs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 動態查找並刪除 BlobStorage 表上引用 Staff 的 FK
            migrationBuilder.Sql(@"
                DECLARE @FKName NVARCHAR(128);
                SELECT @FKName = name 
                FROM sys.foreign_keys 
                WHERE parent_object_id = OBJECT_ID('BlobStorage') 
                  AND referenced_object_id = OBJECT_ID('Staff');
                
                IF @FKName IS NOT NULL
                BEGIN
                    EXEC('ALTER TABLE [BlobStorage] DROP CONSTRAINT [' + @FKName + ']');
                END
            ");

            // 直接使用 sp_rename 更名資料表，避免受限於既有 PK 名稱差異
            migrationBuilder.Sql("EXEC sp_rename 'dbo.Staff', 'Staffs';");

            // 重新建立 BlobStorage 的 FK，指向新的 Staffs 表
            migrationBuilder.AddForeignKey(
                name: "FK_BlobStorage_Staffs_uploaded_by",
                table: "BlobStorage",
                column: "uploaded_by",
                principalTable: "Staffs",
                principalColumn: "staff_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlobStorage_Staffs_uploaded_by",
                table: "BlobStorage");

            // 使用 sp_rename 將表名改回 Staff
            migrationBuilder.Sql("EXEC sp_rename 'dbo.Staffs', 'Staff';");

            // 重新建立 FK 指向 Staff 表
            migrationBuilder.AddForeignKey(
                name: "FK_BlobStorage_Staff_uploaded_by",
                table: "BlobStorage",
                column: "uploaded_by",
                principalTable: "Staff",
                principalColumn: "staff_id");
        }
    }
}
