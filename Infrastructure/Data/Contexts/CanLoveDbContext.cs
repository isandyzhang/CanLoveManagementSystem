using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using CanLove_Backend.Domain.Case.Models.Basic;
using CanLove_Backend.Domain.Case.Models.Opening;
using CanLove_Backend.Domain.Case.Shared.Models;
using CanLove_Backend.Infrastructure.Options.Models;
using CanLove_Backend.Infrastructure.Data.History;
using CanLove_Backend.Infrastructure.Data.Audit;
using CanLove_Backend.Domain.Staff.Models;
using CanLove_Backend.Infrastructure.Storage.Blob;

namespace CanLove_Backend.Infrastructure.Data.Contexts;

public partial class CanLoveDbContext : DbContext
{
    public CanLoveDbContext()
    {
    }

    public CanLoveDbContext(DbContextOptions<CanLoveDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Case> Cases { get; set; }

    public virtual DbSet<CaseConsultationRecord> CaseConsultationRecords { get; set; }

    public virtual DbSet<CaseDetail> CaseDetails { get; set; }

    public virtual DbSet<CaseOpening> CaseOpenings { get; set; }

    public virtual DbSet<CaseDetailHistory> CaseDetailHistories { get; set; }

    public virtual DbSet<CaseEqemotionalEvaluation> CaseEqemotionalEvaluations { get; set; }

    public virtual DbSet<CaseFamilyMember> CaseFamilyMembers { get; set; }

    public virtual DbSet<CaseFamilyMemberNote> CaseFamilyMemberNotes { get; set; }

    public virtual DbSet<CaseFamilySpecialStatus> CaseFamilySpecialStatuses { get; set; }

    public virtual DbSet<CaseFqeconomicStatus> CaseFqeconomicStatuses { get; set; }

    public virtual DbSet<CaseHistory> CaseHistories { get; set; }

    public virtual DbSet<CaseHqhealthStatus> CaseHqhealthStatuses { get; set; }

    public virtual DbSet<CaseIqacademicPerformance> CaseIqacademicPerformances { get; set; }

    public virtual DbSet<CaseSocialWorkerContent> CaseSocialWorkerContents { get; set; }

    public virtual DbSet<CaseSocialWorkerService> CaseSocialWorkerServices { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<DataChangeLog> DataChangeLogs { get; set; }

    public virtual DbSet<District> Districts { get; set; }

    public virtual DbSet<FamilyStructureType> FamilyStructureTypes { get; set; }

    public virtual DbSet<FinalAssessmentSummary> FinalAssessmentSummaries { get; set; }

    public virtual DbSet<Nationality> Nationalities { get; set; }

    public virtual DbSet<OptionSet> OptionSets { get; set; }

    public virtual DbSet<OptionSetValue> OptionSetValues { get; set; }

    public virtual DbSet<School> Schools { get; set; }

    public virtual DbSet<UserActivityLog> UserActivityLogs { get; set; }

    public virtual DbSet<Staff> Staffs { get; set; }

    public virtual DbSet<BlobStorage> BlobStorage { get; set; }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    if (!optionsBuilder.IsConfigured)
    {
        // 留空，交由 Program.cs 注入
    }
}
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Chinese_PRC_CI_AS");

        // ============================================================================
        // 實體配置說明
        // ============================================================================
        // 目前使用內聯配置（下方的 modelBuilder.Entity<T> 區塊）
        // 
        // 建議的優化方式：使用 IEntityTypeConfiguration<T> 將配置拆分到獨立文件
        // 範例配置已建立在 Infrastructure/Data/Configurations/ 目錄
        // 
        // 啟用方式（未來遷移時）：
        // 1. 將下方的內聯配置移到對應的 Configuration 類別
        // 2. 取消下行的註解，自動載入所有配置
        // modelBuilder.ApplyConfigurationsFromAssembly(typeof(CanLoveDbContext).Assembly);
        // ============================================================================

        modelBuilder.Entity<Case>(entity =>
        {
            entity.HasKey(e => e.CaseId).HasName("PK__Cases__956FA6E99085F5FE");

            entity.ToTable(tb => tb.HasTrigger("TR_Cases_UpdateTime"));

            entity.HasIndex(e => e.AssessmentDate, "IX_Cases_assessment_date");

            entity.HasIndex(e => e.Status, "IX_Cases_status");

            entity.HasIndex(e => e.SubmittedBy, "IX_Cases_submitted_by");

            entity.HasIndex(e => e.IdNumber, "UQ__Cases__D58CDE11C0544CB6").IsUnique();

            entity.Property(e => e.CaseId)
                .HasMaxLength(10)
                .HasColumnName("caseID");
            entity.Property(e => e.Address)
                .HasMaxLength(100)
                .HasColumnName("address");
            entity.Property(e => e.AssessmentDate).HasColumnName("assessment_date");
            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.CityId)
                .HasColumnName("city_id")
                .HasColumnType("int");
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");
            entity.Property(e => e.Deleted)
                .HasDefaultValue(false)
                .HasColumnName("deleted");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy)
                .HasMaxLength(30)
                .HasColumnName("deleted_by");
            entity.Property(e => e.DistrictId)
                .HasColumnName("district_id")
                .HasColumnType("int");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("PendingReview")
                .HasColumnName("status");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("email");
            entity.Property(e => e.Gender)
                .HasMaxLength(30)
                .HasColumnName("gender");
            entity.Property(e => e.IdNumber)
                .HasMaxLength(255)
                .HasColumnName("id_number");
            entity.Property(e => e.IsLocked)
                .HasDefaultValue(false)
                .HasColumnName("is_locked");
            entity.Property(e => e.LockedAt).HasColumnName("locked_at");
            entity.Property(e => e.LockedBy)
                .HasMaxLength(30)
                .HasColumnName("locked_by");
            entity.Property(e => e.Name)
                .HasMaxLength(20)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .HasColumnName("phone");
            // Photo 欄位已棄用，改為使用 PhotoBlobId
            entity.Ignore(e => e.Photo);
            entity.Property(e => e.PhotoBlobId)
                .HasColumnName("photo_blob_id")
                .HasColumnType("int");
            entity.Property(e => e.ReviewedAt).HasColumnName("reviewed_at");
            entity.Property(e => e.ReviewedBy)
                .HasMaxLength(30)
                .HasColumnName("reviewed_by");
            entity.Property(e => e.SchoolId)
                .HasColumnName("school_id")
                .HasColumnType("int");
            entity.Property(e => e.SubmittedAt).HasColumnName("submitted_at");
            entity.Property(e => e.SubmittedBy)
                .HasMaxLength(30)
                .HasColumnName("submitted_by");
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.HasOne(d => d.City).WithMany(p => p.Cases)
                .HasForeignKey(d => d.CityId)
                .HasConstraintName("FK__Cases__city_id__00200768");

            entity.HasOne(d => d.District).WithMany(p => p.Cases)
                .HasForeignKey(d => d.DistrictId)
                .HasConstraintName("FK__Cases__district___01142BA1");

            entity.HasOne(d => d.School).WithMany(p => p.Cases)
                .HasForeignKey(d => d.SchoolId)
                .HasConstraintName("FK__Cases__school_id__7F2BE32F");

            entity.HasOne(d => d.PhotoBlob).WithMany()
                .HasForeignKey(d => d.PhotoBlobId)
                .HasConstraintName("FK_Cases_PhotoBlob");

            // 全局查詢過濾器：自動排除已刪除的記錄
            entity.HasQueryFilter(e => e.Deleted != true);
        });

        modelBuilder.Entity<CaseOpening>(entity =>
        {
            entity.HasKey(e => e.OpeningId);

            entity.ToTable("CaseOpening", tb => tb.HasTrigger("TR_CaseOpening_UpdateTime"));

            entity.HasIndex(e => e.CaseId, "UQ_CaseOpening_CaseId").IsUnique();
            entity.HasIndex(e => e.Status, "IX_CaseOpening_Status");

            entity.Property(e => e.OpeningId).HasColumnName("opening_id");
            entity.Property(e => e.CaseId).HasMaxLength(10).HasColumnName("caseID");
            entity.Property(e => e.OpenDate).HasColumnName("open_date");
            entity.Property(e => e.OpenReason).HasMaxLength(200).HasColumnName("open_reason");
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Draft").HasColumnName("status");
            entity.Property(e => e.SubmittedBy).HasMaxLength(30).HasColumnName("submitted_by");
            entity.Property(e => e.SubmittedAt).HasColumnName("submitted_at");
            entity.Property(e => e.ReviewedBy).HasMaxLength(30).HasColumnName("reviewed_by");
            entity.Property(e => e.ReviewedAt).HasColumnName("reviewed_at");
            entity.Property(e => e.ReviewComment).HasMaxLength(500).HasColumnName("review_comment");
            entity.Property(e => e.AssignedStaffId).HasColumnName("assigned_staff_id");
            entity.Property(e => e.IsLocked).HasDefaultValue(false).HasColumnName("is_locked");
            entity.Property(e => e.LockedBy).HasMaxLength(30).HasColumnName("locked_by");
            entity.Property(e => e.LockedAt).HasColumnName("locked_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Case).WithOne()
                .HasForeignKey<CaseOpening>(d => d.CaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CaseOpening_Case");
        });


        modelBuilder.Entity<CaseConsultationRecord>(entity =>
        {
            entity.HasKey(e => e.ConsultationId).HasName("PK__CaseCons__650FE0FB57E0285C");

            entity.HasIndex(e => e.ConsultationDatetime, "IX_CaseConsultationRecords_consultation_datetime");

            entity.Property(e => e.ConsultationId).HasColumnName("consultation_id");
            entity.Property(e => e.CaseId)
                .HasMaxLength(10)
                .HasColumnName("caseID");
            entity.Property(e => e.ConsultationContent)
                .HasColumnType("ntext")
                .HasColumnName("consultation_content");
            entity.Property(e => e.ConsultationDatetime).HasColumnName("consultation_datetime");
            entity.Property(e => e.ConsultationMethodValueId).HasColumnName("consultation_method_value_id");
            entity.Property(e => e.ConsultationTargetValueId).HasColumnName("consultation_target_value_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");
            entity.Property(e => e.Deleted)
                .HasDefaultValue(false)
                .HasColumnName("deleted");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy)
                .HasMaxLength(30)
                .HasColumnName("deleted_by");
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Case).WithMany(p => p.CaseConsultationRecords)
                .HasForeignKey(d => d.CaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CaseConsu__caseI__5BAD9CC8");

            entity.HasOne(d => d.ConsultationMethodValue).WithMany(p => p.CaseConsultationRecordConsultationMethodValues)
                .HasForeignKey(d => d.ConsultationMethodValueId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CaseConsu__consu__5CA1C101");

            entity.HasOne(d => d.ConsultationTargetValue).WithMany(p => p.CaseConsultationRecordConsultationTargetValues)
                .HasForeignKey(d => d.ConsultationTargetValueId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CaseConsu__consu__5D95E53A");

            // 全局查詢過濾器：自動排除已刪除的記錄
            entity.HasQueryFilter(e => e.Deleted != true);
        });

        modelBuilder.Entity<CaseDetail>(entity =>
        {
            entity.HasKey(e => e.CaseId).HasName("PK__CaseDeta__956FA6E927C7051B");

            entity.ToTable("CaseDetail", tb => tb.HasTrigger("TR_CaseDetail_UpdateTime"));

            entity.HasIndex(e => e.CaseId, "IX_CaseDetail_caseID");

            entity.Property(e => e.CaseId)
                .HasMaxLength(10)
                .HasColumnName("caseID");
            entity.Property(e => e.ContactName)
                .HasMaxLength(20)
                .HasColumnName("contact_name");
            entity.Property(e => e.ContactPhone)
                .HasMaxLength(15)
                .HasColumnName("contact_phone");
            entity.Property(e => e.ContactRelationValueId).HasColumnName("contact_relation_value_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");
            entity.Property(e => e.Deleted)
                .HasDefaultValue(false)
                .HasColumnName("deleted");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy)
                .HasMaxLength(30)
                .HasColumnName("deleted_by");
            entity.Property(e => e.FamilyStructureOtherDesc)
                .HasMaxLength(100)
                .HasColumnName("family_structure_other_desc");
            entity.Property(e => e.FamilyStructureTypeId).HasColumnName("family_structure_type_id");
            entity.Property(e => e.HelpExperienceValueId).HasColumnName("help_experience_value_id");
            entity.Property(e => e.HomePhone)
                .HasMaxLength(15)
                .HasColumnName("home_phone");
            entity.Property(e => e.MainCaregiverBirth).HasColumnName("main_caregiver_birth");
            entity.Property(e => e.MainCaregiverEduValueId).HasColumnName("main_caregiver_edu_value_id");
            entity.Property(e => e.MainCaregiverId)
                .HasMaxLength(255)
                .HasColumnName("main_caregiver_id");
            entity.Property(e => e.MainCaregiverJob)
                .HasMaxLength(30)
                .HasColumnName("main_caregiver_job");
            entity.Property(e => e.MainCaregiverMarryStatusValueId).HasColumnName("main_caregiver_marry_status_value_id");
            entity.Property(e => e.MainCaregiverName)
                .HasMaxLength(20)
                .HasColumnName("main_caregiver_name");
            entity.Property(e => e.MainCaregiverRelationValueId)
                .HasColumnName("main_caregiver_relation_value_id");
            entity.Property(e => e.Note)
                .HasMaxLength(1000)
                .HasColumnName("note");
            entity.Property(e => e.ParentNationFatherId).HasColumnName("parent_nation_father_id");
            entity.Property(e => e.ParentNationMotherId).HasColumnName("parent_nation_mother_id");
            entity.Property(e => e.SourceValueId).HasColumnName("source_value_id");
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Case).WithOne(p => p.CaseDetail)
                .HasForeignKey<CaseDetail>(d => d.CaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CaseDetai__caseI__06CD04F7");

            entity.HasOne(d => d.CaseOpening).WithOne(o => o.CaseDetail)
                .HasForeignKey<CaseDetail>(d => d.OpeningId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CaseDetail_CaseOpening");

            entity.HasOne(d => d.ContactRelationValue).WithMany(p => p.CaseDetailContactRelationValues)
                .HasForeignKey(d => d.ContactRelationValueId)
                .HasConstraintName("FK__CaseDetai__conta__07C12930");

            entity.HasOne(d => d.MainCaregiverRelationValue).WithMany(p => p.CaseDetailMainCaregiverRelationValues)
                .HasForeignKey(d => d.MainCaregiverRelationValueId)
                .HasConstraintName("FK__CaseDetai__main_caregiver_relation__07C12931");

            entity.HasOne(d => d.FamilyStructureType).WithMany(p => p.CaseDetails)
                .HasForeignKey(d => d.FamilyStructureTypeId)
                .HasConstraintName("FK__CaseDetai__famil__08B54D69");

            entity.HasOne(d => d.HelpExperienceValue).WithMany(p => p.CaseDetailHelpExperienceValues)
                .HasForeignKey(d => d.HelpExperienceValueId)
                .HasConstraintName("FK__CaseDetai__help___0E6E26BF");

            entity.HasOne(d => d.MainCaregiverEduValue).WithMany(p => p.CaseDetailMainCaregiverEduValues)
                .HasForeignKey(d => d.MainCaregiverEduValueId)
                .HasConstraintName("FK__CaseDetai__main___0C85DE4D");

            entity.HasOne(d => d.MainCaregiverMarryStatusValue).WithMany(p => p.CaseDetailMainCaregiverMarryStatusValues)
                .HasForeignKey(d => d.MainCaregiverMarryStatusValueId)
                .HasConstraintName("FK__CaseDetai__main___0B91BA14");

            entity.HasOne(d => d.ParentNationFather).WithMany(p => p.CaseDetailParentNationFathers)
                .HasForeignKey(d => d.ParentNationFatherId)
                .HasConstraintName("FK__CaseDetai__paren__09A971A2");

            entity.HasOne(d => d.ParentNationMother).WithMany(p => p.CaseDetailParentNationMothers)
                .HasForeignKey(d => d.ParentNationMotherId)
                .HasConstraintName("FK__CaseDetai__paren__0A9D95DB");

            entity.HasOne(d => d.SourceValue).WithMany(p => p.CaseDetailSourceValues)
                .HasForeignKey(d => d.SourceValueId)
                .HasConstraintName("FK__CaseDetai__sourc__0D7A0286");

            // 全局查詢過濾器：自動排除已刪除的記錄
            entity.HasQueryFilter(e => e.Deleted != true);
        });

        modelBuilder.Entity<CaseDetailHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__CaseDeta__096AA2E9A69DB73B");

            entity.ToTable("CaseDetailHistory");

            entity.Property(e => e.HistoryId).HasColumnName("history_id");
            entity.Property(e => e.CaseId)
                .HasMaxLength(10)
                .HasColumnName("caseID");
            entity.Property(e => e.ChangeReason)
                .HasMaxLength(500)
                .HasColumnName("change_reason");
            entity.Property(e => e.ChangeType)
                .HasMaxLength(20)
                .HasColumnName("change_type");
            entity.Property(e => e.ChangedAt)
                .HasColumnName("changed_at");
            entity.Property(e => e.ChangedBy)
                .HasMaxLength(30)
                .HasColumnName("changed_by");
            entity.Property(e => e.FieldName)
                .HasMaxLength(100)
                .HasColumnName("field_name");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .HasColumnName("ip_address");
            entity.Property(e => e.NewValue).HasColumnName("new_value");
            entity.Property(e => e.OldValue).HasColumnName("old_value");
            entity.Property(e => e.UserAgent)
                .HasMaxLength(500)
                .HasColumnName("user_agent");
            entity.Property(e => e.VersionNumber).HasColumnName("version_number");

            entity.HasOne(d => d.Case).WithMany(p => p.CaseDetailHistories)
                .HasForeignKey(d => d.CaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CaseDetai__caseI__6BE40491");
        });

        modelBuilder.Entity<CaseEqemotionalEvaluation>(entity =>
        {
            entity.HasKey(e => e.CaseId).HasName("PK__CaseEQem__956FA6E91D9170DF");

            entity.ToTable("CaseEQemotionalEvaluation", tb => tb.HasTrigger("TR_CaseEQemotionalEvaluation_UpdateTime"));

            entity.HasIndex(e => e.CaseId, "IX_CaseEQemotionalEvaluation_caseID");
            entity.HasIndex(e => e.OpeningId, "IX_CaseEQemotionalEvaluation_opening_id");

            entity.Property(e => e.CaseId)
                .HasMaxLength(10)
                .HasColumnName("caseID");
            entity.Property(e => e.OpeningId).HasColumnName("opening_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");
            entity.Property(e => e.Deleted)
                .HasDefaultValue(false)
                .HasColumnName("deleted");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy)
                .HasMaxLength(30)
                .HasColumnName("deleted_by");
            entity.Property(e => e.EqQ1).HasColumnName("eq_q1");
            entity.Property(e => e.EqQ2).HasColumnName("eq_q2");
            entity.Property(e => e.EqQ3).HasColumnName("eq_q3");
            entity.Property(e => e.EqQ4).HasColumnName("eq_q4");
            entity.Property(e => e.EqQ5).HasColumnName("eq_q5");
            entity.Property(e => e.EqQ6).HasColumnName("eq_q6");
            entity.Property(e => e.EqQ7).HasColumnName("eq_q7");
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Case).WithOne(p => p.CaseEqemotionalEvaluation)
                .HasForeignKey<CaseEqemotionalEvaluation>(d => d.CaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CaseEQemo__caseI__3E1D39E1");

            entity.HasOne(d => d.CaseOpening).WithOne(p => p.CaseEqemotionalEvaluation)
                .HasForeignKey<CaseEqemotionalEvaluation>(d => d.OpeningId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CaseEQemotionalEvaluation_CaseOpening");

            // 全局查詢過濾器：自動排除已刪除的記錄
            entity.HasQueryFilter(e => e.Deleted != true);
        });

        modelBuilder.Entity<CaseFamilyMember>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.CaseId)
                .HasMaxLength(10)
                .HasColumnName("caseID");
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");
            entity.Property(e => e.Deleted)
                .HasDefaultValue(false)
                .HasColumnName("deleted");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy)
                .HasMaxLength(30)
                .HasColumnName("deleted_by");
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .HasColumnName("description");
            entity.Property(e => e.MemberTypeValueId).HasColumnName("member_type_value_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Case).WithMany()
                .HasForeignKey(d => d.CaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CaseFamil__caseI__4E53A1AA");

            entity.HasOne(d => d.MemberTypeValue).WithMany()
                .HasForeignKey(d => d.MemberTypeValueId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CaseFamil__membe__4F47C5E3");

            // 全局查詢過濾器：自動排除已刪除的記錄
            entity.HasQueryFilter(e => e.Deleted != true);
        });

        modelBuilder.Entity<CaseFamilyMemberNote>(entity =>
        {
            entity.HasKey(e => e.NoteId).HasName("PK__CaseFami__CEDD0FA4B2CD6ED0");

            entity.Property(e => e.NoteId).HasColumnName("note_id");
            entity.Property(e => e.CaseId)
                .HasMaxLength(10)
                .HasColumnName("caseID");
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");
            entity.Property(e => e.Deleted)
                .HasDefaultValue(false)
                .HasColumnName("deleted");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy)
                .HasMaxLength(30)
                .HasColumnName("deleted_by");
            entity.Property(e => e.MemberName)
                .HasMaxLength(50)
                .HasColumnName("member_name");
            entity.Property(e => e.MemberTypeValueId).HasColumnName("member_type_value_id");
            entity.Property(e => e.NoteContent)
                .HasColumnType("ntext")
                .HasColumnName("note_content");
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Case).WithMany(p => p.CaseFamilyMemberNotes)
                .HasForeignKey(d => d.CaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CaseFamil__caseI__55009F39");

            entity.HasOne(d => d.MemberTypeValue).WithMany(p => p.CaseFamilyMemberNotes)
                .HasForeignKey(d => d.MemberTypeValueId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CaseFamil__membe__55F4C372");

            // 全局查詢過濾器：自動排除已刪除的記錄
            entity.HasQueryFilter(e => e.Deleted != true);
        });

        modelBuilder.Entity<CaseFamilySpecialStatus>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("CaseFamilySpecialStatus");

            entity.Property(e => e.CaseId)
                .HasMaxLength(10)
                .HasColumnName("caseID");
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");
            entity.Property(e => e.Deleted)
                .HasDefaultValue(false)
                .HasColumnName("deleted");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy)
                .HasMaxLength(30)
                .HasColumnName("deleted_by");
            entity.Property(e => e.DisabilityIcfCode)
                .HasMaxLength(100)
                .HasColumnName("disability_icf_code");
            entity.Property(e => e.LowIncomeCardNumber)
                .HasMaxLength(20)
                .HasColumnName("low_income_card_number");
            entity.Property(e => e.OtherDescription)
                .HasMaxLength(100)
                .HasColumnName("other_description");
            entity.Property(e => e.StatusTypeValueId).HasColumnName("status_type_value_id");
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Case).WithMany()
                .HasForeignKey(d => d.CaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CaseFamil__caseI__489AC854");

            entity.HasOne(d => d.StatusTypeValue).WithMany()
                .HasForeignKey(d => d.StatusTypeValueId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CaseFamil__statu__498EEC8D");

            // 全局查詢過濾器：自動排除已刪除的記錄
            entity.HasQueryFilter(e => e.Deleted != true);
        });

        modelBuilder.Entity<CaseFqeconomicStatus>(entity =>
        {
            entity.HasKey(e => e.CaseId).HasName("PK__CaseFQec__956FA6E98584FE85");

            entity.ToTable("CaseFQeconomicStatus", tb => tb.HasTrigger("TR_CaseFQeconomicStatus_UpdateTime"));

            entity.HasIndex(e => e.CaseId, "IX_CaseFQeconomicStatus_caseID");

            entity.Property(e => e.CaseId)
                .HasMaxLength(10)
                .HasColumnName("caseID");
            entity.Property(e => e.OpeningId).HasColumnName("opening_id");
            entity.Property(e => e.CivilWelfareResources)
                .HasMaxLength(50)
                .HasColumnName("civil_welfare_resources");
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");
            entity.Property(e => e.Deleted)
                .HasDefaultValue(false)
                .HasColumnName("deleted");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy)
                .HasMaxLength(30)
                .HasColumnName("deleted_by");
            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .HasColumnName("description");
            entity.Property(e => e.EconomicOverview)
                .HasMaxLength(200)
                .HasColumnName("economic_overview");
            entity.Property(e => e.MonthlyExpense)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("monthly_expense");
            entity.Property(e => e.MonthlyExpenseNote)
                .HasMaxLength(50)
                .HasColumnName("monthly_expense_note");
            entity.Property(e => e.MonthlyIncome)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("monthly_income");
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");
            entity.Property(e => e.WorkSituation)
                .HasMaxLength(50)
                .HasColumnName("work_situation");

            entity.HasOne(d => d.Case).WithOne(p => p.CaseFqeconomicStatus)
                .HasForeignKey<CaseFqeconomicStatus>(d => d.CaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CaseFQeco__caseI__208CD6FA");

            entity.HasOne(d => d.CaseOpening).WithOne(p => p.CaseFqeconomicStatus)
                .HasForeignKey<CaseFqeconomicStatus>(d => d.OpeningId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CaseFQeconomicStatus_CaseOpening");

            // 全局查詢過濾器：自動排除已刪除的記錄
            entity.HasQueryFilter(e => e.Deleted != true);
        });

        modelBuilder.Entity<CaseHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__CaseHist__096AA2E9EAA0545B");

            entity.ToTable("CaseHistory");

            entity.Property(e => e.HistoryId).HasColumnName("history_id");
            entity.Property(e => e.CaseId)
                .HasMaxLength(10)
                .HasColumnName("caseID");
            entity.Property(e => e.ChangeReason)
                .HasMaxLength(500)
                .HasColumnName("change_reason");
            entity.Property(e => e.ChangeType)
                .HasMaxLength(20)
                .HasColumnName("change_type");
            entity.Property(e => e.ChangedAt)
                .HasColumnName("changed_at");
            entity.Property(e => e.ChangedBy)
                .HasMaxLength(30)
                .HasColumnName("changed_by");
            entity.Property(e => e.FieldName)
                .HasMaxLength(100)
                .HasColumnName("field_name");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .HasColumnName("ip_address");
            entity.Property(e => e.NewValue).HasColumnName("new_value");
            entity.Property(e => e.OldValue).HasColumnName("old_value");
            entity.Property(e => e.UserAgent)
                .HasMaxLength(500)
                .HasColumnName("user_agent");
            entity.Property(e => e.VersionNumber).HasColumnName("version_number");

            entity.HasOne(d => d.Case).WithMany(p => p.CaseHistories)
                .HasForeignKey(d => d.CaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CaseHisto__caseI__681373AD");
        });

        modelBuilder.Entity<CaseHqhealthStatus>(entity =>
        {
            entity.HasKey(e => e.CaregiverId).HasName("PK__CaseHQhe__F6A63A40A924AA99");

            entity.ToTable("CaseHQhealthStatus", tb => tb.HasTrigger("TR_CaseHQhealthStatus_UpdateTime"));

            entity.HasIndex(e => e.CaseId, "IX_CaseHQhealthStatus_caseID");
            entity.HasIndex(e => e.OpeningId, "IX_CaseHQhealthStatus_opening_id");

            entity.Property(e => e.CaregiverId).HasColumnName("caregiver_id");
            entity.Property(e => e.CaregiverName)
                .HasMaxLength(50)
                .HasColumnName("caregiver_name");
            entity.Property(e => e.CaregiverRoleValueId).HasColumnName("caregiver_role_value_id");
            entity.Property(e => e.CaseId)
                .HasMaxLength(10)
                .HasColumnName("caseID");
            entity.Property(e => e.OpeningId).HasColumnName("opening_id");
            entity.Property(e => e.ChildCareStatusNote)
                .HasMaxLength(50)
                .HasColumnName("child_care_status_note");
            entity.Property(e => e.ChildCareStatusRating).HasColumnName("child_care_status_rating");
            entity.Property(e => e.ChildHealthStatusNote)
                .HasMaxLength(50)
                .HasColumnName("child_health_status_note");
            entity.Property(e => e.ChildHealthStatusRating).HasColumnName("child_health_status_rating");
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");
            entity.Property(e => e.Deleted)
                .HasDefaultValue(false)
                .HasColumnName("deleted");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy)
                .HasMaxLength(30)
                .HasColumnName("deleted_by");
            entity.Property(e => e.EmotionalExpressionNote)
                .HasMaxLength(50)
                .HasColumnName("emotional_expression_note");
            entity.Property(e => e.EmotionalExpressionRating).HasColumnName("emotional_expression_rating");
            entity.Property(e => e.HealthStatusNote)
                .HasMaxLength(50)
                .HasColumnName("health_status_note");
            entity.Property(e => e.HealthStatusRating).HasColumnName("health_status_rating");
            entity.Property(e => e.IsPrimary)
                .HasDefaultValue(false)
                .HasColumnName("is_primary");
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.HasOne(d => d.CaregiverRoleValue).WithMany(p => p.CaseHqhealthStatuses)
                .HasForeignKey(d => d.CaregiverRoleValueId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CaseHQhea__careg__2BFE89A6");

            entity.HasOne(d => d.Case).WithMany(p => p.CaseHqhealthStatuses)
                .HasForeignKey(d => d.CaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CaseHQhea__caseI__2B0A656D");

            entity.HasOne(d => d.CaseOpening).WithMany(p => p.CaseHqhealthStatuses)
                .HasForeignKey(d => d.OpeningId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CaseHQhealthStatus_CaseOpening");

            // 全局查詢過濾器：自動排除已刪除的記錄
            entity.HasQueryFilter(e => e.Deleted != true);
        });

        modelBuilder.Entity<CaseIqacademicPerformance>(entity =>
        {
            entity.HasKey(e => e.CaseId).HasName("PK__CaseIQac__956FA6E94231541A");

            entity.ToTable("CaseIQacademicPerformance", tb => tb.HasTrigger("TR_CaseIQacademicPerformance_UpdateTime"));

            entity.HasIndex(e => e.CaseId, "IX_CaseIQacademicPerformance_caseID");
            entity.HasIndex(e => e.OpeningId, "IX_CaseIQacademicPerformance_opening_id");

            entity.Property(e => e.CaseId)
                .HasMaxLength(10)
                .HasColumnName("caseID");
            entity.Property(e => e.OpeningId).HasColumnName("opening_id");
            entity.Property(e => e.AcademicPerformanceSummary)
                .HasMaxLength(500)
                .HasColumnName("academic_performance_summary");
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");
            entity.Property(e => e.Deleted)
                .HasDefaultValue(false)
                .HasColumnName("deleted");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy)
                .HasMaxLength(30)
                .HasColumnName("deleted_by");
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Case).WithOne(p => p.CaseIqacademicPerformance)
                .HasForeignKey<CaseIqacademicPerformance>(d => d.CaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CaseIQaca__caseI__31B762FC");

            entity.HasOne(d => d.CaseOpening).WithOne(p => p.CaseIqacademicPerformance)
                .HasForeignKey<CaseIqacademicPerformance>(d => d.OpeningId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CaseIQacademicPerformance_CaseOpening");

            // 全局查詢過濾器：自動排除已刪除的記錄
            entity.HasQueryFilter(e => e.Deleted != true);
        });

        modelBuilder.Entity<CaseSocialWorkerContent>(entity =>
        {
            entity.HasKey(e => e.CaseId).HasName("PK__CaseSoci__956FA6E99FF2C225");

            entity.ToTable("CaseSocialWorkerContent");

            entity.Property(e => e.CaseId)
                .HasMaxLength(10)
                .HasColumnName("caseID");
            entity.Property(e => e.CaregiverChildInteractionNote)
                .HasMaxLength(50)
                .HasColumnName("caregiver_child_interaction_note");
            entity.Property(e => e.CaregiverChildInteractionRating).HasColumnName("caregiver_child_interaction_rating");
            entity.Property(e => e.CaregiverFamilyInteractionNote)
                .HasMaxLength(50)
                .HasColumnName("caregiver_family_interaction_note");
            entity.Property(e => e.CaregiverFamilyInteractionRating).HasColumnName("caregiver_family_interaction_rating");
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");
            entity.Property(e => e.Deleted)
                .HasDefaultValue(false)
                .HasColumnName("deleted");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy)
                .HasMaxLength(30)
                .HasColumnName("deleted_by");
            entity.Property(e => e.FamilyResourceAbilityNote)
                .HasMaxLength(50)
                .HasColumnName("family_resource_ability_note");
            entity.Property(e => e.FamilyResourceAbilityRating).HasColumnName("family_resource_ability_rating");
            entity.Property(e => e.FamilySocialSupportNote)
                .HasMaxLength(50)
                .HasColumnName("family_social_support_note");
            entity.Property(e => e.FamilySocialSupportRating).HasColumnName("family_social_support_rating");
            entity.Property(e => e.FamilyTreeImg)
                .HasMaxLength(250)
                .HasColumnName("family_tree_img");
            entity.Property(e => e.HouseCleanlinessNote)
                .HasMaxLength(50)
                .HasColumnName("house_cleanliness_note");
            entity.Property(e => e.HouseCleanlinessRating).HasColumnName("house_cleanliness_rating");
            entity.Property(e => e.HouseSafetyNote)
                .HasMaxLength(50)
                .HasColumnName("house_safety_note");
            entity.Property(e => e.HouseSafetyRating).HasColumnName("house_safety_rating");
            entity.Property(e => e.ResidenceTypeValueId).HasColumnName("residence_type_value_id");
            entity.Property(e => e.SpecialCircumstancesDescription)
                .HasMaxLength(50)
                .HasColumnName("special_circumstances_description");
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Case).WithOne(p => p.CaseSocialWorkerContent)
                .HasForeignKey<CaseSocialWorkerContent>(d => d.CaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CaseSocia__caseI__19DFD96B");

            entity.HasOne(d => d.CaseOpening).WithOne(p => p.CaseSocialWorkerContent)
                .HasForeignKey<CaseSocialWorkerContent>(d => d.OpeningId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CaseSocialWorkerContent_CaseOpening");

            entity.HasOne(d => d.ResidenceTypeValue).WithMany(p => p.CaseSocialWorkerContents)
                .HasForeignKey(d => d.ResidenceTypeValueId)
                .HasConstraintName("FK__CaseSocia__resid__1AD3FDA4");

            // 全局查詢過濾器：自動排除已刪除的記錄
            entity.HasQueryFilter(e => e.Deleted != true);
        });

        modelBuilder.Entity<CaseSocialWorkerService>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK__CaseSoci__3E0DB8AFBE5651FA");

            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.CaseId)
                .HasMaxLength(10)
                .HasColumnName("caseID");
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");
            entity.Property(e => e.Deleted)
                .HasDefaultValue(false)
                .HasColumnName("deleted");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy)
                .HasMaxLength(30)
                .HasColumnName("deleted_by");
            entity.Property(e => e.ServiceDate).HasColumnName("service_date");
            entity.Property(e => e.ServiceDescription)
                .HasMaxLength(500)
                .HasColumnName("service_description");
            entity.Property(e => e.ServiceProvider)
                .HasMaxLength(50)
                .HasColumnName("service_provider");
            entity.Property(e => e.ServiceTypeValueId).HasColumnName("service_type_value_id");
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Case).WithMany(p => p.CaseSocialWorkerServices)
                .HasForeignKey(d => d.CaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CaseSocia__caseI__634EBE90");

            entity.HasOne(d => d.ServiceTypeValue).WithMany(p => p.CaseSocialWorkerServices)
                .HasForeignKey(d => d.ServiceTypeValueId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CaseSocia__servi__6442E2C9");

            // 全局查詢過濾器：自動排除已刪除的記錄
            entity.HasQueryFilter(e => e.Deleted != true);
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(e => e.CityId).HasName("PK__Cities__031491A82DDA3769");

            entity.HasIndex(e => e.CityName, "UQ__Cities__1AA4F7B590443F24").IsUnique();

            entity.Property(e => e.CityId).HasColumnName("city_id");
            entity.Property(e => e.CityName)
                .HasMaxLength(10)
                .HasColumnName("city_name");
        });

        modelBuilder.Entity<DataChangeLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__DataChan__7839F62D88C723F6");

            entity.ToTable("DataChangeLog");

            entity.Property(e => e.LogId).HasColumnName("logID");
            entity.Property(e => e.ChangedAt)
                .HasColumnName("changed_at");
            entity.Property(e => e.ChangedBy)
                .HasMaxLength(30)
                .HasColumnName("changed_by");
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");
            entity.Property(e => e.FieldName)
                .HasMaxLength(100)
                .HasColumnName("field_name");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .HasColumnName("ip_address");
            entity.Property(e => e.NewValue).HasColumnName("new_value");
            entity.Property(e => e.OldValue).HasColumnName("old_value");
            entity.Property(e => e.OperationType)
                .HasMaxLength(20)
                .HasColumnName("operation_type");
            entity.Property(e => e.RecordId)
                .HasMaxLength(50)
                .HasColumnName("record_id");
            entity.Property(e => e.TableName)
                .HasMaxLength(100)
                .HasColumnName("table_name");
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");
            entity.Property(e => e.UserAgent)
                .HasMaxLength(500)
                .HasColumnName("user_agent");
        });

        modelBuilder.Entity<District>(entity =>
        {
            entity.HasKey(e => e.DistrictId).HasName("PK__District__2521322B86B5E7A2");

            entity.HasIndex(e => e.DistrictName, "UQ__District__9E05AFF9CEDD6473").IsUnique();

            entity.Property(e => e.DistrictId).HasColumnName("district_id");
            entity.Property(e => e.CityId).HasColumnName("city_id");
            entity.Property(e => e.DistrictName)
                .HasMaxLength(10)
                .HasColumnName("district_name");

            entity.HasOne(d => d.City).WithMany(p => p.Districts)
                .HasForeignKey(d => d.CityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Districts__city___6E01572D");
        });

        modelBuilder.Entity<FamilyStructureType>(entity =>
        {
            entity.HasKey(e => e.StructureTypeId).HasName("PK__FamilySt__6BBEBC67EB138E2C");

            entity.HasIndex(e => e.StructureCode, "UQ__FamilySt__84CB930AE64C5F03").IsUnique();

            entity.Property(e => e.StructureTypeId).HasColumnName("structure_type_id");
            entity.Property(e => e.NeedsDescription)
                .HasDefaultValue(false)
                .HasColumnName("needs_description");
            entity.Property(e => e.StructureCode)
                .HasMaxLength(20)
                .HasColumnName("structure_code");
            entity.Property(e => e.StructureName)
                .HasMaxLength(50)
                .HasColumnName("structure_name");
        });

        modelBuilder.Entity<FinalAssessmentSummary>(entity =>
        {
            entity.HasKey(e => e.CaseId).HasName("PK__FinalAss__956FA6E9E622ECA0");

            entity.ToTable("FinalAssessmentSummary", tb => tb.HasTrigger("TR_FinalAssessmentSummary_UpdateTime"));

            entity.HasIndex(e => e.CaseId, "IX_FinalAssessmentSummary_caseID");
            entity.HasIndex(e => e.OpeningId, "IX_FinalAssessmentSummary_opening_id");

            entity.Property(e => e.CaseId)
                .HasMaxLength(10)
                .HasColumnName("caseID");
            entity.Property(e => e.OpeningId).HasColumnName("opening_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");
            entity.Property(e => e.Deleted)
                .HasDefaultValue(false)
                .HasColumnName("deleted");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy)
                .HasMaxLength(30)
                .HasColumnName("deleted_by");
            entity.Property(e => e.EqSummary)
                .HasColumnType("text")
                .HasColumnName("eq_summary");
            entity.Property(e => e.FqSummary)
                .HasColumnType("text")
                .HasColumnName("fq_summary");
            entity.Property(e => e.HqSummary)
                .HasColumnType("text")
                .HasColumnName("hq_summary");
            entity.Property(e => e.IqSummary)
                .HasColumnType("text")
                .HasColumnName("iq_summary");
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Case).WithOne(p => p.FinalAssessmentSummary)
                .HasForeignKey<FinalAssessmentSummary>(d => d.CaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FinalAsse__caseI__43D61337");

            entity.HasOne(d => d.CaseOpening).WithOne(p => p.FinalAssessmentSummary)
                .HasForeignKey<FinalAssessmentSummary>(d => d.OpeningId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FinalAssessmentSummary_CaseOpening");

            // 全局查詢過濾器：自動排除已刪除的記錄
            entity.HasQueryFilter(e => e.Deleted != true);
        });

        modelBuilder.Entity<Nationality>(entity =>
        {
            entity.HasKey(e => e.NationalityId).HasName("PK__National__2E6444ED91C8FBA5");

            entity.HasIndex(e => e.NationalityName, "UQ__National__64AF10BC33C40389").IsUnique();

            entity.HasIndex(e => e.NationalityCode, "UQ__National__A22D2FE5AD603ECB").IsUnique();

            entity.Property(e => e.NationalityId).HasColumnName("nationality_id");
            entity.Property(e => e.NationalityCode)
                .HasMaxLength(10)
                .HasColumnName("nationality_code");
            entity.Property(e => e.NationalityName)
                .HasMaxLength(20)
                .HasColumnName("nationality_name");
        });

        modelBuilder.Entity<OptionSet>(entity =>
        {
            entity.HasKey(e => e.OptionSetId).HasName("PK__OptionSe__2092EE9521649D1A");

            entity.HasIndex(e => e.OptionKey, "UQ__OptionSe__C54F5DD694F6E72D").IsUnique();

            entity.Property(e => e.OptionSetId).HasColumnName("option_set_id");
            entity.Property(e => e.OptionKey)
                .HasMaxLength(50)
                .HasColumnName("option_key");
            entity.Property(e => e.OptionSetName)
                .HasMaxLength(50)
                .HasColumnName("option_set_name");
        });

        modelBuilder.Entity<OptionSetValue>(entity =>
        {
            entity.HasKey(e => e.OptionValueId).HasName("PK__OptionSe__3AAA210D287E4599");

            entity.Property(e => e.OptionValueId).HasColumnName("option_value_id");
            entity.Property(e => e.OptionSetId).HasColumnName("option_set_id");
            entity.Property(e => e.ValueCode)
                .HasMaxLength(30)
                .HasColumnName("value_code");
            entity.Property(e => e.ValueName)
                .HasMaxLength(50)
                .HasColumnName("value_name");

            entity.HasOne(d => d.OptionSet).WithMany(p => p.OptionSetValues)
                .HasForeignKey(d => d.OptionSetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OptionSet__optio__6477ECF3");
        });

        modelBuilder.Entity<School>(entity =>
        {
            entity.HasKey(e => e.SchoolId).HasName("PK__Schools__27CA6CF491635D7B");

            entity.HasIndex(e => e.SchoolName, "UQ__Schools__007188191EFA6D19").IsUnique();

            entity.Property(e => e.SchoolId).HasColumnName("school_id");
            entity.Property(e => e.SchoolName)
                .HasMaxLength(50)
                .HasColumnName("school_name");
            entity.Property(e => e.SchoolType)
                .HasMaxLength(20)
                .HasColumnName("school_type");
        });

        modelBuilder.Entity<UserActivityLog>(entity =>
        {
            entity.HasKey(e => e.ActivityId).HasName("PK__UserActi__0FC9CBCC3741A266");

            entity.ToTable("UserActivityLog");

            entity.Property(e => e.ActivityId).HasColumnName("activityID");
            entity.Property(e => e.ActivityDescription)
                .HasMaxLength(500)
                .HasColumnName("activity_description");
            entity.Property(e => e.ActivityType)
                .HasMaxLength(50)
                .HasColumnName("activity_type");
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .HasColumnName("ip_address");
            entity.Property(e => e.TargetRecordId)
                .HasMaxLength(50)
                .HasColumnName("target_record_id");
            entity.Property(e => e.TargetTable)
                .HasMaxLength(100)
                .HasColumnName("target_table");
            entity.Property(e => e.UserAgent)
                .HasMaxLength(500)
                .HasColumnName("user_agent");
            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .HasColumnName("user_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
