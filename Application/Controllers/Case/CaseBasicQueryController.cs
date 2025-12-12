using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CanLove_Backend.Infrastructure.Data.Contexts;
using CanLove_Backend.Domain.Case.Models.Basic;
using CanLove_Backend.Application.ViewModels.Case.Basic;
using CanLove_Backend.Domain.Case.Services.Basic;
using CanLove_Backend.Domain.Case.Shared.Services;
using CanLove_Backend.Infrastructure.Storage.Encryption;
using CaseEntity = CanLove_Backend.Domain.Case.Models.Basic.Case;
namespace CanLove_Backend.Application.Controllers.Case;

/// <summary>
/// 個案基本資料查詢控制器：專責處理查詢、搜尋和查看功能
/// </summary>
public class CaseBasicQueryController : CaseBasicBaseController
{
    private readonly CanLoveDbContext _context;
    private readonly ICaseBasicService _caseService;
    private readonly CaseBasicPhotoService _photoService;
    private readonly DataEncryptionService _encryptionService;

    public CaseBasicQueryController(
        CanLoveDbContext context,
        ICaseBasicService caseService,
        CaseBasicPhotoService photoService,
        DataEncryptionService encryptionService,
        CaseBasicValidationService validationService,
        CaseBasicOptionsService optionsService,
        CaseInfoService caseInfoService)
        : base(validationService, optionsService, caseInfoService)
    {
        _context = context;
        _caseService = caseService;
        _photoService = photoService;
        _encryptionService = encryptionService;
    }

