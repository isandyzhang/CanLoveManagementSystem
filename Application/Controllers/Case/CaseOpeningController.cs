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
        private readonly CaseWizard_S1_CD_Service _step1Service;

        public CaseOpeningController(CanLoveDbContext context, OptionService optionService, CaseWizard_S1_CD_Service step1Service)
        {
            _context = context;
            _optionService = optionService;
            _step1Service = step1Service;
        }

        /// <summary>
        /// 開案前：選擇個案頁
        /// </summary>
        [HttpGet]
        public IActionResult SelectCase()
        {
            ViewData["Sidebar.OpenCaseRecord"] = "開案紀錄表";
            ViewData["Title"] = "選擇個案";
            ViewData["Breadcrumbs"] = new List<(string Text, string Url)>
            {
                ("個案管理", Url.Action("Query", "CaseBasic") ?? string.Empty),
                ("選擇個案", string.Empty)
            };
            
            ViewBag.TargetController = "CaseOpening";
            ViewBag.TargetAction = "Create";
            ViewBag.TargetTab = "CaseOpening";
            ViewBag.TargetStep = "CaseDetail";
            ViewBag.Mode = "Create";
            ViewBag.AutoLoad = false;
            
            return View("~/Views/Shared/SelectCase.cshtml");
        }

        /// <summary>
        /// 統一的新增入口 - 使用語義化步驟名稱
        /// </summary>
        [HttpGet]
        [Route("Create/{step}")]
        public async Task<IActionResult> Create(string caseId, string step, CaseFormMode? mode = null)
        {
            // 設置 Sidebar 項目名稱
            ViewData["Sidebar.OpenCaseRecord"] = "開案紀錄表";
            
            // 若未選擇個案，導向選擇個案頁
            if (string.IsNullOrEmpty(caseId))
            {
                return RedirectToAction("SelectCase");
            }

            // 檢查個案是否存在
            var caseExists = await _context.Cases
                .AnyAsync(c => c.CaseId == caseId && c.Deleted != true);
            
            if (!caseExists)
            {
                TempData["ErrorMessage"] = "個案不存在";
                return RedirectToAction("SelectCase");
            }

            // 根據 step 參數路由到對應的步驟方法
            return step switch
            {
                "CaseDetail" => await Step1(caseId, mode),
                "SocialWorkerContent" => await Step2(caseId, mode),
                "EconomicStatus" => await Step3(caseId, mode),
                "HealthStatus" => await Step4(caseId, mode),
                "AcademicPerformance" => await Step5(caseId, mode),
                "EmotionalEvaluation" => await Step6(caseId, mode),
                "FinalAssessment" => await Step7(caseId, mode),
                _ => NotFound()
            };
        }

        // 注意：POST 方法保留使用現有的 Step1-Step7 方法
        // 因為每個步驟的 ViewModel 類型不同，統一處理會很複雜
        // 表單提交時會直接 POST 到對應的 Step 方法

        /// <summary>
        /// 步驟1: 個案詳細資料 (CaseDetail 表格)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Step1(string caseId, CaseFormMode? mode = null)
        {
            // 設置 Sidebar 項目名稱
            ViewData["Sidebar.OpenCaseRecord"] = "開案紀錄表";
            
            // 若未選擇個案，導向選擇個案頁
            if (string.IsNullOrEmpty(caseId))
            {
                return RedirectToAction("SelectCase");
            }

            // 檢查個案是否存在
            var caseExists = await _context.Cases
                .AnyAsync(c => c.CaseId == caseId && c.Deleted != true);
            
            if (!caseExists)
            {
                TempData["ErrorMessage"] = "個案不存在";
                return View("~/Views/Case/Opening/Step1.cshtml", new CaseWizard_S1_CD_ViewModel { CaseId = string.Empty });
            }

            var viewModel = await _step1Service.GetStep1DataAsync(caseId);
            viewModel.Mode = mode ?? CanLove_Backend.Domain.Case.ViewModels.Basic.CaseFormMode.Create;
            viewModel.CurrentStep = 1;
            return View("~/Views/Case/Opening/Step1.cshtml", viewModel);
        }

        /// <summary>
        /// 步驟1: 個案詳細資料 (CaseDetail 表格)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Step1(CaseWizard_S1_CD_ViewModel model)
        {
            if (ModelState.IsValid)
            {
                var (success, message) = await _step1Service.SaveStep1DataAsync(model);
                
                if (success)
                {
                    TempData["SuccessMessage"] = message;
                    return RedirectToAction("Create", new { caseId = model.CaseId, step = "SocialWorkerContent" });
                }
                else
                {
                    ModelState.AddModelError("", message);
                }
            }

            return View("~/Views/Case/Opening/Step1.cshtml", model);
        }

        /// <summary>
        /// 步驟2: 社會工作服務內容 (CaseSocialWorkerContent 表格)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Step2(string caseId, CaseFormMode? mode = null)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                return NotFound();
            }

            var socialWorkerContent = await _context.CaseSocialWorkerContents
                .FirstOrDefaultAsync(cswc => cswc.CaseId == caseId);

            var viewModel = new CaseWizard_S2_CSWC_ViewModel
            {
                CaseId = caseId,
                FamilyTreeImg = socialWorkerContent?.FamilyTreeImg,
                ResidenceTypeValueId = socialWorkerContent?.ResidenceTypeValueId,
                HouseCleanlinessRating = socialWorkerContent?.HouseCleanlinessRating,
                HouseCleanlinessNote = socialWorkerContent?.HouseCleanlinessNote,
                HouseSafetyRating = socialWorkerContent?.HouseSafetyRating,
                HouseSafetyNote = socialWorkerContent?.HouseSafetyNote,
                CaregiverChildInteractionRating = socialWorkerContent?.CaregiverChildInteractionRating,
                CaregiverChildInteractionNote = socialWorkerContent?.CaregiverChildInteractionNote,
                CaregiverFamilyInteractionRating = socialWorkerContent?.CaregiverFamilyInteractionRating,
                CaregiverFamilyInteractionNote = socialWorkerContent?.CaregiverFamilyInteractionNote,
                FamilyResourceAbilityRating = socialWorkerContent?.FamilyResourceAbilityRating,
                FamilyResourceAbilityNote = socialWorkerContent?.FamilyResourceAbilityNote,
                FamilySocialSupportRating = socialWorkerContent?.FamilySocialSupportRating,
                FamilySocialSupportNote = socialWorkerContent?.FamilySocialSupportNote,
                SpecialCircumstancesDescription = socialWorkerContent?.SpecialCircumstancesDescription,
                // 載入選項資料
                ResidenceTypeOptions = await _optionService.GetResidenceTypeOptionsAsync(),
                Mode = mode ?? CanLove_Backend.Domain.Case.ViewModels.Basic.CaseFormMode.Create,
                CurrentStep = 2
            };

            return View("~/Views/Case/Opening/Step2.cshtml", viewModel);
        }

        /// <summary>
        /// 步驟2: 社會工作服務內容 (CaseSocialWorkerContent 表格)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Step2(CaseWizard_S2_CSWC_ViewModel model)
        {
            if (ModelState.IsValid)
            {
                var socialWorkerContent = await _context.CaseSocialWorkerContents
                    .FirstOrDefaultAsync(cswc => cswc.CaseId == model.CaseId);

                if (socialWorkerContent == null)
                {
                    socialWorkerContent = new CaseSocialWorkerContent
                    {
                        CaseId = model.CaseId,
                        CreatedAt = DateTimeExtensions.TaiwanTime
                    };
                    _context.CaseSocialWorkerContents.Add(socialWorkerContent);
                }

                socialWorkerContent.FamilyTreeImg = model.FamilyTreeImg;
                socialWorkerContent.ResidenceTypeValueId = model.ResidenceTypeValueId;
                socialWorkerContent.HouseCleanlinessRating = model.HouseCleanlinessRating;
                socialWorkerContent.HouseCleanlinessNote = model.HouseCleanlinessNote;
                socialWorkerContent.HouseSafetyRating = model.HouseSafetyRating;
                socialWorkerContent.HouseSafetyNote = model.HouseSafetyNote;
                socialWorkerContent.CaregiverChildInteractionRating = model.CaregiverChildInteractionRating;
                socialWorkerContent.CaregiverChildInteractionNote = model.CaregiverChildInteractionNote;
                socialWorkerContent.CaregiverFamilyInteractionRating = model.CaregiverFamilyInteractionRating;
                socialWorkerContent.CaregiverFamilyInteractionNote = model.CaregiverFamilyInteractionNote;
                socialWorkerContent.FamilyResourceAbilityRating = model.FamilyResourceAbilityRating;
                socialWorkerContent.FamilyResourceAbilityNote = model.FamilyResourceAbilityNote;
                socialWorkerContent.FamilySocialSupportRating = model.FamilySocialSupportRating;
                socialWorkerContent.FamilySocialSupportNote = model.FamilySocialSupportNote;
                socialWorkerContent.SpecialCircumstancesDescription = model.SpecialCircumstancesDescription;
                socialWorkerContent.UpdatedAt = DateTimeExtensions.TaiwanTime;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "步驟2完成，請繼續下一步";
                return RedirectToAction("Create", new { caseId = model.CaseId, step = "EconomicStatus" });
            }

            return View("~/Views/Case/Opening/Step2.cshtml", model);
        }

        /// <summary>
        /// 步驟3: 經濟狀況評估 (CaseFQeconomicStatus 表格)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Step3(string caseId, CaseFormMode? mode = null)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                return NotFound();
            }

            var economicStatus = await _context.CaseFqeconomicStatuses
                .FirstOrDefaultAsync(cfs => cfs.CaseId == caseId);

            var viewModel = new CaseWizard_S3_CFQES_ViewModel
            {
                CaseId = caseId,
                EconomicOverview = economicStatus?.EconomicOverview,
                WorkSituation = economicStatus?.WorkSituation,
                CivilWelfareResources = economicStatus?.CivilWelfareResources,
                MonthlyIncome = economicStatus?.MonthlyIncome,
                MonthlyExpense = economicStatus?.MonthlyExpense,
                MonthlyExpenseNote = economicStatus?.MonthlyExpenseNote,
                Description = economicStatus?.Description,
                Mode = mode ?? CanLove_Backend.Domain.Case.ViewModels.Basic.CaseFormMode.Create,
                CurrentStep = 3
            };

            return View("~/Views/Case/Opening/Step3.cshtml", viewModel);
        }

        /// <summary>
        /// 步驟3: 經濟狀況評估 (CaseFQeconomicStatus 表格)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Step3(CaseWizard_S3_CFQES_ViewModel model)
        {
            if (ModelState.IsValid)
            {
                var economicStatus = await _context.CaseFqeconomicStatuses
                    .FirstOrDefaultAsync(cfs => cfs.CaseId == model.CaseId);

                if (economicStatus == null)
                {
                    economicStatus = new CaseFqeconomicStatus
                    {
                        CaseId = model.CaseId,
                        CreatedAt = DateTimeExtensions.TaiwanTime
                    };
                    _context.CaseFqeconomicStatuses.Add(economicStatus);
                }

                economicStatus.EconomicOverview = model.EconomicOverview;
                economicStatus.WorkSituation = model.WorkSituation;
                economicStatus.CivilWelfareResources = model.CivilWelfareResources;
                economicStatus.MonthlyIncome = model.MonthlyIncome;
                economicStatus.MonthlyExpense = model.MonthlyExpense;
                economicStatus.MonthlyExpenseNote = model.MonthlyExpenseNote;
                economicStatus.Description = model.Description;
                economicStatus.UpdatedAt = DateTimeExtensions.TaiwanTime;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "步驟3完成，請繼續下一步";
                return RedirectToAction("Create", new { caseId = model.CaseId, step = "HealthStatus" });
            }

            return View("~/Views/Case/Opening/Step3.cshtml", model);
        }

        /// <summary>
        /// 步驟4: 健康狀況評估 (CaseHQhealthStatus 表格)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Step4(string caseId, CaseFormMode? mode = null)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                return NotFound();
            }

            var healthStatus = await _context.CaseHqhealthStatuses
                .FirstOrDefaultAsync(chs => chs.CaseId == caseId);

            var viewModel = new CaseWizard_S4_CHQHS_ViewModel
            {
                CaseId = caseId,
                CaregiverId = healthStatus?.CaregiverId ?? 0,
                CaregiverRoleValueId = healthStatus?.CaregiverRoleValueId ?? 0,
                CaregiverName = healthStatus?.CaregiverName,
                IsPrimary = healthStatus?.IsPrimary,
                EmotionalExpressionRating = healthStatus?.EmotionalExpressionRating,
                EmotionalExpressionNote = healthStatus?.EmotionalExpressionNote,
                HealthStatusRating = healthStatus?.HealthStatusRating,
                HealthStatusNote = healthStatus?.HealthStatusNote,
                ChildHealthStatusRating = healthStatus?.ChildHealthStatusRating,
                ChildHealthStatusNote = healthStatus?.ChildHealthStatusNote,
                ChildCareStatusRating = healthStatus?.ChildCareStatusRating,
                ChildCareStatusNote = healthStatus?.ChildCareStatusNote,
                // 載入選項資料
                CaregiverRoleOptions = await _optionService.GetCaregiverRoleOptionsAsync(),
                Mode = mode ?? CanLove_Backend.Domain.Case.ViewModels.Basic.CaseFormMode.Create,
                CurrentStep = 4
            };

            return View("~/Views/Case/Opening/Step4.cshtml", viewModel);
        }

        /// <summary>
        /// 步驟4: 健康狀況評估 (CaseHQhealthStatus 表格)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Step4(CaseWizard_S4_CHQHS_ViewModel model)
        {
            if (ModelState.IsValid)
            {
                var healthStatus = await _context.CaseHqhealthStatuses
                    .FirstOrDefaultAsync(chs => chs.CaseId == model.CaseId);

                if (healthStatus == null)
                {
                    healthStatus = new CaseHqhealthStatus
                    {
                        CaseId = model.CaseId,
                        CaregiverId = model.CaregiverId,
                        CaregiverRoleValueId = model.CaregiverRoleValueId,
                        CreatedAt = DateTimeExtensions.TaiwanTime
                    };
                    _context.CaseHqhealthStatuses.Add(healthStatus);
                }

                healthStatus.CaregiverId = model.CaregiverId;
                healthStatus.CaregiverRoleValueId = model.CaregiverRoleValueId;
                healthStatus.CaregiverName = model.CaregiverName;
                healthStatus.IsPrimary = model.IsPrimary;
                healthStatus.EmotionalExpressionRating = model.EmotionalExpressionRating;
                healthStatus.EmotionalExpressionNote = model.EmotionalExpressionNote;
                healthStatus.HealthStatusRating = model.HealthStatusRating;
                healthStatus.HealthStatusNote = model.HealthStatusNote;
                healthStatus.ChildHealthStatusRating = model.ChildHealthStatusRating;
                healthStatus.ChildHealthStatusNote = model.ChildHealthStatusNote;
                healthStatus.ChildCareStatusRating = model.ChildCareStatusRating;
                healthStatus.ChildCareStatusNote = model.ChildCareStatusNote;
                healthStatus.UpdatedAt = DateTimeExtensions.TaiwanTime;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "步驟4完成，請繼續下一步";
                return RedirectToAction("Create", new { caseId = model.CaseId, step = "AcademicPerformance" });
            }

            return View("~/Views/Case/Opening/Step4.cshtml", model);
        }

        /// <summary>
        /// 步驟5: 學業表現評估 (CaseIQacademicPerformance 表格)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Step5(string caseId, CaseFormMode? mode = null)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                return NotFound();
            }

            var academicPerformance = await _context.CaseIqacademicPerformances
                .FirstOrDefaultAsync(cap => cap.CaseId == caseId);

            var viewModel = new CaseWizard_S5_CIQAP_ViewModel
            {
                CaseId = caseId,
                AcademicPerformanceSummary = academicPerformance?.AcademicPerformanceSummary,
                Mode = mode ?? CanLove_Backend.Domain.Case.ViewModels.Basic.CaseFormMode.Create,
                CurrentStep = 5
            };

            return View("~/Views/Case/Opening/Step5.cshtml", viewModel);
        }

        /// <summary>
        /// 步驟5: 學業表現評估 (CaseIQacademicPerformance 表格)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Step5(CaseWizard_S5_CIQAP_ViewModel model)
        {
            if (ModelState.IsValid)
            {
                var academicPerformance = await _context.CaseIqacademicPerformances
                    .FirstOrDefaultAsync(cap => cap.CaseId == model.CaseId);

                if (academicPerformance == null)
                {
                    academicPerformance = new CaseIqacademicPerformance
                    {
                        CaseId = model.CaseId,
                        CreatedAt = DateTimeExtensions.TaiwanTime
                    };
                    _context.CaseIqacademicPerformances.Add(academicPerformance);
                }

                academicPerformance.AcademicPerformanceSummary = model.AcademicPerformanceSummary;
                academicPerformance.UpdatedAt = DateTimeExtensions.TaiwanTime;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "步驟5完成，請繼續下一步";
                return RedirectToAction("Create", new { caseId = model.CaseId, step = "EmotionalEvaluation" });
            }

            return View("~/Views/Case/Opening/Step5.cshtml", model);
        }

        /// <summary>
        /// 步驟6: 情緒評估 (CaseEQemotionalEvaluation 表格)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Step6(string caseId, CaseFormMode? mode = null)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                return NotFound();
            }

            var emotionalEvaluation = await _context.CaseEqemotionalEvaluations
                .FirstOrDefaultAsync(cee => cee.CaseId == caseId);

            var viewModel = new CaseWizard_S6_CEEE_ViewModel
            {
                CaseId = caseId,
                EqQ1 = emotionalEvaluation?.EqQ1,
                EqQ2 = emotionalEvaluation?.EqQ2,
                EqQ3 = emotionalEvaluation?.EqQ3,
                EqQ4 = emotionalEvaluation?.EqQ4,
                EqQ5 = emotionalEvaluation?.EqQ5,
                EqQ6 = emotionalEvaluation?.EqQ6,
                EqQ7 = emotionalEvaluation?.EqQ7,
                Mode = mode ?? CanLove_Backend.Domain.Case.ViewModels.Basic.CaseFormMode.Create,
                CurrentStep = 6
            };

            return View("~/Views/Case/Opening/Step6.cshtml", viewModel);
        }

        /// <summary>
        /// 步驟6: 情緒評估 (CaseEQemotionalEvaluation 表格)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Step6(CaseWizard_S6_CEEE_ViewModel model)
        {
            if (ModelState.IsValid)
            {
                var emotionalEvaluation = await _context.CaseEqemotionalEvaluations
                    .FirstOrDefaultAsync(cee => cee.CaseId == model.CaseId);

                if (emotionalEvaluation == null)
                {
                    emotionalEvaluation = new CaseEqemotionalEvaluation
                    {
                        CaseId = model.CaseId,
                        CreatedAt = DateTimeExtensions.TaiwanTime
                    };
                    _context.CaseEqemotionalEvaluations.Add(emotionalEvaluation);
                }

                emotionalEvaluation.EqQ1 = model.EqQ1;
                emotionalEvaluation.EqQ2 = model.EqQ2;
                emotionalEvaluation.EqQ3 = model.EqQ3;
                emotionalEvaluation.EqQ4 = model.EqQ4;
                emotionalEvaluation.EqQ5 = model.EqQ5;
                emotionalEvaluation.EqQ6 = model.EqQ6;
                emotionalEvaluation.EqQ7 = model.EqQ7;
                emotionalEvaluation.UpdatedAt = DateTimeExtensions.TaiwanTime;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "步驟6完成，請繼續下一步";
                return RedirectToAction("Create", new { caseId = model.CaseId, step = "FinalAssessment" });
            }

            return View("~/Views/Case/Opening/Step6.cshtml", model);
        }

        /// <summary>
        /// 步驟7: 最後評估表 (FinalAssessmentSummary 表格)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Step7(string caseId, CaseFormMode? mode = null)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                return NotFound();
            }

            var finalAssessment = await _context.FinalAssessmentSummaries
                .FirstOrDefaultAsync(fas => fas.CaseId == caseId);

            var viewModel = new CaseWizard_S7_FAS_ViewModel
            {
                CaseId = caseId,
                FqSummary = finalAssessment?.FqSummary,
                HqSummary = finalAssessment?.HqSummary,
                IqSummary = finalAssessment?.IqSummary,
                EqSummary = finalAssessment?.EqSummary,
                Mode = mode ?? CanLove_Backend.Domain.Case.ViewModels.Basic.CaseFormMode.Create,
                CurrentStep = 7
            };

            return View("~/Views/Case/Opening/Step7.cshtml", viewModel);
        }

        /// <summary>
        /// 步驟7: 最後評估表 (FinalAssessmentSummary 表格)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Step7(CaseWizard_S7_FAS_ViewModel model)
        {
            if (ModelState.IsValid)
            {
                var finalAssessment = await _context.FinalAssessmentSummaries
                    .FirstOrDefaultAsync(fas => fas.CaseId == model.CaseId);

                if (finalAssessment == null)
                {
                    finalAssessment = new FinalAssessmentSummary
                    {
                        CaseId = model.CaseId,
                        CreatedAt = DateTimeExtensions.TaiwanTime
                    };
                    _context.FinalAssessmentSummaries.Add(finalAssessment);
                }

                finalAssessment.FqSummary = model.FqSummary;
                finalAssessment.HqSummary = model.HqSummary;
                finalAssessment.IqSummary = model.IqSummary;
                finalAssessment.EqSummary = model.EqSummary;
                finalAssessment.UpdatedAt = DateTimeExtensions.TaiwanTime;

                await _context.SaveChangesAsync();

                // 送審（Create 模式 Step7：SubmitAction = "Submit"）
                if (string.Equals(model.SubmitAction, "Submit", StringComparison.OrdinalIgnoreCase))
                {
                    var opening = await _context.CaseOpenings.FirstOrDefaultAsync(o => o.CaseId == model.CaseId);
                    if (opening == null)
                    {
                        opening = new CaseOpening
                        {
                            CaseId = model.CaseId,
                            CreatedAt = DateTimeExtensions.TaiwanTime
                        };
                        _context.CaseOpenings.Add(opening);
                    }

                    opening.Status = "PendingReview";
                    opening.SubmittedBy = User.Identity?.Name ?? string.Empty;
                    opening.SubmittedAt = DateTimeExtensions.TaiwanTime;
                    opening.UpdatedAt = DateTimeExtensions.TaiwanTime;

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "開案資料已提交審閱";
                    return RedirectToAction("Review", "CaseOpening");
                }

                TempData["SuccessMessage"] = "個案開案流程完成！";
                return RedirectToAction("Complete", new { caseId = model.CaseId });
            }

            return View("~/Views/Case/Opening/Step7.cshtml", model);
        }

        /// <summary>
        /// 步驟8: 完成頁面
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Step8(string caseId)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                return NotFound();
            }

            var caseData = await _context.Cases
                .Include(c => c.City)
                .Include(c => c.District)
                .Include(c => c.School)
                .FirstOrDefaultAsync(c => c.CaseId == caseId);

            if (caseData == null)
            {
                return NotFound();
            }

            var viewModel = new CaseWizardCompleteViewModel
            {
                CaseId = caseData.CaseId,
                CaseName = caseData.Name,
                CompletedAt = DateTimeExtensions.TaiwanTime
            };
            return View("~/Views/Case/Opening/Complete.cshtml", viewModel);
        }

        /// <summary>
        /// 步驟8: 完成頁面
        /// </summary>
        [HttpPost]
        public IActionResult Step8(CaseWizardCompleteViewModel model)
        {
            // 完成頁面不需要 POST 處理
            return RedirectToAction("Complete", new { caseId = model.CaseId });
        }

        /// <summary>
        /// 完成頁面
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Complete(string caseId)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                return NotFound();
            }

            var caseData = await _context.Cases
                .Include(c => c.City)
                .Include(c => c.District)
                .Include(c => c.School)
                .FirstOrDefaultAsync(c => c.CaseId == caseId);

            if (caseData == null)
            {
                return NotFound();
            }

            var viewModel = new CaseWizardCompleteViewModel
            {
                CaseId = caseData.CaseId,
                CaseName = caseData.Name,
                CompletedAt = DateTimeExtensions.TaiwanTime
            };


            return View("~/Views/Case/Opening/Complete.cshtml", viewModel);
        }

        /// <summary>
        /// 上一步
        /// </summary>
        [HttpPost]
        public IActionResult PreviousStep(int currentStep, string caseId)
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
            
            return currentStep switch
            {
                2 => RedirectToAction("Create", new { caseId, step = stepNames[1] }),
                3 => RedirectToAction("Create", new { caseId, step = stepNames[2] }),
                4 => RedirectToAction("Create", new { caseId, step = stepNames[3] }),
                5 => RedirectToAction("Create", new { caseId, step = stepNames[4] }),
                6 => RedirectToAction("Create", new { caseId, step = stepNames[5] }),
                7 => RedirectToAction("Create", new { caseId, step = stepNames[6] }),
                8 => RedirectToAction("Create", new { caseId, step = stepNames[7] }),
                9 => RedirectToAction("Step8", new { caseId }),
                _ => RedirectToAction("Create", new { caseId, step = stepNames[1] })
            };
        }

        /// <summary>
        /// 查詢開案記錄
        /// </summary>
        [HttpGet]
        public IActionResult Query(string? caseId = null)
        {
            ViewData["Title"] = "查詢個案 - 開案紀錄";
            ViewBag.CurrentPage = "Search";
            ViewBag.CurrentTab = "CaseOpening";
            
            // 如果有 caseId，載入開案資料
            if (!string.IsNullOrWhiteSpace(caseId))
            {
                ViewBag.CaseId = caseId;
                ViewBag.AutoLoad = true;
            }
            else
            {
                ViewBag.AutoLoad = false;
            }
            
            ViewBag.Mode = "ReadOnly";
            ViewBag.TargetAction = "Query";
            ViewBag.TargetController = "CaseOpening";
            
            return View("~/Views/Case/Opening/SearchOpening.cshtml");
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

            // 如果指定了 caseId，載入該開案記錄的詳細資料
            if (!string.IsNullOrWhiteSpace(caseId))
            {
                var opening = await _context.CaseOpenings
                    .Include(o => o.Case)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.CaseId == caseId);

                if (opening != null && opening.Status == "PendingReview")
                {
                    ViewBag.SelectedOpening = opening;
                    ViewBag.ShowOpeningDetails = true;
                }
            }

            return View("~/Views/Case/Opening/Review.cshtml", openings);
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
            
            // 直接更新 CaseOpening 表狀態
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

            // 同時更新 Case 表的審核資訊
            var caseEntity = await _context.Cases.FirstOrDefaultAsync(c => c.CaseId == caseId);
            if (caseEntity != null)
            {
                caseEntity.ReviewedBy = reviewer;
                caseEntity.ReviewedAt = DateTimeExtensions.TaiwanTime;
            }

            _context.Update(opening);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = approved ? "審核通過" : "已退回";
            return RedirectToAction(nameof(Review));
        }
}