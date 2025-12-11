using Microsoft.AspNetCore.Mvc;
using CanLove_Backend.Infrastructure.Data.Contexts;
using CanLove_Backend.Domain.Case.Models.Opening;
using CanLove_Backend.Domain.Case.ViewModels.Basic;
using CanLove_Backend.Domain.Case.ViewModels.Opening;
using CanLove_Backend.Domain.Case.Services.Opening;
using CanLove_Backend.Domain.Case.Services.Opening.Steps;
using CanLove_Backend.Domain.Case.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace CanLove_Backend.Application.Controllers.Case;

/// <summary>
/// 開案記錄查詢控制器：專責處理查詢、搜尋和查看功能
/// </summary>
public class CaseOpeningQueryController : CaseOpeningBaseController
{
    private readonly CanLoveDbContext _context;
    private readonly CaseDetailService _caseDetailService;
    private readonly SocialWorkerContentService _socialWorkerContentService;
    private readonly EconomicStatusService _economicStatusService;
    private readonly HealthStatusService _healthStatusService;
    private readonly AcademicPerformanceService _academicPerformanceService;
    private readonly EmotionalEvaluationService _emotionalEvaluationService;
    private readonly FinalAssessmentService _finalAssessmentService;

    public CaseOpeningQueryController(
        CanLoveDbContext context,
        CaseDetailService caseDetailService,
        SocialWorkerContentService socialWorkerContentService,
        EconomicStatusService economicStatusService,
        HealthStatusService healthStatusService,
        AcademicPerformanceService academicPerformanceService,
        EmotionalEvaluationService emotionalEvaluationService,
        FinalAssessmentService finalAssessmentService,
        CaseOpeningValidationService validationService,
        CaseInfoService caseInfoService)
        : base(validationService, caseInfoService)
    {
        _context = context;
        _caseDetailService = caseDetailService;
        _socialWorkerContentService = socialWorkerContentService;
        _economicStatusService = economicStatusService;
        _healthStatusService = healthStatusService;
        _academicPerformanceService = academicPerformanceService;
        _emotionalEvaluationService = emotionalEvaluationService;
        _finalAssessmentService = finalAssessmentService;
    }

    /// <summary>
    /// 共用核心方法：處理步驟路由的核心邏輯（只讀模式）
    /// </summary>
    /// <param name="caseId">個案編號</param>
    /// <param name="step">步驟名稱</param>
    /// <param name="mode">表單模式</param>
    /// <param name="navigationContext">導航上下文（ReadOnly）</param>
    /// <returns>步驟視圖結果</returns>
    private async Task<IActionResult> GetStepActionResult(string? caseId, string step, CaseFormMode mode, string navigationContext)
    {
        // 設置 Sidebar 項目名稱
        ViewData["Sidebar.OpenCaseRecord"] = "開案紀錄表";
        
        // 設置導航上下文
        ViewData["NavigationContext"] = navigationContext;
        
        // 根據導航上下文設置 Sidebar 當前頁面
        ViewData["Sidebar.CurrentPage"] = "Query";
        
        // 根據導航上下文設置麵包屑父級
        ViewData["BreadcrumbParent"] = "查詢個案";
        ViewData["BreadcrumbParentUrl"] = Url.Action("Query", "CaseOpeningQuery") ?? string.Empty;
        
        // 如果有 caseId，檢查個案是否存在
        string? validCaseId = caseId;
        if (!string.IsNullOrEmpty(caseId))
        {
            var caseExists = await _context.Cases
                .AnyAsync(c => c.CaseId == caseId && c.Deleted != true);
            
            if (!caseExists)
            {
                TempData["ErrorMessage"] = "個案不存在";
                validCaseId = null; // 清除無效的 caseId
            }
        }

        // 根據 step 參數路由到對應的步驟方法（只讀模式）
        return step switch
        {
            "SelectCase" => await Step0(validCaseId, mode),
            "CaseDetail" => await Step1(validCaseId ?? string.Empty, mode),
            "SocialWorkerContent" => await Step2(validCaseId ?? string.Empty, mode),
            "EconomicStatus" => await Step3(validCaseId ?? string.Empty, mode),
            "HealthStatus" => await Step4(validCaseId ?? string.Empty, mode),
            "AcademicPerformance" => await Step5(validCaseId ?? string.Empty, mode),
            "EmotionalEvaluation" => await Step6(validCaseId ?? string.Empty, mode),
            "FinalAssessment" => await Step7(validCaseId ?? string.Empty, mode),
            _ => NotFound()
        };
    }

