using Microsoft.AspNetCore.Mvc;
using CanLove_Backend.Infrastructure.Data.Contexts;
using CanLove_Backend.Domain.Case.Models.Opening;
using CanLove_Backend.Domain.Case.ViewModels.Basic;
using CanLove_Backend.Domain.Case.ViewModels.Opening;
using CanLove_Backend.Domain.Case.Services.Opening;
using CanLove_Backend.Domain.Case.Services.Opening.Steps;
using CanLove_Backend.Domain.Case.Exceptions;
using CanLove_Backend.Domain.Case.Shared.Services;
using Microsoft.EntityFrameworkCore;
using CanLove_Backend.Core.Extensions;

namespace CanLove_Backend.Application.Controllers.Case;

/// <summary>
/// 個案開案步驟表單控制器（Create/Edit 用 Wizard）
/// </summary>
public class CaseOpeningCreateEditController : CaseOpeningBaseController
{
    private readonly CanLoveDbContext _context;
    private readonly CaseDetailService _caseDetailService;
    private readonly SocialWorkerContentService _socialWorkerContentService;
    private readonly EconomicStatusService _economicStatusService;
    private readonly HealthStatusService _healthStatusService;
    private readonly AcademicPerformanceService _academicPerformanceService;
    private readonly EmotionalEvaluationService _emotionalEvaluationService;
    private readonly FinalAssessmentService _finalAssessmentService;

