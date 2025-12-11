using Microsoft.AspNetCore.Mvc;
using CanLove_Backend.Domain.Case.Models.Basic;
using CanLove_Backend.Domain.Case.ViewModels.Basic;
using CanLove_Backend.Domain.Case.Services.Basic;
using CanLove_Backend.Domain.Case.Shared.Services;
using CaseEntity = CanLove_Backend.Domain.Case.Models.Basic.Case;
using CaseBasicOptionsData = CanLove_Backend.Domain.Case.Services.Basic.CaseBasicOptionsData;

namespace CanLove_Backend.Application.Controllers.Case;

/// <summary>
/// 個案基本資料控制器基礎類別：提供共享的基礎方法給所有子控制器繼承
/// </summary>
public abstract class CaseBasicBaseController : Controller
{
    protected readonly CaseBasicValidationService _validationService;
    protected readonly CaseBasicOptionsService _optionsService;
    protected readonly CaseInfoService _caseInfoService;

    protected CaseBasicBaseController(
        CaseBasicValidationService validationService,
        CaseBasicOptionsService optionsService,
        CaseInfoService caseInfoService)
    {
        _validationService = validationService;
        _optionsService = optionsService;
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
    /// 取得個案（使用驗證服務）
    /// </summary>
    protected async Task<CaseEntity?> GetCaseAsync(string caseId)
    {
        return await _validationService.GetCaseAsync(caseId);
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
    /// 統一載入選項資料並設置到 ViewBag
    /// </summary>
    protected async Task<CaseBasicOptionsData> LoadOptionsDataAsync()
    {
        var optionsData = await _optionsService.GetAllOptionsAsync();
        ViewBag.DistrictsByCity = optionsData.DistrictsByCity;
        return optionsData;
    }

    /// <summary>
    /// 設置導航上下文（麵包屑、Sidebar 等）
    /// </summary>
    /// <param name="context">導航上下文：Create、Edit、Query、Review、ReadOnly</param>
    protected void SetNavigationContext(string context)
    {
        ViewData["BreadcrumbParent"] = context switch
        {
            "Review" => "個案審核",
            "ReadOnly" => "查詢個案",
            "Edit" => "查詢個案",
            "Query" => "個案管理",
            _ => "新增個案"
        };

        ViewData["BreadcrumbParentUrl"] = context switch
        {
            "Review" => Url.Action("Review", "CaseBasicReview") ?? string.Empty,
            "ReadOnly" => Url.Action("Query", "CaseBasicQuery") ?? string.Empty,
            "Edit" => Url.Action("Query", "CaseBasicQuery") ?? string.Empty,
            "Query" => Url.Action("Query", "CaseBasicQuery") ?? string.Empty,
            _ => Url.Action("Create", "CaseBasicCreateEdit") ?? string.Empty
        };

        ViewData["Sidebar.CurrentPage"] = context switch
        {
            "Review" => "Review",
            "ReadOnly" => "Query",
            "Edit" => "Query",
            "Query" => "Search",
            _ => "Create"
        };
    }

    /// <summary>
    /// 根據 mode 取得對應的 action 名稱
    /// </summary>
    protected string GetActionNameByMode(CaseFormMode mode)
    {
        return mode switch
        {
            CaseFormMode.Review => "Review",
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
