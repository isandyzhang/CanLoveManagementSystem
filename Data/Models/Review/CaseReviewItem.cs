using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CanLove_Backend.Data.Models.Review;

public class CaseReviewItem
{
    [Key]
    [Column("review_id")]
    public int ReviewId { get; set; }

    [Required]
    [MaxLength(10)]
    [Column("case_id")]
    public string CaseId { get; set; } = null!;

    // 類型：CaseBasic / CaseOpening / Consultation / Leave ...
    [Required]
    [MaxLength(30)]
    [Column("type")]
    public string Type { get; set; } = null!;

    // 來源主鍵字串（例如 CaseId、或其他主鍵）
    [Required]
    [MaxLength(50)]
    [Column("target_id")]
    public string TargetId { get; set; } = null!;

    // 清單顯示標題（可放個案姓名或摘要）
    [MaxLength(200)]
    [Column("title")]
    public string? Title { get; set; }

    // 狀態：PendingReview / Rejected / Approved
    [Required]
    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "PendingReview";

    [MaxLength(50)]
    [Column("submitted_by")]
    public string? SubmittedBy { get; set; }

    [Column("submitted_at")]
    public DateTime? SubmittedAt { get; set; }

    [MaxLength(50)]
    [Column("reviewed_by")]
    public string? ReviewedBy { get; set; }

    [Column("reviewed_at")]
    public DateTime? ReviewedAt { get; set; }

    [MaxLength(500)]
    [Column("review_comment")]
    public string? ReviewComment { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
}


