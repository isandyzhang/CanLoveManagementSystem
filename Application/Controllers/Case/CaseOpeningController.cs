using Microsoft.AspNetCore.Mvc;
using CanLove_Backend.Infrastructure.Data.Contexts;
using CanLove_Backend.Domain.Case.Models.Opening;
using CanLove_Backend.Domain.Case.ViewModels.Basic;
using CanLove_Backend.Domain.Case.ViewModels.Opening;
using CanLove_Backend.Domain.Case.Services.Opening.Steps;
using CanLove_Backend.Infrastructure.Storage.Blob;
using CanLove_Backend.Infrastructure.Storage.Encryption;
using CanLove_Backend.Infrastructure.Options.Services;
using CanLove_Backend.Domain.Case.Shared.Services;
using CanLove_Backend.Domain.Staff.Services;
using Microsoft.EntityFrameworkCore;
using CanLove_Backend.Core.Extensions;

namespace CanLove_Backend.Application.Controllers.Case;

/// <summary>
/// 個案開案步驟表單控制器
/// </summary>
public class CaseOpeningController : Controller
    {
        private readonly CanLoveDbContext _context;
        private readonly OptionService _optionService;
        private readonly CaseDetailService _caseDetailService;
        private readonly SocialWorkerContentService _socialWorkerContentService;
        private readonly EconomicStatusService _economicStatusService;
        private readonly HealthStatusService _healthStatusService;
        private readonly AcademicPerformanceService _academicPerformanceService;
        private readonly EmotionalEvaluationService _emotionalEvaluationService;
        private readonly FinalAssessmentService _finalAssessmentService;

        public CaseOpeningController(
            CanLoveDbContext context, 
            OptionService optionService, 
            CaseDetailService caseDetailService,
            SocialWorkerContentService socialWorkerContentService,
            EconomicStatusService economicStatusService,
            HealthStatusService healthStatusService,
            AcademicPerformanceService academicPerformanceService,
            EmotionalEvaluationService emotionalEvaluationService,
            FinalAssessmentService finalAssessmentService)
        {
            _context = context;
            _optionService = optionService;
            _caseDetailService = caseDetailService;
            _socialWorkerContentService = socialWorkerContentService;
            _economicStatusService = economicStatusService;
            _healthStatusService = healthStatusService;
            _academicPerformanceService = academicPerformanceService;
            _emotionalEvaluationService = emotionalEvaluationService;
            _finalAssessmentService = finalAssessmentService;
        }

        /// <summary>
        /// 載入個案基本資訊（包含 City, District, School）
        /// </summary>
        private async Task<CanLove_Backend.Domain.Case.Models.Basic.Case?> LoadCaseInfoAsync(string? caseId)
        {
            if (string.IsNullOrEmpty(caseId)) return null;
            return await _context.Cases
                .Include(c => c.City)
                .Include(c => c.District)
                .Include(c => c.School)
                .FirstOrDefaultAsync(c => c.CaseId == caseId && c.Deleted != true);
        }

        /// <summary>
        /// 驗證個案是否存在
        /// </summary>
        private async Task<bool> ValidateCaseExistsAsync(string caseId)
        {
            return await _context.Cases
                .AnyAsync(c => c.CaseId == caseId && c.Deleted != true);
        }

        /// <summary>
        /// 驗證 CaseOpening 是否存在
        /// </summary>
        private async Task<CaseOpening?> GetCaseOpeningAsync(string caseId)
        {
            return await _context.CaseOpenings
                .FirstOrDefaultAsync(o => o.CaseId == caseId);
        }

        /// <summary>
        /// 根據 mode 取得對應的 action 名稱
        /// </summary>
        private string GetActionNameByMode(CaseFormMode mode)
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
        private CaseFormMode ValidateMode(CaseFormMode? mode)
        {
            return mode ?? CaseFormMode.Create;
        }

        /// <summary>
        /// 驗證 caseId 是否為空，如果為空則返回 NotFound
        /// </summary>
        private IActionResult? ValidateCaseId(string? caseId)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                return NotFound();
            }
            return null;
        }

        /// <summary>
        /// 設置 ViewBag.CaseInfo（載入個案基本資訊供 View 顯示）
        /// </summary>
        private async Task SetCaseInfoAsync(string caseId)
        {
            ViewBag.CaseInfo = await LoadCaseInfoAsync(caseId);
        }

        /// <summary>
        /// 取得 mode 的中文顯示文字
        /// </summary>
        private string GetModeText(CaseFormMode mode)
        {
            return mode switch
            {
                CaseFormMode.Create => "新增",
                CaseFormMode.Review => "審核",
                CaseFormMode.ReadOnly => "檢視",
                _ => "未知"
            };
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
                "ReadOnly" => Url.Action("Query", "CaseOpening") ?? string.Empty,
                "Edit" => Url.Action("Query", "CaseOpening") ?? string.Empty,
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
        [Route("Create/{step}")]
        public async Task<IActionResult> Create(string caseId, string step, CaseFormMode? mode = null)
        {
            var validatedMode = ValidateMode(mode);
            return await GetStepActionResult(caseId, step, validatedMode, "Create");
        }

        /// <summary>
        /// 審核開案紀錄表入口 - 使用語義化步驟名稱（Wizard 審核）
        /// </summary>
        [HttpGet]
        [Route("Review/{step}")]
        public async Task<IActionResult> Review(string caseId, string step)
        {
            // 直接導向對應的步驟，不做狀態檢查
            // mode 完全由前端按鈕控制
            return await GetStepActionResult(caseId, step, CaseFormMode.Review, "Review");
        }

        /// <summary>
        /// 查看開案紀錄表入口 - 使用語義化步驟名稱（只讀模式）
        /// </summary>
        [HttpGet]
        [Route("View/{step}")]
        public async Task<IActionResult> View(string caseId, string step)
        {
            return await GetStepActionResult(caseId, step, CaseFormMode.ReadOnly, "ReadOnly");
        }

        /// <summary>
        /// 編輯開案紀錄表入口 - 使用語義化步驟名稱
        /// </summary>
        [HttpGet]
        [Route("Edit/{step}")]
        public async Task<IActionResult> Edit(string caseId, string step)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                TempData["ErrorMessage"] = "個案編號不能為空";
                return RedirectToAction("Query", "CaseOpening");
            }

            // 檢查開案記錄是否存在
            var opening = await _context.CaseOpenings
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.CaseId == caseId);

            if (opening == null)
            {
                TempData["ErrorMessage"] = "找不到指定的開案記錄";
                return RedirectToAction("Query", "CaseOpening");
            }

            // 驗證狀態：只有 Draft 和 Rejected 可以編輯
            if (opening.Status != "Draft" && opening.Status != "Rejected")
            {
                TempData["ErrorMessage"] = $"此開案記錄狀態為「{GetStatusText(opening.Status)}」，無法編輯。";
                return RedirectToAction("Query", "CaseOpening");
            }

            return await GetStepActionResult(caseId, step, CaseFormMode.Create, "Edit");
        }

        // 注意：POST 方法保留使用現有的 Step0-Step7 方法
        // 因為每個步驟的 ViewModel 類型不同，統一處理會很複雜
        // 表單提交時會直接 POST 到對應的 Step 方法

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
            
            var viewModel = new CaseWizard_S0_SelectCase_ViewModel
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
        public async Task<IActionResult> Step0(CaseWizard_S0_SelectCase_ViewModel model)
        {
            if (ModelState.IsValid)
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
                
                TempData["SuccessMessage"] = "個案選擇成功，請繼續填寫開案資料";
                var actionName = GetActionNameByMode(model.Mode);
                return RedirectToAction(actionName, new { caseId = model.CaseId, step = "CaseDetail" });
            }
            
            return View("~/Views/Case/Opening/Step0.cshtml", model);
        }
        
        /// <summary>
        /// 取得狀態文字
        /// </summary>
        private string GetStatusText(string status)
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
                var (success, message) = await _caseDetailService.SaveStep1DataAsync(model);
                
                if (success)
                {
                    TempData["SuccessMessage"] = message;
                    var actionName = GetActionNameByMode(model.Mode);
                    return RedirectToAction(actionName, new { caseId = model.CaseId, step = "SocialWorkerContent" });
                }
                else
                {
                    ModelState.AddModelError("", message);
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
                        return await ReviewItemDecision(model.CaseId, approved);
                    }

                    // Review 模式下，如果 SubmitAction 為空，只保存資料
                    TempData["SuccessMessage"] = "資料已儲存";
                    var reviewActionName = GetActionNameByMode(model.Mode);
                    return RedirectToAction(reviewActionName, new { caseId = model.CaseId, step = "FinalAssessment" });
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
                        return RedirectToAction("Create", "CaseOpening");
                    }

                    TempData["SuccessMessage"] = "個案開案流程完成！";
                    return RedirectToAction("Query", "CaseOpening");
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
        /// 編輯開案記錄入口（重定向到新的 Edit 方法）
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

            // 重定向到新的 Edit 方法
            return RedirectToAction("Edit", new { caseId = id, step = "CaseDetail" });
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
                return RedirectToAction(nameof(Query));
            }

            var opening = await _context.CaseOpenings
                .FirstOrDefaultAsync(o => o.CaseId == caseId);

            if (opening == null)
            {
                TempData["ErrorMessage"] = "找不到指定的開案記錄";
                return RedirectToAction(nameof(Query));
            }

            // 驗證狀態必須是 Rejected
            if (opening.Status != "Rejected")
            {
                TempData["ErrorMessage"] = $"此開案記錄狀態為「{GetStatusText(opening.Status)}」，無法重新送審。只有被拒絕的記錄可以重新送審。";
                return RedirectToAction(nameof(Query));
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
            return RedirectToAction(nameof(Query));
        }

        /// <summary>
        /// 開案審核頁面
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