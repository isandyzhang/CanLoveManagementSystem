using Microsoft.EntityFrameworkCore;
using CanLove_Backend.Infrastructure.Data.Contexts;
using CanLove_Backend.Domain.Case.Models.Opening;

namespace CanLove_Backend.Domain.Case.Services.Opening;

/// <summary>
/// 開案記錄驗證服務 - 統一處理驗證邏輯
/// </summary>
public class CaseOpeningValidationService
{
    private readonly CanLoveDbContext _context;

    public CaseOpeningValidationService(CanLoveDbContext context)
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
    /// 取得開案記錄（如果存在）
    /// </summary>
    public async Task<CaseOpening?> GetCaseOpeningAsync(string caseId)
    {
        if (string.IsNullOrWhiteSpace(caseId))
        {
            return null;
        }

        return await _context.CaseOpenings
            .FirstOrDefaultAsync(o => o.CaseId == caseId);
    }

    /// <summary>
    /// 驗證個案是否存在並取得開案記錄（合併查詢）
    /// </summary>
    public async Task<(bool CaseExists, CaseOpening? Opening)> ValidateCaseAndGetOpeningAsync(string caseId)
    {
        if (string.IsNullOrWhiteSpace(caseId))
        {
            return (false, null);
        }

        // 先檢查個案是否存在
        var caseExists = await _context.Cases
            .AnyAsync(c => c.CaseId == caseId && c.Deleted != true);

        if (!caseExists)
        {
            return (false, null);
        }

        // 如果個案存在，取得開案記錄
        var opening = await _context.CaseOpenings
            .FirstOrDefaultAsync(o => o.CaseId == caseId);

        return (true, opening);
    }

    /// <summary>
    /// 驗證開案記錄狀態是否允許編輯
    /// </summary>
    public bool CanEdit(CaseOpening? opening)
    {
        if (opening == null)
        {
            return false;
        }

        // 只有 Draft 和 Rejected 狀態可以編輯
        return opening.Status == "Draft" || opening.Status == "Rejected";
    }

    /// <summary>
    /// 驗證開案記錄狀態是否允許審核
    /// </summary>
    public bool CanReview(CaseOpening? opening)
    {
        if (opening == null)
        {
            return false;
        }

        return opening.Status == "PendingReview";
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
            "Closed" => "已結案",
            _ => "草稿"
        };
    }
}