    /// <summary>
    /// 查看開案紀錄表入口 - 使用語義化步驟名稱（只讀模式）
    /// </summary>
    [HttpGet]
    [Route("CaseOpening/View/{step}")]
    public async Task<IActionResult> View(string caseId, string step)
    {
        return await GetStepActionResult(caseId, step, CaseFormMode.ReadOnly, "ReadOnly");
    }

    /// <summary>
    /// 步驟0: 選擇個案（只讀模式）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Step0(string? caseId, CaseFormMode? mode = null)
    {
        ViewData["Sidebar.OpenCaseRecord"] = "開案紀錄表";
        var validatedMode = ValidateMode(mode);
        
        var viewModel = new CaseWizard_S0_SelectCase_VM
        {
            CaseId = caseId ?? string.Empty,
            Mode = validatedMode,
            CurrentStep = 0
        };
        
        return View("~/Views/Case/Opening/Step0.cshtml", viewModel);
    }

    /// <summary>
    /// 步驟1: 個案詳細資料（只讀模式）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Step1(string caseId, CaseFormMode? mode = null)
    {
        ViewData["Sidebar.OpenCaseRecord"] = "開案紀錄表";
        var validatedMode = ValidateMode(mode);
        var validationResult = ValidateCaseId(caseId);
        if (validationResult != null) return validationResult;

        var viewModel = await _caseDetailService.GetStep1DataAsync(caseId);
        viewModel.Mode = validatedMode;
        viewModel.CurrentStep = 1;

        await SetCaseInfoAsync(caseId);
        return View("~/Views/Case/Opening/Step1.cshtml", viewModel);
    }

    /// <summary>
    /// 步驟2: 社會工作服務內容（只讀模式）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Step2(string caseId, CaseFormMode? mode = null)
    {
        var validatedMode = ValidateMode(mode);
        var validationResult = ValidateCaseId(caseId);
        if (validationResult != null) return validationResult;

        var viewModel = await _socialWorkerContentService.GetStep2DataAsync(caseId);
        viewModel.Mode = validatedMode;
        viewModel.CurrentStep = 2;

        await SetCaseInfoAsync(caseId);
        return View("~/Views/Case/Opening/Step2.cshtml", viewModel);
    }

    /// <summary>
    /// 步驟3: 經濟狀況評估（只讀模式）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Step3(string caseId, CaseFormMode? mode = null)
    {
        var validatedMode = ValidateMode(mode);
        var validationResult = ValidateCaseId(caseId);
        if (validationResult != null) return validationResult;

        var viewModel = await _economicStatusService.GetStep3DataAsync(caseId);
        viewModel.Mode = validatedMode;
        viewModel.CurrentStep = 3;

        await SetCaseInfoAsync(caseId);
        return View("~/Views/Case/Opening/Step3.cshtml", viewModel);
    }

    /// <summary>
    /// 步驟4: 健康狀況評估（只讀模式）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Step4(string caseId, CaseFormMode? mode = null)
    {
        var validatedMode = ValidateMode(mode);
        var validationResult = ValidateCaseId(caseId);
        if (validationResult != null) return validationResult;

        var viewModel = await _healthStatusService.GetStep4DataAsync(caseId);
        viewModel.Mode = validatedMode;
        viewModel.CurrentStep = 4;

        await SetCaseInfoAsync(caseId);
        return View("~/Views/Case/Opening/Step4.cshtml", viewModel);
    }

    /// <summary>
    /// 步驟5: 學業表現評估（只讀模式）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Step5(string caseId, CaseFormMode? mode = null)
    {
        var validatedMode = ValidateMode(mode);
        var validationResult = ValidateCaseId(caseId);
        if (validationResult != null) return validationResult;

        var viewModel = await _academicPerformanceService.GetStep5DataAsync(caseId);
        viewModel.Mode = validatedMode;
        viewModel.CurrentStep = 5;

        await SetCaseInfoAsync(caseId);
        return View("~/Views/Case/Opening/Step5.cshtml", viewModel);
    }

    /// <summary>
    /// 步驟6: 情緒評估（只讀模式）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Step6(string caseId, CaseFormMode? mode = null)
    {
        var validatedMode = ValidateMode(mode);
        var validationResult = ValidateCaseId(caseId);
        if (validationResult != null) return validationResult;

        var viewModel = await _emotionalEvaluationService.GetStep6DataAsync(caseId);
        viewModel.Mode = validatedMode;
        viewModel.CurrentStep = 6;

        await SetCaseInfoAsync(caseId);
        return View("~/Views/Case/Opening/Step6.cshtml", viewModel);
    }

