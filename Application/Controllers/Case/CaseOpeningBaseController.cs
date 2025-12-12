using Microsoft.AspNetCore.Mvc;
using CanLove_Backend.Domain.Case.Models.Opening;
using CanLove_Backend.Application.ViewModels.Case.Basic;
using CanLove_Backend.Domain.Case.Services.Opening;
using CanLove_Backend.Domain.Case.Shared.Services;

namespace CanLove_Backend.Application.Controllers.Case;

/// <summary>
/// 開案記錄控制器基礎類別：提供共享的基礎方法給所有子控制器繼承
/// </summary>
public abstract class CaseOpeningBaseController : Controller
{
    protected readonly CaseOpeningValidationService _validationService;
    protected readonly CaseInfoService _caseInfoService;

    protected CaseOpeningBaseController(
        CaseOpeningValidationService validationService,
        CaseInfoService caseInfoService)
    {
        _validationService = validationService;
        _caseInfoService = caseInfoService;
    }

    /// <summary>
    /// 驗證個案是否存在（使用驗證服務）
    /// </summary>
    protected async Task<bool> ValidateCaseExistsAsync(string caseId)
    {
        return await _validationService.ValidateCaseExistsAsync(caseId);
    }

    /// <summary>
    /// 取得開案記錄（使用驗證服務）
    /// </summary>
    protected async Task<CaseOpening?> GetCaseOpeningAsync(string caseId)
    {
        return await _validationService.GetCaseOpeningAsync(caseId);
    }

    /// <summary>
    /// 設置 ViewBag.CaseInfo（使用 CaseInfoService 載入個案基本資訊供 View 顯示）
    /// </summary>
    protected async Task SetCaseInfoAsync(string caseId)
    {
        ViewBag.CaseInfo = await _caseInfoService.GetCaseInfoAsync(caseId);
    }

    /// <summary>
    /// 取得狀態文字（使用驗證服務）
    /// </summary>
    protected string GetStatusText(string status)
    {
        return _validationService.GetStatusText(status);
    }

    /// <summary>
    /// 驗證 caseId 是否為空，如果為空則返回 NotFound
    /// </summary>
    protected IActionResult? ValidateCaseId(string? caseId)
    {
        if (string.IsNullOrEmpty(caseId))
        {
            return NotFound();
        }
        return null;
    }

    /// <summary>
    /// 根據 mode 取得對應的 action 名稱
    /// </summary>
    protected string GetActionNameByMode(CaseFormMode mode)
    {
        return mode switch
        {
            CaseFormMode.Review => "Review",   // 審核模式一律走 Review Wizard 入口
            CaseFormMode.ReadOnly => "View",
            _ => "Create"
        };
    }

    /// <summary>
    /// 驗證並返回模式（如果為 null 則返回預設值 Create）
    /// </summary>
    protected CaseFormMode ValidateMode(CaseFormMode? mode)
    {
        return mode ?? CaseFormMode.Create;
    }

    /// <summary>
    /// 取得 mode 的中文顯示文字
    /// </summary>
    protected string GetModeText(CaseFormMode mode)
    {
        return mode switch
        {
            CaseFormMode.Create => "新增",
            CaseFormMode.Review => "審核",
            CaseFormMode.ReadOnly => "檢視",
            _ => "未知"
        };
    }
}
