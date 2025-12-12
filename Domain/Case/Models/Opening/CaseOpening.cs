using CanLove_Backend.Domain.Case.Models.Basic;
using System;
using System.Collections.Generic;

namespace CanLove_Backend.Domain.Case.Models.Opening;

public class CaseOpening
{
    public int OpeningId { get; set; }

    public string CaseId { get; set; } = null!;

    public DateOnly? OpenDate { get; set; }

    public string? OpenReason { get; set; }

    public string Status { get; set; } = "Draft"; // Draft/PendingReview/Rejected/Approved/Closed

    public string? SubmittedBy { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public string? ReviewedBy { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string? ReviewComment { get; set; }

    public int? AssignedStaffId { get; set; }

    public bool? IsLocked { get; set; }

    public string? LockedBy { get; set; }

    public DateTime? LockedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // 軟刪除欄位（比照其他 Case* 表）
    public bool Deleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

    public string? DeletedBy { get; set; }

    public virtual CanLove_Backend.Domain.Case.Models.Basic.Case Case { get; set; } = null!;

    public virtual ICollection<CaseHqhealthStatus> CaseHqhealthStatuses { get; set; } = new List<CaseHqhealthStatus>();

    public virtual CaseDetail? CaseDetail { get; set; }

    public virtual CaseFqeconomicStatus? CaseFqeconomicStatus { get; set; }

    public virtual CaseIqacademicPerformance? CaseIqacademicPerformance { get; set; }

    public virtual CaseEqemotionalEvaluation? CaseEqemotionalEvaluation { get; set; }

    public virtual FinalAssessmentSummary? FinalAssessmentSummary { get; set; }

    public virtual CaseSocialWorkerContent? CaseSocialWorkerContent { get; set; }
}


