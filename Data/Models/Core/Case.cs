using CanLove_Backend.Data.Models.Core;
using CanLove_Backend.Data.Models.CaseDetails;
using CanLove_Backend.Data.Models.Options;
using CanLove_Backend.Data.Models.History;
using CanLove_Backend.Data.Models.Audit;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CanLove_Backend.Data.Models.Core;

public partial class Case
{
    public string CaseId { get; set; } = null!;

    public DateOnly? AssessmentDate { get; set; }

    public string Name { get; set; } = null!;

    public string? Gender { get; set; }

    public int? SchoolId { get; set; }

    public DateOnly BirthDate { get; set; } = new DateOnly(2000, 1, 1);

    public string IdNumber { get; set; } = null!;

    public string? Address { get; set; }

    public int? CityId { get; set; }

    public int? DistrictId { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    [NotMapped]
    public string? Photo { get; set; }

    public int? PhotoBlobId { get; set; }

    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "Draft";

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
