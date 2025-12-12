using Microsoft.AspNetCore.Mvc;
using CanLove_Backend.Infrastructure.Data.Contexts;
using CanLove_Backend.Domain.Case.Models.Opening;
using CanLove_Backend.Application.ViewModels.Case.Basic;
using CanLove_Backend.Domain.Case.Services.Opening;
using CanLove_Backend.Domain.Case.Shared.Services;
using CanLove_Backend.Core.Extensions;
using Microsoft.EntityFrameworkCore;

namespace CanLove_Backend.Application.Controllers.Case;

/// <summary>
/// 開案審核控制器：專責處理 CaseOpening 的審核流程
/// </summary>
public class CaseOpeningReviewController : CaseOpeningBaseController
{
    private readonly CanLoveDbContext _context;

    public CaseOpeningReviewController(
        CanLoveDbContext context,
        CaseOpeningValidationService validationService,
        CaseInfoService caseInfoService)
        : base(validationService, caseInfoService)
    {
        _context = context;
    }

    /// <summary>
    /// 開案審核頁面（列出所有 PendingReview 的開案記錄）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Review(string? caseId)
    {
        ViewData["Title"] = "個案審核 - 開案紀錄表";
        ViewBag.CurrentPage = "Review";
        ViewBag.CurrentTab = "CaseOpening";
        
        // 計算各類型的待審項目數量
        var caseBasicCount = await _context.Cases
            .Where(c => c.Status == "PendingReview" && c.Deleted != true)
            .CountAsync();
        
        var caseOpeningCount = await _context.CaseOpenings
            .Where(o => o.Status == "PendingReview")
            .CountAsync();
        
        // 設定 TypeCounts 供 _CaseFormTabs 使用
        ViewBag.TypeCounts = new Dictionary<string, int>
        {
            { "CaseBasic", caseBasicCount },
            { "CaseOpening", caseOpeningCount },
            { "CareVisitRecord", 0 }, // 功能未開發
            { "Consultation", 0 } // 功能未開發
        };
        
        // 查詢 CaseOpening 表中狀態為 PendingReview 的記錄
        var openings = await _context.CaseOpenings
            .Where(o => o.Status == "PendingReview")
            .Include(o => o.Case)
            .OrderByDescending(o => o.SubmittedAt)
            .AsNoTracking()
            .ToListAsync();

        // 取得提交者顯示名稱（以 Staff.DisplayName 對照 SubmittedBy Email）
        var emails = openings
            .Where(o => !string.IsNullOrWhiteSpace(o.SubmittedBy))
            .Select(o => o.SubmittedBy!)
            .Distinct()
            .ToList();

        var staffMap = await _context.Staffs
            .Where(s => emails.Contains(s.Email))
            .Select(s => new { s.Email, s.DisplayName })
            .ToDictionaryAsync(x => x.Email, x => x.DisplayName);

        ViewBag.SubmitterNameMap = staffMap;

        return View("~/Views/Case/Opening/Review/Index.cshtml", openings);
    }

    /// <summary>
    /// 審核開案紀錄表入口 - 使用語義化步驟名稱（Wizard 審核）
    /// 重定向到 CaseOpeningCreateEditController 處理 Wizard 流程
    /// </summary>
    [HttpGet]
    [Route("CaseOpening/Review/{step}")]
    public IActionResult Review(string caseId, string step)
    {
        // 重定向到 CaseOpeningCreateEditController 的 Create 方法，但使用 Review 模式
        // 注意：這裡使用 Create 方法，因為它會根據 navigationContext 處理不同的模式
        return RedirectToAction("Create", "CaseOpeningCreateEdit", new { caseId, step, mode = CaseFormMode.Review });
    }

