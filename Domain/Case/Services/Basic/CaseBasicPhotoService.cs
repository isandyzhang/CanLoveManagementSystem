using Microsoft.AspNetCore.Http;
using CanLove_Backend.Infrastructure.Storage.Blob;
using CanLove_Backend.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using CanLove_Backend.Core.Extensions;

namespace CanLove_Backend.Domain.Case.Services.Basic;

/// <summary>
/// 個案基本資料照片處理服務 - 統一處理照片上傳、刪除、URL 取得等邏輯
/// </summary>
public class CaseBasicPhotoService
{
    private readonly IBlobService _blobService;
    private readonly CanLoveDbContext _context;
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

    public CaseBasicPhotoService(IBlobService blobService, CanLoveDbContext context)
    {
        _blobService = blobService;
        _context = context;
    }

    /// <summary>
    /// 驗證照片檔案
    /// </summary>
    /// <param name="file">上傳的檔案</param>
    /// <returns>驗證結果，如果成功則返回 null，否則返回錯誤訊息</returns>
    public string? ValidatePhotoFile(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return null; // 沒有檔案是允許的（可選上傳）
        }

        // 驗證檔案類型
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(fileExtension))
        {
            return "僅支援 JPG、PNG、GIF 格式的圖片";
        }

        // 驗證檔案大小
        if (file.Length > MaxFileSize)
        {
            return "檔案大小不能超過 5MB";
        }

        return null; // 驗證通過
    }

    /// <summary>
    /// 上傳照片
    /// </summary>
    /// <param name="file">上傳的檔案</param>
    /// <param name="caseId">個案編號</param>
    /// <param name="userEmail">使用者 Email（用於取得 StaffId）</param>
    /// <returns>上傳結果，包含 BlobId 或錯誤訊息</returns>
    public async Task<(bool Success, int? BlobId, string? ErrorMessage)> UploadPhotoAsync(
        IFormFile file, 
        string caseId, 
        string? userEmail = null)
    {
        try
        {
            // 驗證檔案
            var validationError = ValidatePhotoFile(file);
            if (validationError != null)
            {
                return (false, null, validationError);
            }

            // 取得目前使用者ID（如果有的話）
            int? uploadedBy = null;
            if (!string.IsNullOrWhiteSpace(userEmail))
            {
                var staff = await _context.Staffs
                    .FirstOrDefaultAsync(s => s.Email == userEmail && !s.Deleted);
                uploadedBy = staff?.StaffId;
            }

            // 上傳檔案到 Blob Storage
            // 使用個案編號作為檔案名稱的一部分
            var timestamp = DateTimeExtensions.TaiwanTime.ToString("yyyyMMddHHmmss");
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var blobName = $"avatars/{caseId}/photo_{timestamp}{fileExtension}";
            
            var blobStorage = await _blobService.UploadFileAsync(
                file.OpenReadStream(),
                "cases",
                blobName,
                file.ContentType,
                uploadedBy,
                false);

            return (true, blobStorage.BlobId, null);
        }
        catch (Exception ex)
        {
            return (false, null, $"照片上傳失敗：{ex.Message}");
        }
    }

    /// <summary>
    /// 刪除照片
    /// </summary>
    /// <param name="blobId">Blob ID</param>
    /// <returns>是否成功刪除</returns>
    public async Task<bool> DeletePhotoAsync(int blobId)
    {
        try
        {
            return await _blobService.DeleteFileAsync(blobId);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 取得照片 URL
    /// </summary>
    /// <param name="blobId">Blob ID</param>
    /// <returns>照片 URL，如果失敗則返回 null</returns>
    public async Task<string?> GetPhotoUrlAsync(int? blobId)
    {
        if (!blobId.HasValue)
        {
            return null;
        }

        try
        {
            return await _blobService.GetFileUrlAsync(blobId.Value);
        }
        catch
        {
            return null;
        }
    }
}
