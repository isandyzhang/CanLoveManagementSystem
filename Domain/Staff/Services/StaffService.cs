using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CanLove_Backend.Infrastructure.Data.Contexts;
using CanLove_Backend.Domain.Staff.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using CanLove_Backend.Core.Extensions;

namespace CanLove_Backend.Domain.Staff.Services;

public class StaffService : IStaffService
{
    private readonly CanLoveDbContext _context;

    public StaffService(CanLoveDbContext context)
    {
        _context = context;
    }

    public async Task<Models.Staff?> GetStaffByAzureObjectIdAsync(string azureObjectId)
    {
        return await _context.Staffs
            .FirstOrDefaultAsync(s => s.AzureObjectId == azureObjectId && !s.Deleted);
    }

    public async Task<Models.Staff> CreateStaffFromAzureAsync(ClaimsPrincipal principal)
    {
        var staff = new Models.Staff
        {
            AzureObjectId = principal.GetObjectId() ?? throw new InvalidOperationException("無法取得Azure ObjectId"),
            AzureTenantId = principal.GetTenantId() ?? throw new InvalidOperationException("無法取得Azure TenantId"),
            Email = principal.FindFirst(ClaimTypes.Email)?.Value 
                ?? principal.FindFirst("preferred_username")?.Value 
                ?? throw new InvalidOperationException("無法取得Email"),
            DisplayName = principal.FindFirst(ClaimTypes.Name)?.Value 
                ?? principal.FindFirst("name")?.Value,
            JobTitle = principal.FindFirst("jobTitle")?.Value,
            Department = principal.FindFirst("department")?.Value,
            IsActive = true,
            CreatedAt = DateTimeExtensions.TaiwanTime,
            UpdatedAt = DateTimeExtensions.TaiwanTime,
            LastSyncAt = DateTimeExtensions.TaiwanTime
        };

        // 嘗試取得頭像URL（從Claims或Graph API）
        TryGetPhotoUrlAsync(staff, principal);

        _context.Staffs.Add(staff);
        await _context.SaveChangesAsync();

        return staff;
    }

    public async Task<Models.Staff> UpdateStaffFromAzureAsync(Models.Staff staff, ClaimsPrincipal principal)
    {
        // 更新基本資訊
        staff.Email = principal.FindFirst(ClaimTypes.Email)?.Value 
            ?? principal.FindFirst("preferred_username")?.Value 
            ?? staff.Email;
        staff.DisplayName = principal.FindFirst(ClaimTypes.Name)?.Value 
            ?? principal.FindFirst("name")?.Value 
            ?? staff.DisplayName;
        staff.JobTitle = principal.FindFirst("jobTitle")?.Value ?? staff.JobTitle;
        staff.Department = principal.FindFirst("department")?.Value ?? staff.Department;
        staff.UpdatedAt = DateTimeExtensions.TaiwanTime;
        staff.LastSyncAt = DateTimeExtensions.TaiwanTime;

        // 更新頭像URL
        TryGetPhotoUrlAsync(staff, principal);

        await _context.SaveChangesAsync();

        return staff;
    }

    public async Task LogStaffLoginAsync(int staffId, string? ipAddress, string? userAgent)
    {
        var staff = await _context.Staffs.FindAsync(staffId);
        if (staff != null)
        {
            staff.LastLoginAt = DateTimeExtensions.TaiwanTime;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Models.Staff>> GetAllForListAsync()
    {
        return await _context.Staffs
            .Where(s => !s.Deleted)
            .OrderBy(s => s.DisplayName)
            .Select(s => new Models.Staff
            {
                StaffId = s.StaffId,
                DisplayName = s.DisplayName,
                Department = s.Department,
                JobTitle = s.JobTitle,
                LastLoginAt = s.LastLoginAt
            })
            .ToListAsync();
    }

    public async Task UpdateDepartmentAndJobTitleAsync(int staffId, string? department, string? jobTitle)
    {
        var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.StaffId == staffId && !s.Deleted);
        if (staff == null)
        {
            throw new InvalidOperationException($"找不到員工：{staffId}");
        }

        staff.Department = department;
        staff.JobTitle = jobTitle;
        staff.UpdatedAt = DateTimeExtensions.TaiwanTime;

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// 嘗試取得Azure AD頭像URL
    /// </summary>
    private void TryGetPhotoUrlAsync(Models.Staff staff, ClaimsPrincipal principal)
    {
        try
        {
            // 方法1：從Claims取得（如果有）
            var photoClaim = principal.FindFirst("picture")?.Value 
                ?? principal.FindFirst("photo")?.Value;
            
            if (!string.IsNullOrEmpty(photoClaim))
            {
                staff.PhotoUrl = photoClaim;
                return;
            }

            // 方法2：構建Graph API的頭像URL（不需要實際呼叫，直接使用URL格式）
            // Graph API的頭像URL格式：https://graph.microsoft.com/v1.0/users/{objectId}/photo/$value
            // 注意：這個URL需要適當的權限才能存取
            var objectId = principal.GetObjectId();
            if (!string.IsNullOrEmpty(objectId))
            {
                // 如果Claims中沒有頭像，使用Graph API URL格式
                staff.PhotoUrl = $"https://graph.microsoft.com/v1.0/users/{objectId}/photo/$value";
            }
        }
        catch
        {
            // 取得頭像失敗時不影響主要流程
        }
    }
}