    /// <summary>
    /// 審核開案記錄詳細資料（審核入口）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ReviewItem(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return View("NotFound");
        }

        // 檢查開案記錄是否存在且為待審閱狀態
        var opening = await _context.CaseOpenings
            .Include(o => o.Case)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.CaseId == id && o.Case != null && o.Case.Deleted != true);

        if (opening == null)
        {
            TempData["ErrorMessage"] = "找不到指定的開案記錄";
            return RedirectToAction(nameof(Review));
        }

        if (opening.Status != "PendingReview")
        {
            TempData["ErrorMessage"] = "此開案記錄不是待審閱狀態";
            return RedirectToAction(nameof(Review));
        }

        ViewBag.CaseId = id;
        return View("~/Views/Case/Opening/Review/Item.cshtml");
    }

    /// <summary>
    /// 重新送審開案記錄（被拒絕後重送）
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resubmit(string caseId)
    {
        if (string.IsNullOrWhiteSpace(caseId))
        {
            TempData["ErrorMessage"] = "個案編號不能為空";
            return RedirectToAction(nameof(Review));
        }

        var opening = await _context.CaseOpenings
            .FirstOrDefaultAsync(o => o.CaseId == caseId);

        if (opening == null)
        {
            TempData["ErrorMessage"] = "找不到指定的開案記錄";
            return RedirectToAction(nameof(Review));
        }

        // 驗證狀態必須是 Rejected
        if (opening.Status != "Rejected")
        {
            TempData["ErrorMessage"] = $"此開案記錄狀態為「{GetStatusText(opening.Status)}」，無法重新送審。只有被拒絕的記錄可以重新送審。";
            return RedirectToAction(nameof(Review));
        }

        // 更新狀態為 PendingReview
        opening.Status = "PendingReview";
        opening.SubmittedBy = User.Identity?.Name ?? string.Empty;
        opening.SubmittedAt = DateTimeExtensions.TaiwanTime;
        opening.UpdatedAt = DateTimeExtensions.TaiwanTime;

        // 清除審核資訊（因為重新送審）
        opening.ReviewedBy = null;
        opening.ReviewedAt = null;
        opening.ReviewComment = null;

        _context.Update(opening);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "開案記錄已重新送審";
        return RedirectToAction(nameof(Review));
    }

    /// <summary>
    /// 開案審核決策（直接更新 CaseOpening 表狀態）
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReviewItemDecision(string caseId, bool approved, string? reviewComment = null)
    {
        if (string.IsNullOrWhiteSpace(caseId))
        {
            TempData["ErrorMessage"] = "個案編號不能為空";
            return RedirectToAction(nameof(Review));
        }

        var opening = await _context.CaseOpenings.FirstOrDefaultAsync(o => o.CaseId == caseId);
        if (opening == null)
        {
            TempData["ErrorMessage"] = "找不到指定的開案記錄";
            return RedirectToAction(nameof(Review));
        }

        // 檢查狀態是否為待審閱
        if (opening.Status != "PendingReview")
        {
            TempData["ErrorMessage"] = "此開案記錄不是待審閱狀態";
            return RedirectToAction(nameof(Review));
        }

        var reviewer = User.Identity?.Name ?? string.Empty;
        
        // 確保實體被追蹤（如果未被追蹤，先附加到上下文）
        if (_context.Entry(opening).State == EntityState.Detached)
        {
            _context.Attach(opening);
        }
        
        // 僅更新 CaseOpening 表狀態與審核資訊
        opening.Status = approved ? "Approved" : "Rejected";
        opening.ReviewedBy = reviewer;
        opening.ReviewedAt = DateTimeExtensions.TaiwanTime;
        opening.ReviewComment = reviewComment;
        opening.UpdatedAt = DateTimeExtensions.TaiwanTime;
        
        // 如果拒絕，清除提交時間
        if (!approved)
        {
            opening.SubmittedAt = null;
        }

        // 保存變更（實體已被追蹤，EF Core 會自動偵測變更）
        var saveResult = await _context.SaveChangesAsync();
        
        // 驗證狀態是否正確更新（防範極端情況）
        if (saveResult > 0 && approved)
        {
            var verifyOpening = await _context.CaseOpenings
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.CaseId == caseId);
            
            if (verifyOpening != null && verifyOpening.Status != "Approved")
            {
                opening.Status = "Approved";
                await _context.SaveChangesAsync();
            }
        }

        TempData["SuccessMessage"] = approved ? "個案審核通過" : "個案已退回";
        return RedirectToAction(nameof(Review));
    }
}


