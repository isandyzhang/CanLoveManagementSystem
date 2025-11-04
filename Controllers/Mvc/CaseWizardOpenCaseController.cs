using Microsoft.AspNetCore.Mvc;
using CanLove_Backend.Data.Contexts;
using CanLove_Backend.Data.Models.CaseDetails;
using CanLove_Backend.Models.Mvc.ViewModels.CaseWizardOpenCase;
using CanLove_Backend.Services.CaseWizardOpenCase.Steps;
using CanLove_Backend.Services.Shared;
using Microsoft.EntityFrameworkCore;

namespace CanLove_Backend.Controllers.Mvc
{
    /// <summary>
    /// 個案開案步驟表單控制器
    /// </summary>
    public class CaseWizardOpenCaseController : Controller
    {
        private readonly CanLoveDbContext _context;
        private readonly OptionService _optionService;
        private readonly CaseWizard_S1_CD_Service _step1Service;

        public CaseWizardOpenCaseController(CanLoveDbContext context, OptionService optionService, CaseWizard_S1_CD_Service step1Service)
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
            return View();
        }


        /// <summary>
        /// 步驟1: 個案詳細資料 (CaseDetail 表格)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Step1(string caseId)
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
                return View(new CaseWizard_S1_CD_ViewModel { CaseId = string.Empty });
            }

            var viewModel = await _step1Service.GetStep1DataAsync(caseId);
            return View(viewModel);
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
                    return RedirectToAction("Step2", new { caseId = model.CaseId });
                }
                else
                {
                    ModelState.AddModelError("", message);
                }
            }

            return View(model);
        }

        /// <summary>
        /// 步驟2: 社會工作服務內容 (CaseSocialWorkerContent 表格)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Step2(string caseId)
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
                ResidenceTypeOptions = await _optionService.GetResidenceTypeOptionsAsync()
            };

            return View(viewModel);
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
                        CreatedAt = DateTime.UtcNow
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
                socialWorkerContent.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "步驟2完成，請繼續下一步";
                return RedirectToAction("Step3", new { caseId = model.CaseId });
            }

            return View(model);
        }

        /// <summary>
        /// 步驟3: 經濟狀況評估 (CaseFQeconomicStatus 表格)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Step3(string caseId)
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
                Description = economicStatus?.Description
            };

            return View(viewModel);
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
                        CreatedAt = DateTime.UtcNow
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
                economicStatus.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "步驟3完成，請繼續下一步";
                return RedirectToAction("Step4", new { caseId = model.CaseId });
            }

            return View(model);
        }

        /// <summary>
        /// 步驟4: 健康狀況評估 (CaseHQhealthStatus 表格)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Step4(string caseId)
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
                CaregiverRoleOptions = await _optionService.GetCaregiverRoleOptionsAsync()
            };

            return View(viewModel);
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
                        CreatedAt = DateTime.UtcNow
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
                healthStatus.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "步驟4完成，請繼續下一步";
                return RedirectToAction("Step5", new { caseId = model.CaseId });
            }

            return View(model);
        }

        /// <summary>
        /// 步驟5: 學業表現評估 (CaseIQacademicPerformance 表格)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Step5(string caseId)
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
                AcademicPerformanceSummary = academicPerformance?.AcademicPerformanceSummary
            };

            return View(viewModel);
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
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.CaseIqacademicPerformances.Add(academicPerformance);
                }

                academicPerformance.AcademicPerformanceSummary = model.AcademicPerformanceSummary;
                academicPerformance.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "步驟5完成，請繼續下一步";
                return RedirectToAction("Step6", new { caseId = model.CaseId });
            }

            return View(model);
        }

        /// <summary>
        /// 步驟6: 情緒評估 (CaseEQemotionalEvaluation 表格)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Step6(string caseId)
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
                EqQ7 = emotionalEvaluation?.EqQ7
            };

            return View(viewModel);
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
                        CreatedAt = DateTime.UtcNow
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
                emotionalEvaluation.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "步驟6完成，請繼續下一步";
                return RedirectToAction("Step7", new { caseId = model.CaseId });
            }

            return View(model);
        }

        /// <summary>
        /// 步驟7: 最後評估表 (FinalAssessmentSummary 表格)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Step7(string caseId)
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
                EqSummary = finalAssessment?.EqSummary
            };

            return View(viewModel);
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
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.FinalAssessmentSummaries.Add(finalAssessment);
                }

                finalAssessment.FqSummary = model.FqSummary;
                finalAssessment.HqSummary = model.HqSummary;
                finalAssessment.IqSummary = model.IqSummary;
                finalAssessment.EqSummary = model.EqSummary;
                finalAssessment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "個案開案流程完成！";
                return RedirectToAction("Complete", new { caseId = model.CaseId });
            }

            return View(model);
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
                CompletedAt = DateTime.UtcNow
            };
            return View(viewModel);
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
                CompletedAt = DateTime.UtcNow
            };


            return View(viewModel);
        }

        /// <summary>
        /// 上一步
        /// </summary>
        [HttpPost]
        public IActionResult PreviousStep(int currentStep, string caseId)
        {
            return currentStep switch
            {
                2 => RedirectToAction("Step1", new { caseId }),
                3 => RedirectToAction("Step2", new { caseId }),
                4 => RedirectToAction("Step3", new { caseId }),
                5 => RedirectToAction("Step4", new { caseId }),
                6 => RedirectToAction("Step5", new { caseId }),
                7 => RedirectToAction("Step6", new { caseId }),
                8 => RedirectToAction("Step7", new { caseId }),
                9 => RedirectToAction("Step8", new { caseId }),
                _ => RedirectToAction("Step1", new { caseId })
            };
        }
    }
}