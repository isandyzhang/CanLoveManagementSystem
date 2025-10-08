using Microsoft.AspNetCore.Mvc;
using CanLove_Backend.Data.Contexts;
using CanLove_Backend.Data.Models.Core;
using CanLove_Backend.Data.Models.Options;
using CanLove_Backend.Models.Mvc.ViewModels;
using CanLove_Backend.Services.Case;
using CanLove_Backend.Services.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace CanLove_Backend.Controllers.Mvc;

/// <summary>
/// 個案管理控制器
/// </summary>
public class CaseController : Controller
{
    private readonly CanLoveDbContext _context;
    private readonly SchoolService _schoolService;
    private readonly OptionService _optionService;
    private readonly CaseService _caseService;

    public CaseController(CanLoveDbContext context, SchoolService schoolService, OptionService optionService, CaseService caseService)
    {
        _context = context;
        _schoolService = schoolService;
        _optionService = optionService;
        _caseService = caseService;
    }

    /// <summary>
    /// 取得個案詳細資料 (AJAX API)
    /// </summary>
    [Authorize(Policy = "RequireViewer")]
    [HttpGet]
    public async Task<IActionResult> ViewDetails(string caseId)
    {
        if (string.IsNullOrEmpty(caseId))
        {
            return Json(new { success = false, message = "個案編號不能為空" });
        }

        try
        {
            var caseData = await _context.Cases
                .Include(c => c.City)
                .Include(c => c.District)
                .Include(c => c.School)
                .Include(c => c.CaseDetail)
                    .ThenInclude(cd => cd!.ContactRelationValue)
                .Include(c => c.CaseDetail)
                    .ThenInclude(cd => cd!.MainCaregiverRelationValue)
                .Include(c => c.CaseDetail)
                    .ThenInclude(cd => cd!.FamilyStructureType)
                .Include(c => c.CaseDetail)
                    .ThenInclude(cd => cd!.MainCaregiverEduValue)
                .Include(c => c.CaseDetail)
                    .ThenInclude(cd => cd!.MainCaregiverMarryStatusValue)
                .Include(c => c.CaseFqeconomicStatus)
                .Include(c => c.CaseHqhealthStatuses)
                    .ThenInclude(hq => hq.CaregiverRoleValue)
                .Include(c => c.CaseIqacademicPerformance)
                .Include(c => c.CaseEqemotionalEvaluation)
                .Include(c => c.FinalAssessmentSummary)
                .FirstOrDefaultAsync(c => c.CaseId == caseId && c.Deleted != true);

            if (caseData == null)
            {
                return Json(new { success = false, message = "找不到指定的個案" });
            }

            // 計算 EQ 總分
            var eqTotal = 0;
            var eqCount = 0;
            if (caseData.CaseEqemotionalEvaluation != null)
            {
                var eq = caseData.CaseEqemotionalEvaluation;
                var eqScores = new[] { eq.EqQ1, eq.EqQ2, eq.EqQ3, eq.EqQ4, eq.EqQ5, eq.EqQ6, eq.EqQ7 };
                foreach (var score in eqScores)
                {
                    if (score.HasValue)
                    {
                        eqTotal += score.Value;
                        eqCount++;
                    }
                }
            }

            // 計算 HQ 平均分數
            var hqTotal = 0;
            var hqCount = 0;
            if (caseData.CaseHqhealthStatuses.Any())
            {
                foreach (var hq in caseData.CaseHqhealthStatuses)
                {
                    if (hq.EmotionalExpressionRating.HasValue)
                    {
                        hqTotal += hq.EmotionalExpressionRating.Value;
                        hqCount++;
                    }
                    if (hq.HealthStatusRating.HasValue)
                    {
                        hqTotal += hq.HealthStatusRating.Value;
                        hqCount++;
                    }
                    if (hq.ChildHealthStatusRating.HasValue)
                    {
                        hqTotal += hq.ChildHealthStatusRating.Value;
                        hqCount++;
                    }
                    if (hq.ChildCareStatusRating.HasValue)
                    {
                        hqTotal += hq.ChildCareStatusRating.Value;
                        hqCount++;
                    }
                }
            }

            var result = new
            {
                success = true,
                data = new
                {
                    // 基本資料
                    caseId = caseData.CaseId,
                    name = caseData.Name,
                    gender = caseData.Gender,
                    birthDate = caseData.BirthDate.ToString("yyyy-MM-dd"),
                    idNumber = caseData.IdNumber,
                    phone = caseData.Phone,
                    email = caseData.Email,
                    address = caseData.Address,
                    cityName = caseData.City?.CityName,
                    districtName = caseData.District?.DistrictName,
                    schoolName = caseData.School?.SchoolName,
                    
                    // 聯絡人資料
                    contactName = caseData.CaseDetail?.ContactName,
                    contactRelation = caseData.CaseDetail?.ContactRelationValue?.ValueName,
                    contactPhone = caseData.CaseDetail?.ContactPhone,
                    homePhone = caseData.CaseDetail?.HomePhone,
                    
                    // 扶養人資料
                    mainCaregiverName = caseData.CaseDetail?.MainCaregiverName,
                    mainCaregiverRelation = caseData.CaseDetail?.MainCaregiverRelationValue?.ValueName,
                    mainCaregiverJob = caseData.CaseDetail?.MainCaregiverJob,
                    mainCaregiverBirth = caseData.CaseDetail?.MainCaregiverBirth?.ToString("yyyy-MM-dd"),
                    mainCaregiverEdu = caseData.CaseDetail?.MainCaregiverEduValue?.ValueName,
                    mainCaregiverMarryStatus = caseData.CaseDetail?.MainCaregiverMarryStatusValue?.ValueName,
                    
                    // 家庭結構
                    familyStructure = caseData.CaseDetail?.FamilyStructureType?.StructureName,
                    familyStructureOther = caseData.CaseDetail?.FamilyStructureOtherDesc,
                    
                    // 經濟狀況
                    monthlyIncome = caseData.CaseFqeconomicStatus?.MonthlyIncome,
                    monthlyExpense = caseData.CaseFqeconomicStatus?.MonthlyExpense,
                    economicOverview = caseData.CaseFqeconomicStatus?.EconomicOverview,
                    workSituation = caseData.CaseFqeconomicStatus?.WorkSituation,
                    
                    // 評估分數
                    eqTotal = eqCount > 0 ? eqTotal : (int?)null,
                    eqCount = eqCount,
                    hqAverage = hqCount > 0 ? Math.Round((double)hqTotal / hqCount, 1) : (double?)null,
                    hqCount = hqCount,
                    
                    // 學業表現
                    academicPerformance = caseData.CaseIqacademicPerformance?.AcademicPerformanceSummary,
                    
                    // 最終評估摘要
                    fqSummary = caseData.FinalAssessmentSummary?.FqSummary,
                    hqSummary = caseData.FinalAssessmentSummary?.HqSummary,
                    iqSummary = caseData.FinalAssessmentSummary?.IqSummary,
                    eqSummary = caseData.FinalAssessmentSummary?.EqSummary,
                    
                    // 審核狀態
                    draftStatus = caseData.DraftStatus,
                    submittedBy = caseData.SubmittedBy,
                    submittedAt = caseData.SubmittedAt?.ToString("yyyy-MM-dd HH:mm"),
                    reviewedBy = caseData.ReviewedBy,
                    reviewedAt = caseData.ReviewedAt?.ToString("yyyy-MM-dd HH:mm"),
                    isLocked = caseData.IsLocked,
                    lockedBy = caseData.LockedBy,
                    lockedAt = caseData.LockedAt?.ToString("yyyy-MM-dd HH:mm"),
                    
                    // 建立/更新時間
                    createdAt = caseData.CreatedAt?.ToString("yyyy-MM-dd HH:mm"),
                    updatedAt = caseData.UpdatedAt?.ToString("yyyy-MM-dd HH:mm")
                }
            };

            return Json(result);
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"取得個案資料時發生錯誤：{ex.Message}" });
        }
    }

    /// <summary>
    /// 個案列表頁面
    /// </summary>
    [Authorize(Policy = "RequireViewer")]
    public async Task<IActionResult> Index()
    {
        var currentUser = User.Identity?.Name ?? "";
        var userRoles = User.FindAll("roles").Select(c => c.Value).ToList();
        
        IQueryable<Case> query = _context.Cases
            .Include(c => c.City)
            .Include(c => c.District)
            .Include(c => c.School)
            .Where(c => c.Deleted != true);

        // 根據角色過濾個案
        if (userRoles.Contains("assistant"))
        {
            // Assistant 只能看到自己建立的個案
            query = query.Where(c => c.SubmittedBy == currentUser);
        }
        else if (userRoles.Contains("socialworker") && !userRoles.Contains("admin"))
        {
            // SocialWorker 可以看到所有個案，但優先顯示待審核的
            query = query.Where(c => c.SubmittedAt != null && c.ReviewedAt == null);
        }
        // Admin 和 Viewer 可以看到所有個案

        var cases = await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return View(cases);
    }

    /// <summary>
    /// 個案建立頁面
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "RequireAssistant")]
    public async Task<IActionResult> Create()
    {
        var viewModel = new CaseCreateViewModel
        {
            Case = new CanLove_Backend.Data.Models.Core.Case
            {
                CaseId = string.Empty, // 讓使用者自己輸入
                AssessmentDate = DateOnly.FromDateTime(DateTime.Today)
            },
            Cities = await _context.Cities.OrderBy(c => c.CityId).ToListAsync(),
            Districts = new List<District>(), // 初始為空，等選擇城市後載入
            Schools = await _schoolService.GetAllSchoolsAsync(), // 載入所有學校供獨立選擇
            GenderOptions = await _optionService.GetGenderOptionsAsync()
        };

        // 載入所有地區資料並按城市分組，供前端JavaScript使用
        var allDistricts = await _context.Districts
            .Include(d => d.City)
            .OrderBy(d => d.CityId)
            .ThenBy(d => d.DistrictName)
            .ToListAsync();

        var districtsByCity = allDistricts
            .GroupBy(d => d.CityId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(d => new { 
                    districtId = d.DistrictId, 
                    districtName = d.DistrictName 
                }).ToList()
            );

        ViewBag.DistrictsByCity = districtsByCity;

        return View(viewModel);
    }

    /// <summary>
    /// 個案建立處理
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "RequireAssistant")]
    public async Task<IActionResult> Create(CaseCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            // 設定個案為草稿狀態
            model.Case.DraftStatus = false; // 0=草稿
            model.Case.SubmittedBy = User.Identity?.Name ?? "";
            model.Case.CreatedAt = DateTime.UtcNow;
            model.Case.UpdatedAt = DateTime.UtcNow;
            
            var response = await _caseService.CreateCaseAsync(model.Case);
            
            if (response.Success)
            {
                TempData["SuccessMessage"] = "個案建立成功";
                return RedirectToAction("Step1", "CaseWizardOpenCase", new { caseId = model.Case.CaseId });
            }
            else
            {
                ModelState.AddModelError("Case.CaseId", response.Message);
            }
        }

        // 如果驗證失敗，重新載入下拉選單資料
        model.Cities = await _context.Cities.OrderBy(c => c.CityId).ToListAsync();
        model.Districts = new List<District>(); // 初始為空，等選擇城市後載入
        model.Schools = await _schoolService.GetAllSchoolsAsync(); // 載入所有學校供獨立選擇
        model.GenderOptions = await _optionService.GetGenderOptionsAsync();

        return View(model);
    }

    /// <summary>
    /// 個案編輯頁面
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "RequireSocialWorker")]
    public async Task<IActionResult> Edit(string id)
    {
        if (id == null)
        {
            return View("NotFound");
        }

        var caseItem = await _context.Cases.FindAsync(id);
        if (caseItem == null)
        {
            return View("NotFound");
        }

        // 檢查個案是否被鎖定
        if (caseItem.IsLocked == true && caseItem.LockedBy != User.Identity?.Name)
        {
            TempData["ErrorMessage"] = "此個案已被其他使用者鎖定，無法編輯";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Cities = await _context.Cities.ToListAsync();
        ViewBag.Districts = await _context.Districts.ToListAsync();
        ViewBag.Schools = await _context.Schools.ToListAsync();
        
        return View(caseItem);
    }

    /// <summary>
    /// 個案編輯處理
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "RequireSocialWorker")]
    public async Task<IActionResult> Edit(string id, CanLove_Backend.Data.Models.Core.Case caseItem)
    {
        if (id != caseItem.CaseId)
        {
            return View("NotFound");
        }

        if (ModelState.IsValid)
        {
            try
            {
                // 檢查個案是否被鎖定
                var existingCase = await _context.Cases.FindAsync(id);
                if (existingCase?.IsLocked == true && existingCase.LockedBy != User.Identity?.Name)
                {
                    TempData["ErrorMessage"] = "此個案已被其他使用者鎖定，無法編輯";
                    return RedirectToAction(nameof(Index));
                }

                caseItem.UpdatedAt = DateTime.UtcNow;
                _context.Update(caseItem);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "個案更新成功";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CaseExists(caseItem.CaseId))
                {
                    return View("NotFound");
                }
                else
                {
                    throw;
                }
            }
        }

        ViewBag.Cities = await _context.Cities.ToListAsync();
        ViewBag.Districts = await _context.Districts.ToListAsync();
        ViewBag.Schools = await _context.Schools.ToListAsync();
        
        return View(caseItem);
    }

    /// <summary>
    /// 個案刪除頁面
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> Delete(string id)
    {
        if (id == null)
        {
            return View("NotFound");
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

        return View(caseItem);
    }

    /// <summary>
    /// 個案刪除處理 (軟刪除)
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        try
        {
            var caseItem = await _context.Cases.FindAsync(id);
            if (caseItem == null)
            {
                return Json(new { success = false, message = "找不到指定的個案" });
            }

            // 檢查是否已經被刪除
            if (caseItem.Deleted == true)
            {
                return Json(new { success = false, message = "此個案已經被刪除" });
            }

            // 執行軟刪除
            caseItem.Deleted = true;
            caseItem.DeletedAt = DateTime.UtcNow;
            caseItem.DeletedBy = User.Identity?.Name ?? "System";
            
            _context.Update(caseItem);
            await _context.SaveChangesAsync();
            
            return Json(new { 
                success = true, 
                message = $"個案「{caseItem.Name}」已成功刪除",
                caseId = caseItem.CaseId,
                deletedAt = caseItem.DeletedAt?.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"刪除個案時發生錯誤：{ex.Message}" });
        }
    }

    /// <summary>
    /// 個案詳情頁面
    /// </summary>
    [Authorize(Policy = "RequireViewer")]
    public async Task<IActionResult> Details(string id)
    {
        if (id == null)
        {
            return View("NotFound");
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

        return View(caseItem);
    }

    /// <summary>
    /// 提交個案審核
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "RequireAssistant")]
    public async Task<IActionResult> SubmitForReview(string id)
    {
        var caseItem = await _context.Cases.FindAsync(id);
        if (caseItem == null)
        {
            return View("NotFound");
        }

        // 檢查是否為建立者
        if (caseItem.SubmittedBy != User.Identity?.Name)
        {
            TempData["ErrorMessage"] = "您只能提交自己建立的個案";
            return RedirectToAction(nameof(Index));
        }

        // 檢查是否已經提交過
        if (caseItem.SubmittedAt != null)
        {
            TempData["ErrorMessage"] = "此個案已經提交審核";
            return RedirectToAction(nameof(Index));
        }

        caseItem.SubmittedAt = DateTime.UtcNow;
        caseItem.DraftStatus = true; // 1=完成
        caseItem.UpdatedAt = DateTime.UtcNow;
        
        _context.Update(caseItem);
        await _context.SaveChangesAsync();
        
        TempData["SuccessMessage"] = "個案已提交審核";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// 審核個案
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "RequireSocialWorker")]
    public async Task<IActionResult> ReviewCase(string id, bool approved, string? reviewComment = null)
    {
        var caseItem = await _context.Cases.FindAsync(id);
        if (caseItem == null)
        {
            return View("NotFound");
        }

        // 檢查是否已經審核過
        if (caseItem.ReviewedAt != null)
        {
            TempData["ErrorMessage"] = "此個案已經審核過";
            return RedirectToAction(nameof(Index));
        }

        caseItem.ReviewedBy = User.Identity?.Name ?? "";
        caseItem.ReviewedAt = DateTime.UtcNow;
        caseItem.UpdatedAt = DateTime.UtcNow;
        
        if (!approved)
        {
            // 退回：重置提交狀態
            caseItem.SubmittedAt = null;
            caseItem.DraftStatus = false; // 回到草稿狀態
        }
        
        _context.Update(caseItem);
        await _context.SaveChangesAsync();
        
        var message = approved ? "個案審核通過" : "個案已退回";
        TempData["SuccessMessage"] = message;
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// 鎖定/解鎖個案
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "RequireSocialWorker")]
    public async Task<IActionResult> ToggleLock(string id)
    {
        var caseItem = await _context.Cases.FindAsync(id);
        if (caseItem == null)
        {
            return View("NotFound");
        }

        var currentUser = User.Identity?.Name ?? "";
        
        if (caseItem.IsLocked == true)
        {
            // 解鎖
            if (caseItem.LockedBy == currentUser)
            {
                caseItem.IsLocked = false;
                caseItem.LockedBy = null;
                caseItem.LockedAt = null;
                TempData["SuccessMessage"] = "個案已解鎖";
            }
            else
            {
                TempData["ErrorMessage"] = "您只能解鎖自己鎖定的個案";
                return RedirectToAction(nameof(Index));
            }
        }
        else
        {
            // 鎖定
            caseItem.IsLocked = true;
            caseItem.LockedBy = currentUser;
            caseItem.LockedAt = DateTime.UtcNow;
            TempData["SuccessMessage"] = "個案已鎖定";
        }
        
        caseItem.UpdatedAt = DateTime.UtcNow;
        _context.Update(caseItem);
        await _context.SaveChangesAsync();
        
        return RedirectToAction(nameof(Index));
    }

    private bool CaseExists(string id)
    {
        return _context.Cases.Any(e => e.CaseId == id);
    }
}
