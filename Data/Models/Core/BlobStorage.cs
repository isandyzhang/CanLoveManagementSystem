using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CanLove_Backend.Data.Models.Core;

public class BlobStorage
{
    [Key]
    [Column("blob_id")]
    public int BlobId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("container_name")]
    public string ContainerName { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    [Column("blob_name")]
    public string BlobName { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    [Column("original_file_name")]
    public string OriginalFileName { get; set; } = null!;

    [Required]
    [MaxLength(10)]
    [Column("file_extension")]
    public string FileExtension { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    [Column("content_type")]
    public string ContentType { get; set; } = null!;

    [Column("file_size")]
    public long FileSize { get; set; }

    [MaxLength(100)]
    [Column("storage_account")]
    public string? StorageAccount { get; set; }

    [MaxLength(1000)]
    [Column("blob_uri")]
    public string? BlobUri { get; set; }

    [Required]
    [MaxLength(1000)]
    [Column("blob_url")]
    public string BlobUrl { get; set; } = null!;

    [Column("uploaded_by")]
    public int? UploadedBy { get; set; }

    [Column("uploaded_at")]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [Column("is_temp")]
    public bool IsTemp { get; set; } = false;

    [Column("expires_at")]
    public DateTime? ExpiresAt { get; set; }

    [Column("metadata")]
    public string? Metadata { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("deleted")]
    public bool Deleted { get; set; } = false;

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [MaxLength(30)]
    [Column("deleted_by")]
    public string? DeletedBy { get; set; }

    // 導航屬性
    [ForeignKey("UploadedBy")]
    public virtual Staff? Uploader { get; set; }
}

