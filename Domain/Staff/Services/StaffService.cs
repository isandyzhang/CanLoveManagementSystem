using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CanLove_Backend.Infrastructure.Data.Contexts;
using CanLove_Backend.Domain.Staff.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using CanLove_Backend.Core.Extensions;
using Microsoft.Extensions.Logging;

namespace CanLove_Backend.Domain.Staff.Services;

public class StaffService : IStaffService
{
    private readonly CanLoveDbContext _context;
    private readonly ILogger<StaffService> _logger;

    public StaffService(CanLoveDbContext context, ILogger<StaffService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Models.Staff?> GetStaffByAzureObjectIdAsync(string azureObjectId)
    {
        _logger.LogInformation("查詢員工資料。AzureObjectId: {AzureObjectId}", azureObjectId);
        
        try
        {
            var staff = await _context.Staffs
                .FirstOrDefaultAsync(s => s.AzureObjectId == azureObjectId && !s.Deleted);
            
            if (staff == null)
            {
                _logger.LogWarning("找不到員工資料。AzureObjectId: {AzureObjectId}", azureObjectId);
            }
            else
            {
                _logger.LogInformation("成功取得員工資料。StaffId: {StaffId}, DisplayName: {DisplayName}", 
                    staff.StaffId, staff.DisplayName);
            }
            
            return staff;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查詢員工資料時發生錯誤。AzureObjectId: {AzureObjectId}", azureObjectId);
            throw;
        }
    }

    public async Task<Models.Staff> CreateStaffFromAzureAsync(ClaimsPrincipal principal)
    {
        var objectId = principal.GetObjectId();
        _logger.LogInformation("開始建立新員工。AzureObjectId: {AzureObjectId}", objectId);
        
        try
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
                EmployeeId = await GenerateNextEmployeeIdAsync(),
                IsActive = true,
                CreatedAt = DateTimeExtensions.TaiwanTime,
                UpdatedAt = DateTimeExtensions.TaiwanTime,
                LastSyncAt = DateTimeExtensions.TaiwanTime
            };

            _logger.LogInformation("員工資料準備完成。EmployeeId: {EmployeeId}, Email: {Email}, DisplayName: {DisplayName}", 
                staff.EmployeeId, staff.Email, staff.DisplayName);

            // 嘗試取得頭像URL（從Claims或Graph API）
            TryGetPhotoUrlAsync(staff, principal);

            _context.Staffs.Add(staff);
            await _context.SaveChangesAsync();

            _logger.LogInformation("成功建立新員工。StaffId: {StaffId}, EmployeeId: {EmployeeId}, AzureObjectId: {AzureObjectId}", 
                staff.StaffId, staff.EmployeeId, staff.AzureObjectId);

            return staff;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "建立新員工失敗。AzureObjectId: {AzureObjectId}, Error: {Error}", 
                objectId, ex.Message);
            throw;
        }
    }

    public async Task<Models.Staff> UpdateStaffFromAzureAsync(Models.Staff staff, ClaimsPrincipal principal)
    {
        _logger.LogInformation("開始更新員工資訊。StaffId: {StaffId}, AzureObjectId: {AzureObjectId}", 
            staff.StaffId, staff.AzureObjectId);
        
        try
        {
            // 更新基本資訊
            var oldEmail = staff.Email;
            var oldDisplayName = staff.DisplayName;
            
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

            // 記錄變更
            if (oldEmail != staff.Email)
            {
                _logger.LogInformation("員工 Email 已更新。StaffId: {StaffId}, 舊值: {OldEmail}, 新值: {NewEmail}", 
                    staff.StaffId, oldEmail, staff.Email);
            }
            if (oldDisplayName != staff.DisplayName)
            {
                _logger.LogInformation("員工 DisplayName 已更新。StaffId: {StaffId}, 舊值: {OldDisplayName}, 新值: {NewDisplayName}", 
                    staff.StaffId, oldDisplayName, staff.DisplayName);
            }

            // 更新頭像URL
            TryGetPhotoUrlAsync(staff, principal);

            await _context.SaveChangesAsync();

            _logger.LogInformation("成功更新員工資訊。StaffId: {StaffId}", staff.StaffId);

            return staff;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新員工資訊失敗。StaffId: {StaffId}, Error: {Error}", 
                staff.StaffId, ex.Message);
            throw;
        }
    }

    public async Task LogStaffLoginAsync(int staffId, string? ipAddress, string? userAgent)
    {
        _logger.LogInformation("記錄員工登入。StaffId: {StaffId}, IP: {IpAddress}, UserAgent: {UserAgent}", 
            staffId, ipAddress, userAgent);
        
        try
        {
            var staff = await _context.Staffs.FindAsync(staffId);
            if (staff != null)
            {
                staff.LastLoginAt = DateTimeExtensions.TaiwanTime;
                await _context.SaveChangesAsync();
                _logger.LogInformation("成功記錄員工登入時間。StaffId: {StaffId}, LastLoginAt: {LastLoginAt}", 
                    staffId, staff.LastLoginAt);
            }
            else
            {
                _logger.LogWarning("找不到員工資料，無法記錄登入時間。StaffId: {StaffId}", staffId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "記錄員工登入時發生錯誤。StaffId: {StaffId}, Error: {Error}", 
                staffId, ex.Message);
            // 不拋出異常，避免影響登入流程
        }
    }

    public async Task<List<Models.Staff>> GetAllForListAsync()
    {
        _logger.LogInformation("查詢所有員工清單");
        
        try
        {
            var staffList = await _context.Staffs
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
            
            _logger.LogInformation("成功取得員工清單。數量: {Count}", staffList.Count);
            
            return staffList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查詢員工清單時發生錯誤。Error: {Error}", ex.Message);
            throw;
        }
    }

    public async Task UpdateDepartmentAndJobTitleAsync(int staffId, string? department, string? jobTitle)
    {
        _logger.LogInformation("更新員工部門和職稱。StaffId: {StaffId}, Department: {Department}, JobTitle: {JobTitle}", 
            staffId, department, jobTitle);
        
        try
        {
            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.StaffId == staffId && !s.Deleted);
            if (staff == null)
            {
                _logger.LogWarning("找不到員工資料。StaffId: {StaffId}", staffId);
                throw new InvalidOperationException($"找不到員工：{staffId}");
            }

            var oldDepartment = staff.Department;
            var oldJobTitle = staff.JobTitle;

            staff.Department = department;
            staff.JobTitle = jobTitle;
            staff.UpdatedAt = DateTimeExtensions.TaiwanTime;

            await _context.SaveChangesAsync();

            _logger.LogInformation("成功更新員工部門和職稱。StaffId: {StaffId}, 舊部門: {OldDepartment}, 新部門: {NewDepartment}, 舊職稱: {OldJobTitle}, 新職稱: {NewJobTitle}", 
                staffId, oldDepartment, department, oldJobTitle, jobTitle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新員工部門和職稱時發生錯誤。StaffId: {StaffId}, Error: {Error}", 
                staffId, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 生成下一個遞增的員工編號（格式：001, 002, 003...）
    /// </summary>
    private async Task<string> GenerateNextEmployeeIdAsync()
    {
        _logger.LogDebug("開始生成下一個員工編號");
        
        try
        {
            // 查詢所有非 NULL 的 EmployeeId
            var existingEmployeeIds = await _context.Staffs
                .Where(s => s.EmployeeId != null && !s.Deleted)
                .Select(s => s.EmployeeId!)
                .ToListAsync();

            int maxEmployeeId = 0;

            // 找出所有可解析為數字的 EmployeeId 中的最大值
            foreach (var employeeId in existingEmployeeIds)
            {
                // 移除前導零後解析為數字
                if (int.TryParse(employeeId.TrimStart('0'), out int id) && id > maxEmployeeId)
                {
                    maxEmployeeId = id;
                }
            }

            var nextEmployeeId = (maxEmployeeId + 1).ToString("D3");
            _logger.LogDebug("生成員工編號完成。下一個編號: {NextEmployeeId}", nextEmployeeId);
            
            return nextEmployeeId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成員工編號時發生錯誤。Error: {Error}", ex.Message);
            throw;
        }
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
                _logger.LogDebug("從 Claims 取得頭像 URL。StaffId: {StaffId}, PhotoUrl: {PhotoUrl}", 
                    staff.StaffId, photoClaim);
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
                _logger.LogDebug("使用 Graph API URL 作為頭像。StaffId: {StaffId}, ObjectId: {ObjectId}", 
                    staff.StaffId, objectId);
            }
        }
        catch (Exception ex)
        {
            // 取得頭像失敗時不影響主要流程，但記錄警告
            _logger.LogWarning(ex, "取得頭像 URL 時發生錯誤，但不影響主要流程。StaffId: {StaffId}", 
                staff.StaffId);
        }
    }
}