    /// <summary>
    /// 步驟7: 最後評估表（只讀模式）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Step7(string caseId, CaseFormMode? mode = null)
    {
        var validatedMode = ValidateMode(mode);
        var validationResult = ValidateCaseId(caseId);
        if (validationResult != null) return validationResult;

        var viewModel = await _finalAssessmentService.GetStep7DataAsync(caseId);
        viewModel.Mode = validatedMode;
        viewModel.CurrentStep = 7;

        await SetCaseInfoAsync(caseId);
        return View("~/Views/Case/Opening/Step7.cshtml", viewModel);
    }

    /// <summary>
    /// 查詢開案記錄
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Query(string? caseId = null, bool showAll = false)
    {
        ViewData["Title"] = "查詢個案 - 開案紀錄";
        ViewBag.CurrentPage = "Search";
        ViewBag.CurrentTab = "CaseOpening";
        
        // 載入所有開案記錄列表（預設一進頁就顯示全部）
        try
        {
            var allOpenings = await _context.CaseOpenings
                .Include(o => o.Case)
                .Where(o => o.Case != null && o.Case.Deleted != true)
                .OrderByDescending(o => o.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
            
            ViewBag.AllOpenings = allOpenings;
            ViewBag.ShowAllOpenings = true;
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = $"查詢資料時發生錯誤：{ex.Message}";
            ViewBag.AllOpenings = new List<CaseOpening>();
        }
        
        return View("~/Views/Case/Opening/Search/Index.cshtml");
    }

    /// <summary>
    /// 搜尋開案記錄 API（AJAX）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> SearchOpenings(string query)
    {
        try
        {
            var queryable = _context.CaseOpenings
                .Include(o => o.Case)
                .Where(o => o.Case != null && o.Case.Deleted != true);

            // 如果有查詢條件，加入搜尋過濾
            if (!string.IsNullOrWhiteSpace(query))
            {
                var searchTerm = query.Trim();
                queryable = queryable.Where(o =>
                    o.CaseId.Contains(searchTerm) ||
                    (o.Case != null && o.Case.Name != null && o.Case.Name.Contains(searchTerm)) ||
                    (o.Case != null && o.Case.Phone != null && o.Case.Phone.Contains(searchTerm))
                );
            }

            var openings = await queryable
                .OrderByDescending(o => o.CreatedAt)
                .Take(200)
                .ToListAsync();
            
            var result = openings.Select(o => new
            {
                caseId = o.CaseId,
                name = o.Case != null ? o.Case.Name : "",
                openDate = o.OpenDate.HasValue ? o.OpenDate.Value.ToString("yyyy-MM-dd") : "",
                status = o.Status ?? "Draft",
                statusText = o.Status switch
                {
                    "Approved" => "已開案",
                    "PendingReview" => "審核中",
                    "Rejected" => "被拒絕",
                    "Closed" => "已結案",
                    _ => "草稿"
                }
            }).ToList();

            return Json(new { success = true, openings = result });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"搜尋失敗：{ex.Message}" });
        }
    }

    /// <summary>
    /// 查看開案記錄詳細資料（只讀模式）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> SearchItem(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return View("NotFound");
        }

        // 檢查開案記錄是否存在
        var opening = await _context.CaseOpenings
            .Include(o => o.Case)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.CaseId == id && o.Case != null && o.Case.Deleted != true);

        if (opening == null)
        {
            TempData["ErrorMessage"] = "找不到指定的開案記錄";
            return RedirectToAction(nameof(Query));
        }

        ViewBag.CaseId = id;
        return View("~/Views/Case/Opening/Search/Item.cshtml");
    }

    /// <summary>
    /// 編輯開案記錄入口（重定向到 CreateEditController）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> EditItem(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return View("NotFound");
        }

        // 檢查開案記錄是否存在
        var opening = await _context.CaseOpenings
            .Include(o => o.Case)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.CaseId == id && o.Case != null && o.Case.Deleted != true);

        if (opening == null)
        {
            TempData["ErrorMessage"] = "找不到指定的開案記錄";
            return RedirectToAction(nameof(Query));
        }

        // 重定向到 CreateEditController 的 Edit 方法
        return RedirectToAction("Edit", "CaseOpeningCreateEdit", new { caseId = id, step = "CaseDetail" });
    }
}
