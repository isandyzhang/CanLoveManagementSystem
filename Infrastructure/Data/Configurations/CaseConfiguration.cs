using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CanLove_Backend.Domain.Case.Models.Basic;

namespace CanLove_Backend.Infrastructure.Data.Configurations;

/// <summary>
/// Case 實體的 Entity Framework Core 配置
/// </summary>
public class CaseConfiguration : IEntityTypeConfiguration<Case>
{
    public void Configure(EntityTypeBuilder<Case> entity)
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

        // 關聯設定
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
    }
}

