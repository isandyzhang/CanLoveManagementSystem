using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CanLove_Backend.Domain.Staff.Models;

public class Staff
{
    [Key]
    [Column("staff_id")]
    public int StaffId { get; set; }

    // Azure AD 登入相關欄位
    [Required]
    [MaxLength(255)]
    [Column("azure_object_id")]
    public string AzureObjectId { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    [Column("azure_tenant_id")]
    public string AzureTenantId { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    [Column("email")]
    public string Email { get; set; } = null!;

    [MaxLength(100)]
    [Column("display_name")]
    public string? DisplayName { get; set; }


    [MaxLength(100)]
    [Column("job_title")]
    public string? JobTitle { get; set; }

    [MaxLength(100)]
    [Column("department")]
    public string? Department { get; set; }

    [MaxLength(500)]
    [Column("photo_url")]
    public string? PhotoUrl { get; set; }  // Azure AD頭像URL，直接使用不需要下載

    // 員工管理相關欄位
    [MaxLength(50)]
    [Column("employee_id")]
    public string? EmployeeId { get; set; }

    [Column("hire_date")]
    public DateOnly? HireDate { get; set; }

    // Line打卡相關欄位
    [MaxLength(255)]
    [Column("line_user_id")]
    public string? LineUserId { get; set; }

    [Column("line_binding_at")]
    public DateTime? LineBindingAt { get; set; }

    // 系統欄位
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("last_login_at")]
    public DateTime? LastLoginAt { get; set; }

    [Column("last_sync_at")]
    public DateTime? LastSyncAt { get; set; }

    [MaxLength(1000)]
    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(8);

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow.AddHours(8);

    [Column("deleted")]
    public bool Deleted { get; set; } = false;

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [MaxLength(30)]
    [Column("deleted_by")]
    public string? DeletedBy { get; set; }
}