    /// <summary>
    /// 查詢 - 基本資料
    /// </summary>
    /// <param name="caseId">個案編號（選填）</param>
    /// <param name="status">狀態篩選（選填：Draft, PendingReview, Approved, Rejected）</param>
    [HttpGet]
    public async Task<IActionResult> Query(string? caseId = null, string? status = null)
    {
        ViewData["Title"] = "查詢個案 - 基本資料";
        ViewBag.CurrentPage = "Search";
        ViewBag.CurrentTab = "CaseBasic";
        SetNavigationContext("Query");
        
        // 預設載入所有個案列表（支援狀態篩選）
        try
        {
            var queryable = _context.Cases
                .Include(c => c.City)
                .Include(c => c.District)
                .Include(c => c.School)
                .Where(c => c.Deleted != true);

            // 狀態篩選
            if (!string.IsNullOrWhiteSpace(status))
            {
                queryable = queryable.Where(c => c.Status == status);
            }

            var allCases = await queryable
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CaseEntity
                {
                    CaseId = c.CaseId,
                    Name = c.Name,
                    Gender = c.Gender,
                    Status = c.Status,
                    BirthDate = c.BirthDate,
                    City = c.City,
                    District = c.District,
                    School = c.School,
                    SubmittedBy = c.SubmittedBy,
                    SubmittedAt = c.SubmittedAt,
                    ReviewedBy = c.ReviewedBy,
                    ReviewedAt = c.ReviewedAt,
                    IsLocked = c.IsLocked,
                    LockedBy = c.LockedBy,
                    LockedAt = c.LockedAt,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .AsNoTracking()
                .ToListAsync();
            
            ViewBag.AllCases = allCases;
            ViewBag.ShowAllCases = true;
            ViewBag.CurrentStatus = status;
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = $"查詢資料時發生錯誤：{ex.Message}";
            ViewBag.AllCases = new List<CaseEntity>();
            ViewBag.ShowAllCases = true;
        }
        
        return View("~/Views/Case/Basic/Search/Index.cshtml");
    }

    /// <summary>
    /// 搜尋個案 API（AJAX）
    /// </summary>
    /// <param name="query">搜尋關鍵字</param>
    /// <param name="status">狀態篩選（選填：Draft, PendingReview, Approved, Rejected）</param>
    /// <param name="excludeWithOpeningRecord">是否排除已有開案紀錄的個案（預設為 false）</param>
    [HttpGet]
    public async Task<IActionResult> SearchCases(string? query = null, string? status = null, bool excludeWithOpeningRecord = false)
    {
        try
        {
            var queryable = _context.Cases
                .Include(c => c.City)
                .Include(c => c.District)
                .Include(c => c.School)
                .Where(c => c.Deleted != true);

            // 狀態篩選
            if (!string.IsNullOrWhiteSpace(status))
            {
                queryable = queryable.Where(c => c.Status == status);
            }

            // 如果需要排除已有開案紀錄的個案
            if (excludeWithOpeningRecord)
            {
                queryable = queryable.Where(c => !_context.CaseOpenings.Any(o => o.CaseId == c.CaseId));
            }

            // 如果有查詢條件，加入搜尋過濾
            if (!string.IsNullOrWhiteSpace(query))
            {
                var searchTerm = query.Trim();
                queryable = queryable.Where(c =>
                    c.CaseId.Contains(searchTerm) ||
                    (c.Name != null && c.Name.Contains(searchTerm)) ||
                    (c.Phone != null && c.Phone.Contains(searchTerm))
                );
            }

            var cases = await queryable
                .OrderByDescending(c => c.CreatedAt)
                .Take(200)
                .Select(c => new
                {
                    caseId = c.CaseId,
                    name = c.Name,
                    gender = c.Gender ?? "",
                    status = c.Status ?? "",
                    birthDate = c.BirthDate,
                    city = c.City != null ? new { CityId = c.City.CityId, CityName = c.City.CityName } : null,
                    district = c.District != null ? new { DistrictId = c.District.DistrictId, DistrictName = c.District.DistrictName } : null,
                    school = c.School != null ? new { SchoolId = c.School.SchoolId, SchoolName = c.School.SchoolName } : null,
                    submittedBy = c.SubmittedBy ?? "",
                    createdAt = c.CreatedAt
                })
                .ToListAsync();

            return Json(new { success = true, cases = cases });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"搜尋失敗：{ex.Message}" });
        }
    }

    /// <summary>
    /// 取得個案詳細資料 (AJAX API)
    /// </summary>
    [Authorize] // TODO: 之後可根據需求改為 [Authorize(Policy = "RequireViewer")]
    [HttpGet]
    public async Task<IActionResult> ViewDetails(string caseId)
    {
        if (string.IsNullOrEmpty(caseId))
        {
            return Json(new { success = false, message = "個案編號不能為空" });
        }

        try
        {
            // 使用 AsNoTracking 和投影優化查詢
            var caseData = await _context.Cases
                .Where(c => c.CaseId == caseId && c.Deleted != true)
                .Select(c => new
                {
                    // 基本資料
                    CaseId = c.CaseId,
                    Name = c.Name,
                    Gender = c.Gender,
                    BirthDate = c.BirthDate,
                    IdNumber = c.IdNumber,
                    Phone = c.Phone,
                    Email = c.Email,
                    Address = c.Address,
                    CityName = c.City != null ? c.City.CityName : null,
                    DistrictName = c.District != null ? c.District.DistrictName : null,
                    SchoolName = c.School != null ? c.School.SchoolName : null,
                    
                    // 聯絡人資料
                    ContactName = c.CaseDetail != null ? c.CaseDetail.ContactName : null,
                    ContactRelation = c.CaseDetail != null && c.CaseDetail.ContactRelationValue != null ? c.CaseDetail.ContactRelationValue.ValueName : null,
                    ContactPhone = c.CaseDetail != null ? c.CaseDetail.ContactPhone : null,
                    HomePhone = c.CaseDetail != null ? c.CaseDetail.HomePhone : null,
                    
                    // 扶養人資料
                    MainCaregiverName = c.CaseDetail != null ? c.CaseDetail.MainCaregiverName : null,
                    MainCaregiverRelation = c.CaseDetail != null && c.CaseDetail.MainCaregiverRelationValue != null ? c.CaseDetail.MainCaregiverRelationValue.ValueName : null,
                    MainCaregiverJob = c.CaseDetail != null ? c.CaseDetail.MainCaregiverJob : null,
                    MainCaregiverBirth = c.CaseDetail != null ? c.CaseDetail.MainCaregiverBirth : (DateOnly?)null,
                    MainCaregiverEdu = c.CaseDetail != null && c.CaseDetail.MainCaregiverEduValue != null ? c.CaseDetail.MainCaregiverEduValue.ValueName : null,
                    MainCaregiverMarryStatus = c.CaseDetail != null && c.CaseDetail.MainCaregiverMarryStatusValue != null ? c.CaseDetail.MainCaregiverMarryStatusValue.ValueName : null,
                    
                    // 家庭結構
                    FamilyStructure = c.CaseDetail != null && c.CaseDetail.FamilyStructureType != null ? c.CaseDetail.FamilyStructureType.StructureName : null,
                    FamilyStructureOther = c.CaseDetail != null ? c.CaseDetail.FamilyStructureOtherDesc : null,
                    
                    // 經濟狀況
                    MonthlyIncome = c.CaseFqeconomicStatus != null ? c.CaseFqeconomicStatus.MonthlyIncome : null,
                    MonthlyExpense = c.CaseFqeconomicStatus != null ? c.CaseFqeconomicStatus.MonthlyExpense : null,
                    EconomicOverview = c.CaseFqeconomicStatus != null ? c.CaseFqeconomicStatus.EconomicOverview : null,
                    WorkSituation = c.CaseFqeconomicStatus != null ? c.CaseFqeconomicStatus.WorkSituation : null,
                    
                    // 情緒評估
                    EqQ1 = c.CaseEqemotionalEvaluation != null ? c.CaseEqemotionalEvaluation.EqQ1 : null,
                    EqQ2 = c.CaseEqemotionalEvaluation != null ? c.CaseEqemotionalEvaluation.EqQ2 : null,
                    EqQ3 = c.CaseEqemotionalEvaluation != null ? c.CaseEqemotionalEvaluation.EqQ3 : null,
                    EqQ4 = c.CaseEqemotionalEvaluation != null ? c.CaseEqemotionalEvaluation.EqQ4 : null,
                    EqQ5 = c.CaseEqemotionalEvaluation != null ? c.CaseEqemotionalEvaluation.EqQ5 : null,
                    EqQ6 = c.CaseEqemotionalEvaluation != null ? c.CaseEqemotionalEvaluation.EqQ6 : null,
                    EqQ7 = c.CaseEqemotionalEvaluation != null ? c.CaseEqemotionalEvaluation.EqQ7 : null,
                    
                    // 健康狀況
                    HealthStatuses = c.CaseHqhealthStatuses.Select(hq => new
                    {
                        EmotionalExpressionRating = hq.EmotionalExpressionRating,
                        HealthStatusRating = hq.HealthStatusRating,
                        ChildHealthStatusRating = hq.ChildHealthStatusRating,
                        ChildCareStatusRating = hq.ChildCareStatusRating
                    }).ToList(),
                    
                    // 學業表現
                    AcademicPerformance = c.CaseIqacademicPerformance != null ? c.CaseIqacademicPerformance.AcademicPerformanceSummary : null,
                    
                    // 最終評估摘要
                    FqSummary = c.FinalAssessmentSummary != null ? c.FinalAssessmentSummary.FqSummary : null,
                    HqSummary = c.FinalAssessmentSummary != null ? c.FinalAssessmentSummary.HqSummary : null,
                    IqSummary = c.FinalAssessmentSummary != null ? c.FinalAssessmentSummary.IqSummary : null,
                    EqSummary = c.FinalAssessmentSummary != null ? c.FinalAssessmentSummary.EqSummary : null,
                    
                    // 審核狀態
                    Status = c.Status,
                    SubmittedBy = c.SubmittedBy,
                    SubmittedAt = c.SubmittedAt,
                    ReviewedBy = c.ReviewedBy,
                    ReviewedAt = c.ReviewedAt,
                    IsLocked = c.IsLocked,
                    LockedBy = c.LockedBy,
                    LockedAt = c.LockedAt,
                    
                    // 建立/更新時間
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (caseData == null)
            {
                return Json(new { success = false, message = "找不到指定的個案" });
            }

            // 計算 EQ 總分
            var eqTotal = 0;
            var eqCount = 0;
            var eqScores = new[] { caseData.EqQ1, caseData.EqQ2, caseData.EqQ3, caseData.EqQ4, caseData.EqQ5, caseData.EqQ6, caseData.EqQ7 };
            foreach (var score in eqScores)
            {
                if (score.HasValue)
                {
                    eqTotal += score.Value;
                    eqCount++;
                }
            }

            // 計算 HQ 平均分數
            var hqTotal = 0;
            var hqCount = 0;
            if (caseData.HealthStatuses != null && caseData.HealthStatuses.Any())
            {
                foreach (var hq in caseData.HealthStatuses)
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
                    // 解密身分證字號
                    idNumber = _encryptionService.DecryptSafely(caseData.IdNumber),
                    phone = caseData.Phone,
                    email = caseData.Email,
                    address = caseData.Address,
                    cityName = caseData.CityName,
                    districtName = caseData.DistrictName,
                    schoolName = caseData.SchoolName,
                    
                    // 聯絡人資料
                    contactName = caseData.ContactName,
                    contactRelation = caseData.ContactRelation,
                    contactPhone = caseData.ContactPhone,
                    homePhone = caseData.HomePhone,
                    
                    // 扶養人資料
                    mainCaregiverName = caseData.MainCaregiverName,
                    mainCaregiverRelation = caseData.MainCaregiverRelation,
                    mainCaregiverJob = caseData.MainCaregiverJob,
                    mainCaregiverBirth = caseData.MainCaregiverBirth?.ToString("yyyy-MM-dd"),
                    mainCaregiverEdu = caseData.MainCaregiverEdu,
                    mainCaregiverMarryStatus = caseData.MainCaregiverMarryStatus,
                    
                    // 家庭結構
                    familyStructure = caseData.FamilyStructure,
                    familyStructureOther = caseData.FamilyStructureOther,
                    
                    // 經濟狀況
                    monthlyIncome = caseData.MonthlyIncome,
                    monthlyExpense = caseData.MonthlyExpense,
                    economicOverview = caseData.EconomicOverview,
                    workSituation = caseData.WorkSituation,
                    
                    // 評估分數
                    eqTotal = eqCount > 0 ? eqTotal : (int?)null,
                    eqCount = eqCount,
                    hqAverage = hqCount > 0 ? Math.Round((double)hqTotal / hqCount, 1) : (double?)null,
                    hqCount = hqCount,
                    
                    // 學業表現
                    academicPerformance = caseData.AcademicPerformance,
                    
                    // 最終評估摘要
                    fqSummary = caseData.FqSummary,
                    hqSummary = caseData.HqSummary,
                    iqSummary = caseData.IqSummary,
                    eqSummary = caseData.EqSummary,
                    
                    // 審核狀態
                    status = caseData.Status,
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
    /// 查看個案詳細資料（只讀模式）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ViewItem(string id)
    {
        var validationResult = ValidateCaseId(id);
        if (validationResult != null)
        {
            return validationResult;
        }

        try
        {
            var caseItem = await _caseService.GetCaseForViewAsync(id);
            if (caseItem == null)
            {
                return View("NotFound");
            }
            
            // 解密身分證字號
            if (!string.IsNullOrWhiteSpace(caseItem.IdNumber))
            {
                caseItem.IdNumber = _encryptionService.DecryptSafely(caseItem.IdNumber);
            }

            // 載入選項資料
            var optionsData = await LoadOptionsDataAsync();
            
            // 取得照片 URL（如果有 PhotoBlobId）
            ViewBag.PhotoUrl = await _photoService.GetPhotoUrlAsync(caseItem.PhotoBlobId);
            ViewBag.PhotoBlobId = caseItem.PhotoBlobId;
            
            // 建立 CaseFormVM 並設置為 ReadOnly 模式
            var viewModel = new CaseFormVM
            {
                Mode = CaseFormMode.ReadOnly,
                Case = caseItem,
                Cities = optionsData.Cities,
                Schools = optionsData.Schools,
                GenderOptions = optionsData.GenderOptions,
                Districts = new List<Domain.Case.Shared.Models.District>()
            };
            
            SetNavigationContext("ReadOnly");
            return View("~/Views/Case/Basic/View/Item.cshtml", viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"載入個案資料時發生錯誤：{ex.Message}";
            return RedirectToAction(nameof(Query));
        }
    }
}