    public CaseOpeningCreateEditController(
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
    /// 新增開案記錄入口頁
    /// </summary>
    [HttpGet]
    public IActionResult Create()
    {
        // 導向 Step0 選擇個案
        return RedirectToAction("Create", new { step = "SelectCase" });
    }

    /// <summary>
    /// 共用核心方法：處理步驟路由的核心邏輯
    /// </summary>
    /// <param name="caseId">個案編號</param>
    /// <param name="step">步驟名稱</param>
    /// <param name="mode">表單模式</param>
    /// <param name="navigationContext">導航上下文（Create/Review/ReadOnly）</param>
    /// <returns>步驟視圖結果</returns>
    private async Task<IActionResult> GetStepActionResult(string? caseId, string step, CaseFormMode mode, string navigationContext)
    {
        // 設置 Sidebar 項目名稱
        ViewData["Sidebar.OpenCaseRecord"] = "開案紀錄表";
        
        // 設置導航上下文
        ViewData["NavigationContext"] = navigationContext;
        
        // 根據導航上下文設置 Sidebar 當前頁面
        ViewData["Sidebar.CurrentPage"] = navigationContext switch
        {
            "Review" => "Review",
            "ReadOnly" => "Query",
            "Edit" => "Query",
            _ => "Create"
        };
        
        // 根據導航上下文設置麵包屑父級
        ViewData["BreadcrumbParent"] = navigationContext switch
        {
            "Review" => "個案審核",
            "ReadOnly" => "查詢個案",
            "Edit" => "查詢個案",
            _ => "新增個案"
        };
        
        ViewData["BreadcrumbParentUrl"] = navigationContext switch
        {
            "Review" => Url.Action("Review", "CaseOpeningReview") ?? string.Empty,
            "ReadOnly" => Url.Action("View", "CaseOpeningQuery") ?? string.Empty,
            "Edit" => Url.Action("Query", "CaseOpeningQuery") ?? string.Empty,
            _ => Url.Action("Create", "CaseBasic") ?? string.Empty
        };
        
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

        // 根據 step 參數路由到對應的步驟方法（mode 完全由前端按鈕控制）
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
    /// 統一的新增入口 - 使用語義化步驟名稱
    /// </summary>
    [HttpGet]
    [Route("CaseOpening/Create/{step}")]
    public async Task<IActionResult> Create(string caseId, string step, CaseFormMode? mode = null)
    {
        var validatedMode = ValidateMode(mode);
        return await GetStepActionResult(caseId, step, validatedMode, "Create");
    }

    /// <summary>
    /// 編輯開案紀錄表入口 - 使用語義化步驟名稱
    /// </summary>
    [HttpGet]
    [Route("CaseOpening/Edit/{step}")]
    public async Task<IActionResult> Edit(string caseId, string step)
    {
        if (string.IsNullOrEmpty(caseId))
        {
            TempData["ErrorMessage"] = "個案編號不能為空";
            return RedirectToAction("Query", "CaseOpeningQuery");
        }

        // 檢查開案記錄是否存在
        var opening = await _context.CaseOpenings
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.CaseId == caseId);

        if (opening == null)
        {
            TempData["ErrorMessage"] = "找不到指定的開案記錄";
            return RedirectToAction("Query", "CaseOpeningQuery");
        }

        // 驗證狀態：只有 Draft 和 Rejected 可以編輯
        if (opening.Status != "Draft" && opening.Status != "Rejected")
        {
            TempData["ErrorMessage"] = $"此開案記錄狀態為「{GetStatusText(opening.Status)}」，無法編輯。";
            return RedirectToAction("Query", "CaseOpeningQuery");
        }

        return await GetStepActionResult(caseId, step, CaseFormMode.Create, "Edit");
    }

    /// <summary>
    /// 步驟0: 選擇個案
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Step0(string? caseId, CaseFormMode? mode = null)
    {
        // 設置 Sidebar 項目名稱
        ViewData["Sidebar.OpenCaseRecord"] = "開案紀錄表";
        
        var validatedMode = ValidateMode(mode);
        
        // 如果已有 caseId，檢查 CaseOpening 是否存在
        if (!string.IsNullOrEmpty(caseId))
        {
            if (await ValidateCaseExistsAsync(caseId))
            {
                var opening = await GetCaseOpeningAsync(caseId);
                
                // 如果 CaseOpening 已存在，直接導向 Step1
                if (opening != null)
                {
                    return RedirectToAction("Create", new { caseId, step = "CaseDetail", mode = validatedMode });
                }
            }
        }
        
        var viewModel = new CaseWizard_S0_SelectCase_VM
        {
            CaseId = caseId ?? string.Empty,
            Mode = validatedMode,
            CurrentStep = 0
        };
        
        return View("~/Views/Case/Opening/Step0.cshtml", viewModel);
    }

    /// <summary>
    /// 步驟0: 選擇個案 - POST
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Step0(CaseWizard_S0_SelectCase_VM model)
    {
        if (ModelState.IsValid)
        {
            // 使用資料庫交易確保資料一致性
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 驗證 caseId 是否存在於 Cases 表
                if (!await ValidateCaseExistsAsync(model.CaseId))
                {
                    ModelState.AddModelError("CaseId", "個案不存在");
                    return View("~/Views/Case/Opening/Step0.cshtml", model);
                }
                
                // 檢查 CaseOpening 是否已存在
                var opening = await GetCaseOpeningAsync(model.CaseId);
                
                if (opening == null)
                {
                    // 建立新的 CaseOpening 記錄，Status = "Draft"
                    opening = new CaseOpening
                    {
                        CaseId = model.CaseId,
                        Status = "Draft",
                        CreatedAt = DateTimeExtensions.TaiwanTime,
                        UpdatedAt = DateTimeExtensions.TaiwanTime
                    };
                    _context.CaseOpenings.Add(opening);
                }
                else
                {
                    // 如果已存在但 Status 不是 "Draft"，顯示警告
                    if (opening.Status != "Draft")
                    {
                        TempData["WarningMessage"] = $"此開案記錄狀態為「{GetStatusText(opening.Status)}」，將繼續編輯。";
                    }
                    
                    // 更新時間戳
                    opening.UpdatedAt = DateTimeExtensions.TaiwanTime;
                }
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                TempData["SuccessMessage"] = "個案選擇成功，請繼續填寫開案資料";
                var actionName = GetActionNameByMode(model.Mode);
                return RedirectToAction(actionName, new { caseId = model.CaseId, step = "CaseDetail" });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "建立開案記錄時發生錯誤，請稍後再試");
                return View("~/Views/Case/Opening/Step0.cshtml", model);
            }
        }
        
