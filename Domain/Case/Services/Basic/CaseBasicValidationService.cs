using Microsoft.EntityFrameworkCore;
using CanLove_Backend.Infrastructure.Data.Contexts;
using CanLove_Backend.Domain.Case.Models.Basic;
using CaseEntity = CanLove_Backend.Domain.Case.Models.Basic.Case;

namespace CanLove_Backend.Domain.Case.Services.Basic;

/// <summary>
/// 個案基本資料驗證服務 - 統一處理驗證邏輯
/// </summary>
public class CaseBasicValidationService
{
    private readonly CanLoveDbContext _context;

    public CaseBasicValidationService(CanLoveDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 驗證個案是否存在
    /// </summary>
    public async Task<bool> ValidateCaseExistsAsync(string caseId)
    {
        if (string.IsNullOrWhiteSpace(caseId))
        {
            return false;
        }

        return await _context.Cases
            .AnyAsync(c => c.CaseId == caseId && c.Deleted != true);
    }

    /// <summary>
    /// 取得個案（如果存在）
    /// </summary>
    public async Task<CaseEntity?> GetCaseAsync(string caseId)
    {
        if (string.IsNullOrWhiteSpace(caseId))
        {
            return null;
        }

        return await _context.Cases
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CaseId == caseId && c.Deleted != true);
    }

    /// <summary>
    /// 驗證個案狀態是否允許編輯
    /// </summary>
    public bool CanEdit(CaseEntity? caseItem)
    {
        if (caseItem == null)
        {
            return false;
        }

        // 只有 Draft、Rejected 和 Approved 狀態可以編輯
        return caseItem.Status == "Draft" || 
               caseItem.Status == "Rejected" || 
               caseItem.Status == "Approved";
    }

    /// <summary>
    /// 驗證個案狀態是否允許審核
    /// </summary>
    public bool CanReview(CaseEntity? caseItem)
    {
        if (caseItem == null)
        {
            return false;
        }

        return caseItem.Status == "PendingReview";
    }

    /// <summary>
    /// 驗證個案是否被鎖定
    /// </summary>
    public bool IsLocked(CaseEntity? caseItem, string? currentUser)
    {
        if (caseItem == null)
        {
            return false;
        }

        if (caseItem.IsLocked != true)
        {
            return false;
        }

        // 如果被鎖定，檢查是否為當前使用者鎖定
        return caseItem.LockedBy != currentUser;
    }

    /// <summary>
    /// 驗證外鍵是否存在
    /// </summary>
    public async Task<(bool IsValid, string? ErrorMessage)> ValidateForeignKeysAsync(int? cityId, int? districtId, int? schoolId)
    {
        if (cityId.HasValue)
        {
            var cityExists = await _context.Cities.AnyAsync(c => c.CityId == cityId.Value);
            if (!cityExists)
            {
                return (false, $"城市 ID {cityId.Value} 不存在");
            }
        }

        if (districtId.HasValue)
        {
            var districtExists = await _context.Districts.AnyAsync(d => d.DistrictId == districtId.Value);
            if (!districtExists)
            {
                return (false, $"地區 ID {districtId.Value} 不存在");
            }
        }

        if (schoolId.HasValue)
        {
            var schoolExists = await _context.Schools.AnyAsync(s => s.SchoolId == schoolId.Value);
            if (!schoolExists)
            {
                return (false, $"學校 ID {schoolId.Value} 不存在");
            }
        }

        return (true, null);
    }

    /// <summary>
    /// 驗證 CaseId 是否已存在
    /// </summary>
    public async Task<bool> CaseIdExistsAsync(string caseId)
    {
        if (string.IsNullOrWhiteSpace(caseId))
        {
            return false;
        }

        return await _context.Cases
            .AnyAsync(c => c.CaseId == caseId);
    }

    /// <summary>
    /// 取得狀態文字
    /// </summary>
    public string GetStatusText(string status)
    {
        return status switch
        {
            "PendingReview" => "待審閱",
            "Approved" => "已審核",
            "Rejected" => "已退回",
            "Draft" => "草稿",
            "Closed" => "已結案",
            _ => "未知"
        };
    }
}
