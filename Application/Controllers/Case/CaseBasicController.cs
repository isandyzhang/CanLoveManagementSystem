using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CanLove_Backend.Infrastructure.Data.Contexts;
using CanLove_Backend.Domain.Case.Models.Basic;
using CanLove_Backend.Domain.Case.Services.Basic;
using CanLove_Backend.Domain.Case.Shared.Services;
using CanLove_Backend.Domain.Case.Exceptions;
using CaseEntity = CanLove_Backend.Domain.Case.Models.Basic.Case;

namespace CanLove_Backend.Application.Controllers.Case;

/// <summary>
/// 個案基本資料管理控制器（保留其他操作：提交審核、鎖定/解鎖、刪除、詳情）
/// </summary>
public class CaseBasicController : CaseBasicBaseController
{
    private readonly CanLoveDbContext _context;
    private readonly ICaseBasicService _caseService;

    public CaseBasicController(
        CanLoveDbContext context,
        ICaseBasicService caseService,
        CaseBasicValidationService validationService,
        CaseBasicOptionsService optionsService,
        CaseInfoService caseInfoService)
        : base(validationService, optionsService, caseInfoService)
    {
        _context = context;
        _caseService = caseService;
    }

    /// <summary>
    /// 提交個案審核
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize] // TODO: 之後可根據需求改為 [Authorize(Policy = "RequireAssistant")]
    public async Task<IActionResult> SubmitForReview(string id)
    {
        try
        {
            var result = await _caseService.SubmitForReviewAsync(id, User.Identity?.Name);
            
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }
            
            return RedirectToAction("Query", "CaseBasicQuery");
        }
        catch (CaseBasicException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Query", "CaseBasicQuery");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"提交審核時發生錯誤：{ex.Message}";
            return RedirectToAction("Query", "CaseBasicQuery");
        }
    }

    /// <summary>
    /// 鎖定/解鎖個案
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize] // TODO: 之後可根據需求改為 [Authorize(Policy = "RequireSocialWorker")]
    public async Task<IActionResult> ToggleLock(string id)
    {
        try
        {
            var result = await _caseService.ToggleLockAsync(id, User.Identity?.Name);
            
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }
            
            return RedirectToAction("Query", "CaseBasicQuery");
        }
        catch (CaseBasicException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Query", "CaseBasicQuery");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"鎖定/解鎖個案時發生錯誤：{ex.Message}";
            return RedirectToAction("Query", "CaseBasicQuery");
        }
    }

    /// <summary>
    /// 個案刪除頁面
    /// </summary>
    [HttpGet]
    [Authorize] // TODO: 之後可根據需求改為 [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> Delete(string id)
    {
        var validationResult = ValidateCaseId(id);
        if (validationResult != null)
        {
            return validationResult;
        }

        var caseItem = await _context.Cases
            .Include(c => c.City)
            .Include(c => c.District)
            .Include(c => c.School)
            .FirstOrDefaultAsync(m => m.CaseId == id);

        if (caseItem == null)
        {
            return View("NotFound");
        }

        return View("~/Views/Case/Basic/Details.cshtml", caseItem);
    }

    /// <summary>
    /// 個案刪除處理 (軟刪除)
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [Authorize] // TODO: 之後可根據需求改為 [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        try
        {
            var result = await _caseService.DeleteCaseAsync(id, User.Identity?.Name);
            
            if (result.Success)
            {
                var caseItem = await _caseService.GetCaseForEditAsync(id);
                return Json(new { 
                    success = true, 
                    message = result.Message,
                    caseId = id,
                    deletedAt = caseItem?.DeletedAt?.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            else
            {
                return Json(new { success = false, message = result.Message });
            }
        }
        catch (CaseBasicException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"刪除個案時發生錯誤：{ex.Message}" });
        }
    }

    /// <summary>
    /// 個案詳情頁面
    /// </summary>
    [Authorize] // TODO: 之後可根據需求改為 [Authorize(Policy = "RequireViewer")]
    public async Task<IActionResult> Details(string id)
    {
        var validationResult = ValidateCaseId(id);
        if (validationResult != null)
        {
            return validationResult;
        }

        var caseItem = await _context.Cases
            .Include(c => c.City)
            .Include(c => c.District)
            .Include(c => c.School)
            .FirstOrDefaultAsync(m => m.CaseId == id);

        if (caseItem == null)
        {
            return View("NotFound");
        }

        return View("~/Views/Case/Basic/Details.cshtml", caseItem);
    }
}
