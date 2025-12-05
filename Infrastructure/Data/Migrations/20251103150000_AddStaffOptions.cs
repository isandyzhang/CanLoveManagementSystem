using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CanLove_Backend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // OptionSets: STAFF_DEPARTMENT, STAFF_JOB_TITLE
            migrationBuilder.InsertData(
                table: "OptionSets",
                columns: new[] { "option_set_id", "option_key", "option_set_name" },
                values: new object[,]
                {
                    { 5000, "STAFF_DEPARTMENT", "部門" },
                    { 5001, "STAFF_JOB_TITLE", "職稱" }
                }
            );

            // OptionSetValues for STAFF_DEPARTMENT (set_id = 5000)
            migrationBuilder.InsertData(
                table: "OptionSetValues",
                columns: new[] { "option_value_id", "option_set_id", "value_code", "value_name" },
                values: new object[,]
                {
                    { 5100, 5000, "DEPT_SOCIAL_ADMIN", "社會行政部" },
                    { 5101, 5000, "DEPT_SOCIAL_RESOURCE", "社會資源部" },
                    { 5102, 5000, "DEPT_SOCIAL_WORK", "社會工作部" },
                    { 5103, 5000, "DEPT_SECRETARIAT", "秘書處" }
                }
            );

            // OptionSetValues for STAFF_JOB_TITLE (set_id = 5001)
            migrationBuilder.InsertData(
                table: "OptionSetValues",
                columns: new[] { "option_value_id", "option_set_id", "value_code", "value_name" },
                values: new object[,]
                {
                    { 5200, 5001, "JOB_SOCIAL_WORKER", "社工人員" },
                    { 5201, 5001, "JOB_ADMIN", "管理員" },
                    { 5202, 5001, "JOB_SOCIAL_AFFAIRS", "社政人員" },
                    { 5203, 5001, "JOB_SECRETARY_GENERAL", "秘書長" },
                    { 5204, 5001, "JOB_DIVERSE", "多元人員" },
                    { 5205, 5001, "JOB_RESOURCE_STAFF", "社資人員" }
                }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Delete OptionSetValues
            migrationBuilder.DeleteData(table: "OptionSetValues", keyColumn: "option_value_id", keyValue: 5100);
            migrationBuilder.DeleteData(table: "OptionSetValues", keyColumn: "option_value_id", keyValue: 5101);
            migrationBuilder.DeleteData(table: "OptionSetValues", keyColumn: "option_value_id", keyValue: 5102);
            migrationBuilder.DeleteData(table: "OptionSetValues", keyColumn: "option_value_id", keyValue: 5103);

            migrationBuilder.DeleteData(table: "OptionSetValues", keyColumn: "option_value_id", keyValue: 5200);
            migrationBuilder.DeleteData(table: "OptionSetValues", keyColumn: "option_value_id", keyValue: 5201);
            migrationBuilder.DeleteData(table: "OptionSetValues", keyColumn: "option_value_id", keyValue: 5202);
            migrationBuilder.DeleteData(table: "OptionSetValues", keyColumn: "option_value_id", keyValue: 5203);
            migrationBuilder.DeleteData(table: "OptionSetValues", keyColumn: "option_value_id", keyValue: 5204);
            migrationBuilder.DeleteData(table: "OptionSetValues", keyColumn: "option_value_id", keyValue: 5205);

            // Delete OptionSets
            migrationBuilder.DeleteData(table: "OptionSets", keyColumn: "option_set_id", keyValue: 5000);
            migrationBuilder.DeleteData(table: "OptionSets", keyColumn: "option_set_id", keyValue: 5001);
        }
    }
}
