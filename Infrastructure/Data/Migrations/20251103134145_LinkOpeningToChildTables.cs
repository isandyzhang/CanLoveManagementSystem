using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CanLove_Backend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class LinkOpeningToChildTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "opening_id",
                table: "FinalAssessmentSummary",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OpeningId",
                table: "CaseSocialWorkerContent",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "opening_id",
                table: "CaseIQacademicPerformance",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "opening_id",
                table: "CaseHQhealthStatus",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "opening_id",
                table: "CaseFQeconomicStatus",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "opening_id",
                table: "CaseEQemotionalEvaluation",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OpeningId",
                table: "CaseDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CaseOpening",
                columns: table => new
                {
                    opening_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    caseID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    open_date = table.Column<DateOnly>(type: "date", nullable: true),
                    open_reason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Draft"),
                    submitted_by = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    submitted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    reviewed_by = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    review_comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    assigned_staff_id = table.Column<int>(type: "int", nullable: true),
                    is_locked = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    locked_by = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    locked_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseOpening", x => x.opening_id);
                    table.ForeignKey(
                        name: "FK_CaseOpening_Case",
                        column: x => x.caseID,
                        principalTable: "Cases",
                        principalColumn: "caseID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinalAssessmentSummary_opening_id",
                table: "FinalAssessmentSummary",
                column: "opening_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinalAssessmentSummary_opening_id1",
                table: "FinalAssessmentSummary",
                column: "opening_id");

            migrationBuilder.CreateIndex(
                name: "IX_CaseSocialWorkerContent_OpeningId",
                table: "CaseSocialWorkerContent",
                column: "OpeningId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseIQacademicPerformance_opening_id",
                table: "CaseIQacademicPerformance",
                column: "opening_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseIQacademicPerformance_opening_id1",
                table: "CaseIQacademicPerformance",
                column: "opening_id");

            migrationBuilder.CreateIndex(
                name: "IX_CaseHQhealthStatus_opening_id",
                table: "CaseHQhealthStatus",
                column: "opening_id");

            migrationBuilder.CreateIndex(
                name: "IX_CaseFQeconomicStatus_opening_id",
                table: "CaseFQeconomicStatus",
                column: "opening_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseEQemotionalEvaluation_opening_id",
                table: "CaseEQemotionalEvaluation",
                column: "opening_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseEQemotionalEvaluation_opening_id1",
                table: "CaseEQemotionalEvaluation",
                column: "opening_id");

            migrationBuilder.CreateIndex(
                name: "IX_CaseDetail_OpeningId",
                table: "CaseDetail",
                column: "OpeningId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseOpening_Status",
                table: "CaseOpening",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "UQ_CaseOpening_CaseId",
                table: "CaseOpening",
                column: "caseID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CaseDetail_CaseOpening",
                table: "CaseDetail",
                column: "OpeningId",
                principalTable: "CaseOpening",
                principalColumn: "opening_id");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseEQemotionalEvaluation_CaseOpening",
                table: "CaseEQemotionalEvaluation",
                column: "opening_id",
                principalTable: "CaseOpening",
                principalColumn: "opening_id");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseFQeconomicStatus_CaseOpening",
                table: "CaseFQeconomicStatus",
                column: "opening_id",
                principalTable: "CaseOpening",
                principalColumn: "opening_id");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseHQhealthStatus_CaseOpening",
                table: "CaseHQhealthStatus",
                column: "opening_id",
                principalTable: "CaseOpening",
                principalColumn: "opening_id");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseIQacademicPerformance_CaseOpening",
                table: "CaseIQacademicPerformance",
                column: "opening_id",
                principalTable: "CaseOpening",
                principalColumn: "opening_id");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseSocialWorkerContent_CaseOpening",
                table: "CaseSocialWorkerContent",
                column: "OpeningId",
                principalTable: "CaseOpening",
                principalColumn: "opening_id");

            migrationBuilder.AddForeignKey(
                name: "FK_FinalAssessmentSummary_CaseOpening",
                table: "FinalAssessmentSummary",
                column: "opening_id",
                principalTable: "CaseOpening",
                principalColumn: "opening_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseDetail_CaseOpening",
                table: "CaseDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_CaseEQemotionalEvaluation_CaseOpening",
                table: "CaseEQemotionalEvaluation");

            migrationBuilder.DropForeignKey(
                name: "FK_CaseFQeconomicStatus_CaseOpening",
                table: "CaseFQeconomicStatus");

            migrationBuilder.DropForeignKey(
                name: "FK_CaseHQhealthStatus_CaseOpening",
                table: "CaseHQhealthStatus");

            migrationBuilder.DropForeignKey(
                name: "FK_CaseIQacademicPerformance_CaseOpening",
                table: "CaseIQacademicPerformance");

            migrationBuilder.DropForeignKey(
                name: "FK_CaseSocialWorkerContent_CaseOpening",
                table: "CaseSocialWorkerContent");

            migrationBuilder.DropForeignKey(
                name: "FK_FinalAssessmentSummary_CaseOpening",
                table: "FinalAssessmentSummary");

            migrationBuilder.DropTable(
                name: "CaseOpening");

            migrationBuilder.DropIndex(
                name: "IX_FinalAssessmentSummary_opening_id",
                table: "FinalAssessmentSummary");

            migrationBuilder.DropIndex(
                name: "IX_FinalAssessmentSummary_opening_id1",
                table: "FinalAssessmentSummary");

            migrationBuilder.DropIndex(
                name: "IX_CaseSocialWorkerContent_OpeningId",
                table: "CaseSocialWorkerContent");

            migrationBuilder.DropIndex(
                name: "IX_CaseIQacademicPerformance_opening_id",
                table: "CaseIQacademicPerformance");

            migrationBuilder.DropIndex(
                name: "IX_CaseIQacademicPerformance_opening_id1",
                table: "CaseIQacademicPerformance");

            migrationBuilder.DropIndex(
                name: "IX_CaseHQhealthStatus_opening_id",
                table: "CaseHQhealthStatus");

            migrationBuilder.DropIndex(
                name: "IX_CaseFQeconomicStatus_opening_id",
                table: "CaseFQeconomicStatus");

            migrationBuilder.DropIndex(
                name: "IX_CaseEQemotionalEvaluation_opening_id",
                table: "CaseEQemotionalEvaluation");

            migrationBuilder.DropIndex(
                name: "IX_CaseEQemotionalEvaluation_opening_id1",
                table: "CaseEQemotionalEvaluation");

            migrationBuilder.DropIndex(
                name: "IX_CaseDetail_OpeningId",
                table: "CaseDetail");

            migrationBuilder.DropColumn(
                name: "opening_id",
                table: "FinalAssessmentSummary");

            migrationBuilder.DropColumn(
                name: "OpeningId",
                table: "CaseSocialWorkerContent");

            migrationBuilder.DropColumn(
                name: "opening_id",
                table: "CaseIQacademicPerformance");

            migrationBuilder.DropColumn(
                name: "opening_id",
                table: "CaseHQhealthStatus");

            migrationBuilder.DropColumn(
                name: "opening_id",
                table: "CaseFQeconomicStatus");

            migrationBuilder.DropColumn(
                name: "opening_id",
                table: "CaseEQemotionalEvaluation");

            migrationBuilder.DropColumn(
                name: "OpeningId",
                table: "CaseDetail");
        }
    }
}
