using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CanLove_Backend.Migrations
{
    /// <inheritdoc />
    public partial class FixPhotoBlobIdColumnType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 先將所有非數值的值設為 NULL（如果有字串值）
            // 注意：如果 photo_blob_id 目前是 nvarchar，需要先清理非數值資料
            migrationBuilder.Sql(@"
                UPDATE Cases 
                SET photo_blob_id = NULL 
                WHERE photo_blob_id IS NOT NULL 
                AND (ISNUMERIC(photo_blob_id) = 0 OR TRY_CAST(photo_blob_id AS INT) IS NULL)
            ");
            
            // 檢查並刪除索引（如果存在）
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Cases_photo_blob_id' AND object_id = OBJECT_ID('Cases'))
                BEGIN
                    DROP INDEX [IX_Cases_photo_blob_id] ON [Cases];
                END
            ");
            
            // 刪除預設約束（如果存在）
            migrationBuilder.Sql(@"
                DECLARE @constraintName sysname;
                SELECT @constraintName = [d].[name]
                FROM [sys].[default_constraints] [d]
                INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cases]') AND [c].[name] = N'photo_blob_id');
                IF @constraintName IS NOT NULL 
                BEGIN
                    EXEC(N'ALTER TABLE [Cases] DROP CONSTRAINT [' + @constraintName + '];');
                END
            ");
            
            // 直接使用 SQL 修改欄位型別（避免 EF Core 自動處理索引）
            migrationBuilder.Sql(@"
                ALTER TABLE [Cases] ALTER COLUMN [photo_blob_id] int NULL;
            ");
            
            // 重新建立索引（如果需要）
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Cases_photo_blob_id' AND object_id = OBJECT_ID('Cases'))
                BEGIN
                    CREATE INDEX [IX_Cases_photo_blob_id] ON [Cases] ([photo_blob_id]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 回滾：將 int 改回 nvarchar(100)
            migrationBuilder.AlterColumn<string>(
                name: "photo_blob_id",
                table: "Cases",
                type: "nvarchar(100)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
