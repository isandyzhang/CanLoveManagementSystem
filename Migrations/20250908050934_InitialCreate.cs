using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CanLove_Backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    city_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    city_name = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Cities__031491A82DDA3769", x => x.city_id);
                });

            migrationBuilder.CreateTable(
                name: "DataChangeLog",
                columns: table => new
                {
                    logID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    table_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    record_id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    operation_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    field_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    old_value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    changed_by = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    changed_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    ip_address = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DataChan__7839F62D88C723F6", x => x.logID);
                });

            migrationBuilder.CreateTable(
                name: "FamilyStructureTypes",
                columns: table => new
                {
                    structure_type_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    structure_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    structure_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    needs_description = table.Column<bool>(type: "bit", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FamilySt__6BBEBC67EB138E2C", x => x.structure_type_id);
                });

            migrationBuilder.CreateTable(
                name: "Nationalities",
                columns: table => new
                {
                    nationality_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nationality_name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    nationality_code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__National__2E6444ED91C8FBA5", x => x.nationality_id);
                });

            migrationBuilder.CreateTable(
                name: "OptionSets",
                columns: table => new
                {
                    option_set_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    option_key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    option_set_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__OptionSe__2092EE9521649D1A", x => x.option_set_id);
                });

            migrationBuilder.CreateTable(
                name: "Schools",
                columns: table => new
                {
                    school_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    school_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    school_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Schools__27CA6CF491635D7B", x => x.school_id);
                });

            migrationBuilder.CreateTable(
                name: "UserActivityLog",
                columns: table => new
                {
                    activityID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    activity_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    activity_description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    target_table = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    target_record_id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ip_address = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserActi__0FC9CBCC3741A266", x => x.activityID);
                });

            migrationBuilder.CreateTable(
                name: "Districts",
                columns: table => new
                {
                    district_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    district_name = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    city_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__District__2521322B86B5E7A2", x => x.district_id);
                    table.ForeignKey(
                        name: "FK__Districts__city___6E01572D",
                        column: x => x.city_id,
                        principalTable: "Cities",
                        principalColumn: "city_id");
                });

            migrationBuilder.CreateTable(
                name: "OptionSetValues",
                columns: table => new
                {
                    option_value_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    option_set_id = table.Column<int>(type: "int", nullable: false),
                    value_code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    value_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__OptionSe__3AAA210D287E4599", x => x.option_value_id);
                    table.ForeignKey(
                        name: "FK__OptionSet__optio__6477ECF3",
                        column: x => x.option_set_id,
                        principalTable: "OptionSets",
                        principalColumn: "option_set_id");
                });

            migrationBuilder.CreateTable(
                name: "Cases",
                columns: table => new
                {
                    caseID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    assessment_date = table.Column<DateOnly>(type: "date", nullable: true),
                    name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    gender = table.Column<string>(type: "nchar(2)", fixedLength: true, maxLength: 2, nullable: true),
                    school_id = table.Column<int>(type: "int", nullable: true),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: false),
                    id_number = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    address = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    city_id = table.Column<int>(type: "int", nullable: true),
                    district_id = table.Column<int>(type: "int", nullable: true),
                    phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    photo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    draft_status = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    submitted_by = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    submitted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    reviewed_by = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    is_locked = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    locked_by = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    locked_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Cases__956FA6E99085F5FE", x => x.caseID);
                    table.ForeignKey(
                        name: "FK__Cases__city_id__00200768",
                        column: x => x.city_id,
                        principalTable: "Cities",
                        principalColumn: "city_id");
                    table.ForeignKey(
                        name: "FK__Cases__district___01142BA1",
                        column: x => x.district_id,
                        principalTable: "Districts",
                        principalColumn: "district_id");
                    table.ForeignKey(
                        name: "FK__Cases__school_id__7F2BE32F",
                        column: x => x.school_id,
                        principalTable: "Schools",
                        principalColumn: "school_id");
                });

            migrationBuilder.CreateTable(
                name: "CaseConsultationRecords",
                columns: table => new
                {
                    consultation_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    caseID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    consultation_method_value_id = table.Column<int>(type: "int", nullable: false),
                    consultation_target_value_id = table.Column<int>(type: "int", nullable: false),
                    consultation_datetime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    consultation_content = table.Column<string>(type: "ntext", nullable: true),
                    deleted = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CaseCons__650FE0FB57E0285C", x => x.consultation_id);
                    table.ForeignKey(
                        name: "FK__CaseConsu__caseI__5BAD9CC8",
                        column: x => x.caseID,
                        principalTable: "Cases",
                        principalColumn: "caseID");
                    table.ForeignKey(
                        name: "FK__CaseConsu__consu__5CA1C101",
                        column: x => x.consultation_method_value_id,
                        principalTable: "OptionSetValues",
                        principalColumn: "option_value_id");
                    table.ForeignKey(
                        name: "FK__CaseConsu__consu__5D95E53A",
                        column: x => x.consultation_target_value_id,
                        principalTable: "OptionSetValues",
                        principalColumn: "option_value_id");
                });

            migrationBuilder.CreateTable(
                name: "CaseDetail",
                columns: table => new
                {
                    caseID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    contact_name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    contact_relation_value_id = table.Column<int>(type: "int", nullable: true),
                    contact_phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    home_phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    family_structure_type_id = table.Column<int>(type: "int", nullable: true),
                    family_structure_other_desc = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    parent_nation_father_id = table.Column<int>(type: "int", nullable: true),
                    parent_nation_mother_id = table.Column<int>(type: "int", nullable: true),
                    main_caregiver_name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    main_caregiver_relation = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    main_caregiver_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    main_caregiver_birth = table.Column<DateOnly>(type: "date", nullable: true),
                    main_caregiver_job = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    main_caregiver_marry_status_value_id = table.Column<int>(type: "int", nullable: true),
                    main_caregiver_edu_value_id = table.Column<int>(type: "int", nullable: true),
                    source_value_id = table.Column<int>(type: "int", nullable: true),
                    help_experience_value_id = table.Column<int>(type: "int", nullable: true),
                    note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    deleted = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CaseDeta__956FA6E927C7051B", x => x.caseID);
                    table.ForeignKey(
                        name: "FK__CaseDetai__caseI__06CD04F7",
                        column: x => x.caseID,
                        principalTable: "Cases",
                        principalColumn: "caseID");
                    table.ForeignKey(
                        name: "FK__CaseDetai__conta__07C12930",
                        column: x => x.contact_relation_value_id,
                        principalTable: "OptionSetValues",
                        principalColumn: "option_value_id");
                    table.ForeignKey(
                        name: "FK__CaseDetai__famil__08B54D69",
                        column: x => x.family_structure_type_id,
                        principalTable: "FamilyStructureTypes",
                        principalColumn: "structure_type_id");
                    table.ForeignKey(
                        name: "FK__CaseDetai__help___0E6E26BF",
                        column: x => x.help_experience_value_id,
                        principalTable: "OptionSetValues",
                        principalColumn: "option_value_id");
                    table.ForeignKey(
                        name: "FK__CaseDetai__main___0B91BA14",
                        column: x => x.main_caregiver_marry_status_value_id,
                        principalTable: "OptionSetValues",
                        principalColumn: "option_value_id");
                    table.ForeignKey(
                        name: "FK__CaseDetai__main___0C85DE4D",
                        column: x => x.main_caregiver_edu_value_id,
                        principalTable: "OptionSetValues",
                        principalColumn: "option_value_id");
                    table.ForeignKey(
                        name: "FK__CaseDetai__paren__09A971A2",
                        column: x => x.parent_nation_father_id,
                        principalTable: "Nationalities",
                        principalColumn: "nationality_id");
                    table.ForeignKey(
                        name: "FK__CaseDetai__paren__0A9D95DB",
                        column: x => x.parent_nation_mother_id,
                        principalTable: "Nationalities",
                        principalColumn: "nationality_id");
                    table.ForeignKey(
                        name: "FK__CaseDetai__sourc__0D7A0286",
                        column: x => x.source_value_id,
                        principalTable: "OptionSetValues",
                        principalColumn: "option_value_id");
                });

            migrationBuilder.CreateTable(
                name: "CaseDetailHistory",
                columns: table => new
                {
                    history_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    caseID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    version_number = table.Column<int>(type: "int", nullable: false),
                    change_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    field_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    old_value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    change_reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    changed_by = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    changed_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    ip_address = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CaseDeta__096AA2E9A69DB73B", x => x.history_id);
                    table.ForeignKey(
                        name: "FK__CaseDetai__caseI__6BE40491",
                        column: x => x.caseID,
                        principalTable: "Cases",
                        principalColumn: "caseID");
                });

            migrationBuilder.CreateTable(
                name: "CaseEQemotionalEvaluation",
                columns: table => new
                {
                    caseID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    eq_q1 = table.Column<int>(type: "int", nullable: true),
                    eq_q2 = table.Column<int>(type: "int", nullable: true),
                    eq_q3 = table.Column<int>(type: "int", nullable: true),
                    eq_q4 = table.Column<int>(type: "int", nullable: true),
                    eq_q5 = table.Column<int>(type: "int", nullable: true),
                    eq_q6 = table.Column<int>(type: "int", nullable: true),
                    eq_q7 = table.Column<int>(type: "int", nullable: true),
                    deleted = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CaseEQem__956FA6E91D9170DF", x => x.caseID);
                    table.ForeignKey(
                        name: "FK__CaseEQemo__caseI__3E1D39E1",
                        column: x => x.caseID,
                        principalTable: "Cases",
                        principalColumn: "caseID");
                });

            migrationBuilder.CreateTable(
                name: "CaseFamilyMemberNotes",
                columns: table => new
                {
                    note_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    caseID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    member_type_value_id = table.Column<int>(type: "int", nullable: false),
                    member_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    note_content = table.Column<string>(type: "ntext", nullable: true),
                    deleted = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CaseFami__CEDD0FA4B2CD6ED0", x => x.note_id);
                    table.ForeignKey(
                        name: "FK__CaseFamil__caseI__55009F39",
                        column: x => x.caseID,
                        principalTable: "Cases",
                        principalColumn: "caseID");
                    table.ForeignKey(
                        name: "FK__CaseFamil__membe__55F4C372",
                        column: x => x.member_type_value_id,
                        principalTable: "OptionSetValues",
                        principalColumn: "option_value_id");
                });

            migrationBuilder.CreateTable(
                name: "CaseFamilyMembers",
                columns: table => new
                {
                    caseID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    member_type_value_id = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: true),
                    description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    deleted = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK__CaseFamil__caseI__4E53A1AA",
                        column: x => x.caseID,
                        principalTable: "Cases",
                        principalColumn: "caseID");
                    table.ForeignKey(
                        name: "FK__CaseFamil__membe__4F47C5E3",
                        column: x => x.member_type_value_id,
                        principalTable: "OptionSetValues",
                        principalColumn: "option_value_id");
                });

            migrationBuilder.CreateTable(
                name: "CaseFamilySpecialStatus",
                columns: table => new
                {
                    caseID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    status_type_value_id = table.Column<int>(type: "int", nullable: false),
                    low_income_card_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    disability_icf_code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    other_description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    deleted = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK__CaseFamil__caseI__489AC854",
                        column: x => x.caseID,
                        principalTable: "Cases",
                        principalColumn: "caseID");
                    table.ForeignKey(
                        name: "FK__CaseFamil__statu__498EEC8D",
                        column: x => x.status_type_value_id,
                        principalTable: "OptionSetValues",
                        principalColumn: "option_value_id");
                });

            migrationBuilder.CreateTable(
                name: "CaseFQeconomicStatus",
                columns: table => new
                {
                    caseID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    economic_overview = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    work_situation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    civil_welfare_resources = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    monthly_income = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    monthly_expense = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    monthly_expense_note = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    deleted = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CaseFQec__956FA6E98584FE85", x => x.caseID);
                    table.ForeignKey(
                        name: "FK__CaseFQeco__caseI__208CD6FA",
                        column: x => x.caseID,
                        principalTable: "Cases",
                        principalColumn: "caseID");
                });

            migrationBuilder.CreateTable(
                name: "CaseHistory",
                columns: table => new
                {
                    history_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    caseID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    version_number = table.Column<int>(type: "int", nullable: false),
                    change_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    field_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    old_value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    change_reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    changed_by = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    changed_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    ip_address = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CaseHist__096AA2E9EAA0545B", x => x.history_id);
                    table.ForeignKey(
                        name: "FK__CaseHisto__caseI__681373AD",
                        column: x => x.caseID,
                        principalTable: "Cases",
                        principalColumn: "caseID");
                });

            migrationBuilder.CreateTable(
                name: "CaseHQhealthStatus",
                columns: table => new
                {
                    caregiver_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    caseID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    caregiver_role_value_id = table.Column<int>(type: "int", nullable: false),
                    caregiver_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    is_primary = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    emotional_expression_rating = table.Column<byte>(type: "tinyint", nullable: true),
                    emotional_expression_note = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    health_status_rating = table.Column<byte>(type: "tinyint", nullable: true),
                    health_status_note = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    child_health_status_rating = table.Column<byte>(type: "tinyint", nullable: true),
                    child_health_status_note = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    child_care_status_rating = table.Column<byte>(type: "tinyint", nullable: true),
                    child_care_status_note = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    deleted = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CaseHQhe__F6A63A40A924AA99", x => x.caregiver_id);
                    table.ForeignKey(
                        name: "FK__CaseHQhea__careg__2BFE89A6",
                        column: x => x.caregiver_role_value_id,
                        principalTable: "OptionSetValues",
                        principalColumn: "option_value_id");
                    table.ForeignKey(
                        name: "FK__CaseHQhea__caseI__2B0A656D",
                        column: x => x.caseID,
                        principalTable: "Cases",
                        principalColumn: "caseID");
                });

            migrationBuilder.CreateTable(
                name: "CaseIQacademicPerformance",
                columns: table => new
                {
                    caseID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    academic_performance_summary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    deleted = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CaseIQac__956FA6E94231541A", x => x.caseID);
                    table.ForeignKey(
                        name: "FK__CaseIQaca__caseI__31B762FC",
                        column: x => x.caseID,
                        principalTable: "Cases",
                        principalColumn: "caseID");
                });

            migrationBuilder.CreateTable(
                name: "CaseSocialWorkerContent",
                columns: table => new
                {
                    caseID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    family_tree_img = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    residence_type_value_id = table.Column<int>(type: "int", nullable: true),
                    house_cleanliness_rating = table.Column<byte>(type: "tinyint", nullable: true),
                    house_cleanliness_note = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    house_safety_rating = table.Column<byte>(type: "tinyint", nullable: true),
                    house_safety_note = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    caregiver_child_interaction_rating = table.Column<byte>(type: "tinyint", nullable: true),
                    caregiver_child_interaction_note = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    caregiver_family_interaction_rating = table.Column<byte>(type: "tinyint", nullable: true),
                    caregiver_family_interaction_note = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    family_resource_ability_rating = table.Column<byte>(type: "tinyint", nullable: true),
                    family_resource_ability_note = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    family_social_support_rating = table.Column<byte>(type: "tinyint", nullable: true),
                    family_social_support_note = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    special_circumstances_description = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    deleted = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CaseSoci__956FA6E99FF2C225", x => x.caseID);
                    table.ForeignKey(
                        name: "FK__CaseSocia__caseI__19DFD96B",
                        column: x => x.caseID,
                        principalTable: "Cases",
                        principalColumn: "caseID");
                    table.ForeignKey(
                        name: "FK__CaseSocia__resid__1AD3FDA4",
                        column: x => x.residence_type_value_id,
                        principalTable: "OptionSetValues",
                        principalColumn: "option_value_id");
                });

            migrationBuilder.CreateTable(
                name: "CaseSocialWorkerServices",
                columns: table => new
                {
                    service_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    caseID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    service_type_value_id = table.Column<int>(type: "int", nullable: false),
                    service_description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    service_date = table.Column<DateOnly>(type: "date", nullable: true),
                    service_provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    deleted = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CaseSoci__3E0DB8AFBE5651FA", x => x.service_id);
                    table.ForeignKey(
                        name: "FK__CaseSocia__caseI__634EBE90",
                        column: x => x.caseID,
                        principalTable: "Cases",
                        principalColumn: "caseID");
                    table.ForeignKey(
                        name: "FK__CaseSocia__servi__6442E2C9",
                        column: x => x.service_type_value_id,
                        principalTable: "OptionSetValues",
                        principalColumn: "option_value_id");
                });

            migrationBuilder.CreateTable(
                name: "FinalAssessmentSummary",
                columns: table => new
                {
                    caseID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    fq_summary = table.Column<string>(type: "text", nullable: true),
                    hq_summary = table.Column<string>(type: "text", nullable: true),
                    iq_summary = table.Column<string>(type: "text", nullable: true),
                    eq_summary = table.Column<string>(type: "text", nullable: true),
                    deleted = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FinalAss__956FA6E9E622ECA0", x => x.caseID);
                    table.ForeignKey(
                        name: "FK__FinalAsse__caseI__43D61337",
                        column: x => x.caseID,
                        principalTable: "Cases",
                        principalColumn: "caseID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaseConsultationRecords_caseID",
                table: "CaseConsultationRecords",
                column: "caseID");

            migrationBuilder.CreateIndex(
                name: "IX_CaseConsultationRecords_consultation_datetime",
                table: "CaseConsultationRecords",
                column: "consultation_datetime");

            migrationBuilder.CreateIndex(
                name: "IX_CaseConsultationRecords_consultation_method_value_id",
                table: "CaseConsultationRecords",
                column: "consultation_method_value_id");

            migrationBuilder.CreateIndex(
                name: "IX_CaseConsultationRecords_consultation_target_value_id",
                table: "CaseConsultationRecords",
                column: "consultation_target_value_id");

            migrationBuilder.CreateIndex(
                name: "IX_CaseDetail_caseID",
                table: "CaseDetail",
                column: "caseID");

            migrationBuilder.CreateIndex(
                name: "IX_CaseDetail_contact_relation_value_id",
                table: "CaseDetail",
                column: "contact_relation_value_id");

            migrationBuilder.CreateIndex(
                name: "IX_CaseDetail_family_structure_type_id",
                table: "CaseDetail",
                column: "family_structure_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_CaseDetail_help_experience_value_id",
                table: "CaseDetail",
                column: "help_experience_value_id");

            migrationBuilder.CreateIndex(
                name: "IX_CaseDetail_main_caregiver_edu_value_id",
                table: "CaseDetail",
                column: "main_caregiver_edu_value_id");

            migrationBuilder.CreateIndex(
                name: "IX_CaseDetail_main_caregiver_marry_status_value_id",
                table: "CaseDetail",
                column: "main_caregiver_marry_status_value_id");

            migrationBuilder.CreateIndex(
                name: "IX_CaseDetail_parent_nation_father_id",
                table: "CaseDetail",
                column: "parent_nation_father_id");

            migrationBuilder.CreateIndex(
                name: "IX_CaseDetail_parent_nation_mother_id",
                table: "CaseDetail",
                column: "parent_nation_mother_id");

            migrationBuilder.CreateIndex(
                name: "IX_CaseDetail_source_value_id",
                table: "CaseDetail",
                column: "source_value_id");

            migrationBuilder.CreateIndex(
                name: "IX_CaseDetailHistory_caseID",
                table: "CaseDetailHistory",
                column: "caseID");

            migrationBuilder.CreateIndex(
                name: "IX_CaseEQemotionalEvaluation_caseID",
                table: "CaseEQemotionalEvaluation",
                column: "caseID");

            migrationBuilder.CreateIndex(
                name: "IX_CaseFamilyMemberNotes_caseID",
                table: "CaseFamilyMemberNotes",
                column: "caseID");

            migrationBuilder.CreateIndex(
                name: "IX_CaseFamilyMemberNotes_member_type_value_id",
                table: "CaseFamilyMemberNotes",
                column: "member_type_value_id");

            migrationBuilder.CreateIndex(
                name: "IX_CaseFamilyMembers_caseID",
                table: "CaseFamilyMembers",
                column: "caseID");

            migrationBuilder.CreateIndex(
                name: "IX_CaseFamilyMembers_member_type_value_id",
                table: "CaseFamilyMembers",
                column: "member_type_value_id");

            migrationBuilder.CreateIndex(
                name: "IX_CaseFamilySpecialStatus_caseID",
                table: "CaseFamilySpecialStatus",
                column: "caseID");

            migrationBuilder.CreateIndex(
                name: "IX_CaseFamilySpecialStatus_status_type_value_id",
                table: "CaseFamilySpecialStatus",
                column: "status_type_value_id");

            migrationBuilder.CreateIndex(
                name: "IX_CaseFQeconomicStatus_caseID",
                table: "CaseFQeconomicStatus",
                column: "caseID");

            migrationBuilder.CreateIndex(
                name: "IX_CaseHistory_caseID",
                table: "CaseHistory",
                column: "caseID");

            migrationBuilder.CreateIndex(
                name: "IX_CaseHQhealthStatus_caregiver_role_value_id",
                table: "CaseHQhealthStatus",
                column: "caregiver_role_value_id");

            migrationBuilder.CreateIndex(
                name: "IX_CaseHQhealthStatus_caseID",
                table: "CaseHQhealthStatus",
                column: "caseID");

            migrationBuilder.CreateIndex(
                name: "IX_CaseIQacademicPerformance_caseID",
                table: "CaseIQacademicPerformance",
                column: "caseID");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_assessment_date",
                table: "Cases",
                column: "assessment_date");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_city_id",
                table: "Cases",
                column: "city_id");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_district_id",
                table: "Cases",
                column: "district_id");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_draft_status",
                table: "Cases",
                column: "draft_status");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_school_id",
                table: "Cases",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_submitted_by",
                table: "Cases",
                column: "submitted_by");

            migrationBuilder.CreateIndex(
                name: "UQ__Cases__D58CDE11C0544CB6",
                table: "Cases",
                column: "id_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseSocialWorkerContent_residence_type_value_id",
                table: "CaseSocialWorkerContent",
                column: "residence_type_value_id");

            migrationBuilder.CreateIndex(
                name: "IX_CaseSocialWorkerServices_caseID",
                table: "CaseSocialWorkerServices",
                column: "caseID");

            migrationBuilder.CreateIndex(
                name: "IX_CaseSocialWorkerServices_service_type_value_id",
                table: "CaseSocialWorkerServices",
                column: "service_type_value_id");

            migrationBuilder.CreateIndex(
                name: "UQ__Cities__1AA4F7B590443F24",
                table: "Cities",
                column: "city_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Districts_city_id",
                table: "Districts",
                column: "city_id");

            migrationBuilder.CreateIndex(
                name: "UQ__District__9E05AFF9CEDD6473",
                table: "Districts",
                column: "district_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__FamilySt__84CB930AE64C5F03",
                table: "FamilyStructureTypes",
                column: "structure_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinalAssessmentSummary_caseID",
                table: "FinalAssessmentSummary",
                column: "caseID");

            migrationBuilder.CreateIndex(
                name: "UQ__National__64AF10BC33C40389",
                table: "Nationalities",
                column: "nationality_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__National__A22D2FE5AD603ECB",
                table: "Nationalities",
                column: "nationality_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__OptionSe__C54F5DD694F6E72D",
                table: "OptionSets",
                column: "option_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OptionSetValues_option_set_id",
                table: "OptionSetValues",
                column: "option_set_id");

            migrationBuilder.CreateIndex(
                name: "UQ__Schools__007188191EFA6D19",
                table: "Schools",
                column: "school_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaseConsultationRecords");

            migrationBuilder.DropTable(
                name: "CaseDetail");

            migrationBuilder.DropTable(
                name: "CaseDetailHistory");

            migrationBuilder.DropTable(
                name: "CaseEQemotionalEvaluation");

            migrationBuilder.DropTable(
                name: "CaseFamilyMemberNotes");

            migrationBuilder.DropTable(
                name: "CaseFamilyMembers");

            migrationBuilder.DropTable(
                name: "CaseFamilySpecialStatus");

            migrationBuilder.DropTable(
                name: "CaseFQeconomicStatus");

            migrationBuilder.DropTable(
                name: "CaseHistory");

            migrationBuilder.DropTable(
                name: "CaseHQhealthStatus");

            migrationBuilder.DropTable(
                name: "CaseIQacademicPerformance");

            migrationBuilder.DropTable(
                name: "CaseSocialWorkerContent");

            migrationBuilder.DropTable(
                name: "CaseSocialWorkerServices");

            migrationBuilder.DropTable(
                name: "DataChangeLog");

            migrationBuilder.DropTable(
                name: "FinalAssessmentSummary");

            migrationBuilder.DropTable(
                name: "UserActivityLog");

            migrationBuilder.DropTable(
                name: "FamilyStructureTypes");

            migrationBuilder.DropTable(
                name: "Nationalities");

            migrationBuilder.DropTable(
                name: "OptionSetValues");

            migrationBuilder.DropTable(
                name: "Cases");

            migrationBuilder.DropTable(
                name: "OptionSets");

            migrationBuilder.DropTable(
                name: "Districts");

            migrationBuilder.DropTable(
                name: "Schools");

            migrationBuilder.DropTable(
                name: "Cities");
        }
    }
}
