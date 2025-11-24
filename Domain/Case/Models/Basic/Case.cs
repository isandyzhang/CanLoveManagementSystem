using CanLove_Backend.Domain.Case.Models.Opening;
using CanLove_Backend.Domain.Case.Shared.Models;
using CanLove_Backend.Infrastructure.Data.History;
using CanLove_Backend.Infrastructure.Data.Audit;
using CanLove_Backend.Infrastructure.Storage.Blob;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CanLove_Backend.Domain.Case.Models.Basic;

public partial class Case
{
    [RegularExpression(@"^[A-Za-z0-9]+$", ErrorMessage = "個案編號只能包含英文字母和數字")]
    public string CaseId { get; set; } = null!;

    public DateOnly? AssessmentDate { get; set; }

    public string Name { get; set; } = null!;

    public string? Gender { get; set; }

    public int? SchoolId { get; set; }

    public DateOnly BirthDate { get; set; } = new DateOnly(2000, 1, 1);

    [RegularExpression(@"^[A-Z][12][0-9]{8}$", ErrorMessage = "身分證字號格式不正確，應為1個英文字母加上9個數字")]
    public string IdNumber { get; set; } = null!;

    public string? Address { get; set; }

    public int? CityId { get; set; }

    public int? DistrictId { get; set; }

    [RegularExpression(@"^(|09[0-9]{8}|0[2-9][0-9]{7,8}|\(0[2-9]\)[0-9]{7,8}|0[2-9]-[0-9]{7,8})$", ErrorMessage = "請輸入有效的台灣電話號碼格式")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "請輸入有效的電子郵件格式")]
    public string? Email { get; set; }

    [NotMapped]
    public string? Photo { get; set; }

    public int? PhotoBlobId { get; set; }

    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "PendingReview";

    public string? SubmittedBy { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public string? ReviewedBy { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public bool? Deleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? DeletedBy { get; set; }

    public bool? IsLocked { get; set; }

    public string? LockedBy { get; set; }

    public DateTime? LockedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<CaseConsultationRecord> CaseConsultationRecords { get; set; } = new List<CaseConsultationRecord>();

    public virtual CaseDetail? CaseDetail { get; set; }

    public virtual ICollection<CaseDetailHistory> CaseDetailHistories { get; set; } = new List<CaseDetailHistory>();

    public virtual CaseEqemotionalEvaluation? CaseEqemotionalEvaluation { get; set; }

    public virtual ICollection<CaseFamilyMemberNote> CaseFamilyMemberNotes { get; set; } = new List<CaseFamilyMemberNote>();

    public virtual CaseFqeconomicStatus? CaseFqeconomicStatus { get; set; }

    public virtual ICollection<CaseHistory> CaseHistories { get; set; } = new List<CaseHistory>();

    public virtual ICollection<CaseHqhealthStatus> CaseHqhealthStatuses { get; set; } = new List<CaseHqhealthStatus>();

    public virtual CaseIqacademicPerformance? CaseIqacademicPerformance { get; set; }

    public virtual CaseSocialWorkerContent? CaseSocialWorkerContent { get; set; }

    public virtual ICollection<CaseSocialWorkerService> CaseSocialWorkerServices { get; set; } = new List<CaseSocialWorkerService>();

    public virtual City? City { get; set; }

    public virtual District? District { get; set; }

    public virtual FinalAssessmentSummary? FinalAssessmentSummary { get; set; }

    public virtual School? School { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.ForeignKey("PhotoBlobId")]
    public virtual BlobStorage? PhotoBlob { get; set; }

}