        return View("~/Views/Case/Opening/Step0.cshtml", model);
    }

    /// <summary>
    /// 步驟1: 個案詳細資料 (CaseDetail 表格)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Step1(string caseId, CaseFormMode? mode = null)
    {
        // 設置 Sidebar 項目名稱
        ViewData["Sidebar.OpenCaseRecord"] = "開案紀錄表";
        
        var validatedMode = ValidateMode(mode);
        
        // 如果沒有 caseId，導向 Step0
        if (string.IsNullOrEmpty(caseId))
        {
            return RedirectToAction("Create", new { step = "SelectCase", mode = validatedMode });
        }
        
        // 檢查個案是否存在
        if (!await ValidateCaseExistsAsync(caseId))
        {
            TempData["ErrorMessage"] = "個案不存在";
            return RedirectToAction("Create", new { step = "SelectCase", mode = validatedMode });
        }
        
        // 檢查 CaseOpening 是否存在，不存在則導向 Step0
        var opening = await GetCaseOpeningAsync(caseId);
        if (opening == null)
        {
            TempData["ErrorMessage"] = "請先完成步驟0選擇個案";
            return RedirectToAction("Create", new { step = "SelectCase", mode = validatedMode });
        }
        
        // 使用 Service 載入資料（包含選項資料）
        var viewModel = await _caseDetailService.GetStep1DataAsync(caseId);
        
        await SetCaseInfoAsync(caseId);
        viewModel.Mode = validatedMode;
        viewModel.CurrentStep = 1;
        return View("~/Views/Case/Opening/Step1.cshtml", viewModel);
    }

    /// <summary>
    /// 步驟1: 個案詳細資料 (CaseDetail 表格)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Step1(CaseDetailVM model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var (success, message) = await _caseDetailService.SaveStep1DataAsync(model);
                
                if (success)
                {
                    TempData["SuccessMessage"] = message;
                    var actionName = GetActionNameByMode(model.Mode);
                    return RedirectToAction(actionName, new { caseId = model.CaseId, step = "SocialWorkerContent" });
                }
            }
            catch (CaseOpeningNotFoundException)
            {
                ModelState.AddModelError("", "找不到對應的開案記錄，請先完成步驟0選擇個案");
            }
            catch (CaseOpeningSaveException)
            {
                // 記錄詳細錯誤到日誌（生產環境應使用 ILogger）
                // 但只回傳一般性錯誤訊息給使用者
                ModelState.AddModelError("", "儲存資料時發生錯誤，請稍後再試。如問題持續，請聯絡系統管理員。");
            }
            catch (Exception)
            {
                // 處理其他未預期的錯誤
                ModelState.AddModelError("", "發生未預期的錯誤，請稍後再試。如問題持續，請聯絡系統管理員。");
            }
        }

        await SetCaseInfoAsync(model.CaseId);
        return View("~/Views/Case/Opening/Step1.cshtml", model);
    }

    /// <summary>
    /// 步驟2: 社會工作服務內容 (CaseSocialWorkerContent 表格)
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
    /// 步驟2: 社會工作服務內容 (CaseSocialWorkerContent 表格)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Step2(SocialWorkerContentVM model)
    {
        if (ModelState.IsValid)
        {
            var (success, message) = await _socialWorkerContentService.SaveStep2DataAsync(model);
            
            if (success)
            {
                TempData["SuccessMessage"] = message;
                var actionName = GetActionNameByMode(model.Mode);
                return RedirectToAction(actionName, new { caseId = model.CaseId, step = "EconomicStatus" });
            }
            else
            {
                ModelState.AddModelError("", message);
            }
        }

        await SetCaseInfoAsync(model.CaseId);
        return View("~/Views/Case/Opening/Step2.cshtml", model);
    }

    /// <summary>
    /// 步驟3: 經濟狀況評估 (CaseFQeconomicStatus 表格)
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
    /// 步驟3: 經濟狀況評估 (CaseFQeconomicStatus 表格)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Step3(EconomicStatusVM model)
    {
        if (ModelState.IsValid)
        {
            var (success, message) = await _economicStatusService.SaveStep3DataAsync(model);
            
            if (success)
            {
                TempData["SuccessMessage"] = message;
                var actionName = GetActionNameByMode(model.Mode);
                return RedirectToAction(actionName, new { caseId = model.CaseId, step = "HealthStatus" });
            }
            else
            {
                ModelState.AddModelError("", message);
            }
        }

        await SetCaseInfoAsync(model.CaseId);
        return View("~/Views/Case/Opening/Step3.cshtml", model);
    }

    /// <summary>
    /// 步驟4: 健康狀況評估 (CaseHQhealthStatus 表格)
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
    /// 步驟4: 健康狀況評估 (CaseHQhealthStatus 表格)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Step4(HealthStatusVM model)
    {
        if (ModelState.IsValid)
        {
            var (success, message) = await _healthStatusService.SaveStep4DataAsync(model);
            
            if (success)
            {
                TempData["SuccessMessage"] = message;
                var actionName = GetActionNameByMode(model.Mode);
                return RedirectToAction(actionName, new { caseId = model.CaseId, step = "AcademicPerformance" });
            }
            else
            {
                ModelState.AddModelError("", message);
            }
        }

        await SetCaseInfoAsync(model.CaseId);
        return View("~/Views/Case/Opening/Step4.cshtml", model);
    }

    /// <summary>
    /// 步驟5: 學業表現評估 (CaseIQacademicPerformance 表格)
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
    /// 步驟5: 學業表現評估 (CaseIQacademicPerformance 表格)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Step5(AcademicPerformanceVM model)
    {
        if (ModelState.IsValid)
        {
            var (success, message) = await _academicPerformanceService.SaveStep5DataAsync(model);
            
            if (success)
            {
                TempData["SuccessMessage"] = message;
                var actionName = GetActionNameByMode(model.Mode);
                return RedirectToAction(actionName, new { caseId = model.CaseId, step = "EmotionalEvaluation" });
            }
            else
            {
                ModelState.AddModelError("", message);
            }
        }

        await SetCaseInfoAsync(model.CaseId);
        return View("~/Views/Case/Opening/Step5.cshtml", model);
    }

    /// <summary>
    /// 步驟6: 情緒評估 (CaseEQemotionalEvaluation 表格)
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
    /// 步驟6: 情緒評估 (CaseEQemotionalEvaluation 表格)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Step6(EmotionalEvaluationVM model)
    {
        if (ModelState.IsValid)
        {
            var (success, message) = await _emotionalEvaluationService.SaveStep6DataAsync(model);
            
            if (success)
            {
                TempData["SuccessMessage"] = message;
                var actionName = GetActionNameByMode(model.Mode);
                return RedirectToAction(actionName, new { caseId = model.CaseId, step = "FinalAssessment" });
            }
            else
            {
                ModelState.AddModelError("", message);
            }
        }

        await SetCaseInfoAsync(model.CaseId);
        return View("~/Views/Case/Opening/Step6.cshtml", model);
    }

    /// <summary>
    /// 步驟7: 最後評估表 (FinalAssessmentSummary 表格)
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
    /// 步驟7: 最後評估表 (FinalAssessmentSummary 表格)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Step7(FinalAssessmentVM model)
    {
        // Review 模式 + 審核按鈕（Approve / Reject）：優先處理審核決策
        var isReviewMode = model.Mode == CaseFormMode.Review;
        var isDecisionAction =
            !string.IsNullOrWhiteSpace(model.SubmitAction) &&
            (string.Equals(model.SubmitAction, "Approve", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(model.SubmitAction, "Reject", StringComparison.OrdinalIgnoreCase));

        if (!ModelState.IsValid && isReviewMode && isDecisionAction)
        {
            // 若有欄位驗證錯誤但仍按下審核按鈕，給出明確提示而不是靜默失敗
            TempData["ErrorMessage"] = "表單驗證失敗，無法進行審核決策，請檢查欄位後再試一次。";
            await SetCaseInfoAsync(model.CaseId);
            return View("~/Views/Case/Opening/Step7.cshtml", model);
        }

        if (ModelState.IsValid)
        {
            // 先獲取 CaseOpening 記錄（用於審核決策和送審邏輯）
            var opening = await GetCaseOpeningAsync(model.CaseId);
            if (opening == null)
            {
                ModelState.AddModelError("", "找不到對應的開案記錄，請先完成步驟0");
                await SetCaseInfoAsync(model.CaseId);
                return View("~/Views/Case/Opening/Step7.cshtml", model);
            }

            // 先保存 finalAssessment 資料（使用 Service）
            var (saveSuccess, saveMessage) = await _finalAssessmentService.SaveStep7DataAsync(model);
            if (!saveSuccess)
            {
                ModelState.AddModelError("", saveMessage);
                await SetCaseInfoAsync(model.CaseId);
                return View("~/Views/Case/Opening/Step7.cshtml", model);
            }

            // 處理不同模式的提交動作（優先處理審核決策）
            if (isReviewMode)
            {
                // 審核決策（Review 模式 Step7：SubmitAction = "Approve" 或 "Reject"）
                if (isDecisionAction)
                {
                    var approved = string.Equals(model.SubmitAction, "Approve", StringComparison.OrdinalIgnoreCase);
                    // 重定向到 CaseOpeningReviewController 的審核決策方法
                    // 注意：審核評論可以從表單的其他欄位獲取，或為 null（可選）
                    return RedirectToAction("ReviewItemDecision", "CaseOpeningReview", new 
                    { 
                        caseId = model.CaseId, 
                        approved = approved
                        // reviewComment 將從請求參數中獲取（如果有的話）
                    });
                }

                // Review 模式下，如果 SubmitAction 為空，只保存資料
                TempData["SuccessMessage"] = "資料已儲存";
                return RedirectToAction("Review", "CaseOpeningReview", new { caseId = model.CaseId });
            }
            else if (model.Mode == CaseFormMode.Create)
            {
                // 送審（Create 模式 Step7：SubmitAction = "Submit"）
                if (string.Equals(model.SubmitAction, "Submit", StringComparison.OrdinalIgnoreCase))
                {
                    opening.Status = "PendingReview";
                    opening.UpdatedAt = DateTimeExtensions.TaiwanTime;

                    opening.SubmittedBy = User.Identity?.Name ?? string.Empty;
                    opening.SubmittedAt = DateTimeExtensions.TaiwanTime;

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "開案資料已提交審閱";
                    return RedirectToAction("Create", "CaseOpeningCreateEdit");
                }

                TempData["SuccessMessage"] = "個案開案流程完成！";
                return RedirectToAction("Query", "CaseOpeningQuery");
            }

            // 其他模式（ReadOnly 等）只保存資料
            TempData["SuccessMessage"] = "資料已儲存";
            var defaultActionName = GetActionNameByMode(model.Mode);
            return RedirectToAction(defaultActionName, new { caseId = model.CaseId, step = "FinalAssessment" });
        }

        await SetCaseInfoAsync(model.CaseId);
        return View("~/Views/Case/Opening/Step7.cshtml", model);
    }

    /// <summary>
    /// 上一步
    /// </summary>
    [HttpPost]
    public IActionResult PreviousStep(int currentStep, string caseId, CaseFormMode? mode = null)
    {
        var stepNames = new Dictionary<int, string>
        {
            { 1, "CaseDetail" },
            { 2, "SocialWorkerContent" },
            { 3, "EconomicStatus" },
            { 4, "HealthStatus" },
            { 5, "AcademicPerformance" },
            { 6, "EmotionalEvaluation" },
            { 7, "FinalAssessment" }
        };
        
        var validatedMode = ValidateMode(mode);
        var actionName = GetActionNameByMode(validatedMode);
        
        return currentStep switch
        {
            2 => RedirectToAction(actionName, new { caseId, step = stepNames[1] }),
            3 => RedirectToAction(actionName, new { caseId, step = stepNames[2] }),
            4 => RedirectToAction(actionName, new { caseId, step = stepNames[3] }),
            5 => RedirectToAction(actionName, new { caseId, step = stepNames[4] }),
            6 => RedirectToAction(actionName, new { caseId, step = stepNames[5] }),
            7 => RedirectToAction(actionName, new { caseId, step = stepNames[6] }),
            _ => RedirectToAction(actionName, new { caseId, step = stepNames[1] })
        };
    }
}
