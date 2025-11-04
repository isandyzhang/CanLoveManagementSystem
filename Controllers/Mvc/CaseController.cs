using Microsoft.AspNetCore.Mvc;
using CanLove_Backend.Data.Contexts;
using CanLove_Backend.Data.Models.Core;
using CanLove_Backend.Data.Models.Options;
using CanLove_Backend.Models.Mvc.ViewModels;
using CanLove_Backend.Services.Case;
using CanLove_Backend.Services.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.IO;

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
    private readonly Services.Shared.IBlobService _blobService;

    public CaseController(CanLoveDbContext context, SchoolService schoolService, OptionService optionService, CaseService caseService, Services.Shared.IBlobService blobService)
    {
        _context = context;
        _schoolService = schoolService;
        _optionService = optionService;
        _caseService = caseService;
        _blobService = blobService;
    }

    /// <summary>
    /// 取得個案詳細資料 (AJAX API)
    /// </summary>
    // [Authorize(Policy = "RequireViewer")] // 暫時註解掉進行測試
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
    /// 個案列表頁面 - 簡化版（用於診斷問題）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            // 最小查詢（加強版）：僅選取安全欄位以避免 DB/模型型別不一致造成的轉型錯誤
            var cases = await _context.Cases
                .Include(c => c.City) // City 驗證完成
                .Include(c => c.District) // District 驗證完成
                .Include(c => c.School) // 加入第三層關聯：School
                .Where(c => c.Deleted != true && c.Status == "Approved")
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CanLove_Backend.Data.Models.Core.Case
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

            return View("Index", cases);
        }
        catch (Exception ex)
        {
            // 錯誤處理：顯示錯誤訊息
            ViewBag.ErrorMessage = $"查詢資料時發生錯誤：{ex.Message}";
            return View("Index", new List<Case>());
        }
    }
    

    /// <summary>
    /// 個案建立頁面
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        // 設置麵包屑父級
        ViewData["BreadcrumbParent"] = "個案管理";
        ViewData["BreadcrumbParentUrl"] = Url.Action("Index", "Case");
        
        // 設置 Sidebar 項目名稱（父層維持「新增個案」）
        ViewData["Sidebar.CreateCase"] = "新增個案";
        
        try
        {
            var viewModel = new CaseCreateViewModel
            {
                Case = new CanLove_Backend.Data.Models.Core.Case
                {
                    CaseId = string.Empty,
                    AssessmentDate = DateOnly.FromDateTime(DateTime.Today)
                },
                Cities = await _context.Cities.OrderBy(c => c.CityId).ToListAsync(),
                Districts = new List<District>(),
                Schools = await _schoolService.GetAllSchoolsAsync(),
                GenderOptions = await _optionService.GetGenderOptionsAsync()
            };

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
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = $"載入頁面時發生錯誤：{ex.Message}";
            return View(new CaseCreateViewModel
            {
                Case = new CanLove_Backend.Data.Models.Core.Case(),
                Cities = new List<City>(),
                Districts = new List<District>(),
                Schools = new List<School>(),
                GenderOptions = new List<OptionSetValue>()
            });
        }
    }

    /// <summary>
    /// 個案建立處理
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    // [Authorize(Policy = "RequireAssistant")] // 暫時註解掉進行測試
    public async Task<IActionResult> Create(CaseCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            // 處理照片上傳
            if (model.PhotoFile != null && model.PhotoFile.Length > 0)
            {
                try
                {
                    // 驗證檔案類型
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(model.PhotoFile.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("PhotoFile", "僅支援 JPG、PNG、GIF 格式的圖片");
                    }
                    else
                    {
                        // 驗證檔案大小（限制 5MB）
                        if (model.PhotoFile.Length > 5 * 1024 * 1024)
                        {
                            ModelState.AddModelError("PhotoFile", "檔案大小不能超過 5MB");
                        }
                        else
                        {
                            // 取得目前使用者ID（如果有的話）
                            int? uploadedBy = null;
                            if (User.Identity?.IsAuthenticated == true && !string.IsNullOrEmpty(User.Identity.Name))
                            {
                                var staff = await _context.Staff
                                    .FirstOrDefaultAsync(s => s.Email == User.Identity.Name && !s.Deleted);
                                uploadedBy = staff?.StaffId;
                            }

                            // 上傳檔案到 Blob Storage
                            // 使用個案編號作為檔案名稱的一部分
                            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                            var blobName = $"{model.Case.CaseId}_photo_{timestamp}{fileExtension}";
                            var blobStorage = await _blobService.UploadFileAsync(
                                model.PhotoFile.OpenReadStream(),
                                "cases",
                                blobName,
                                model.PhotoFile.ContentType,
                                uploadedBy,
                                false);

                            // 設定 PhotoBlobId
                            model.Case.PhotoBlobId = blobStorage.BlobId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("PhotoFile", $"照片上傳失敗：{ex.Message}");
                }
            }

            // 如果照片上傳失敗，不繼續建立個案
            if (!ModelState.IsValid)
            {
                model.Cities = await _context.Cities.OrderBy(c => c.CityId).ToListAsync();
                model.Districts = new List<District>();
                model.Schools = await _schoolService.GetAllSchoolsAsync();
                model.GenderOptions = await _optionService.GetGenderOptionsAsync();
                return View(model);
            }

            // 設定個案為待審閱狀態（點擊「存檔送審」表示要送審）
            model.Case.Status = "PendingReview";
            model.Case.SubmittedBy = User.Identity?.Name ?? "";
            model.Case.SubmittedAt = DateTime.UtcNow;
            model.Case.CreatedAt = DateTime.UtcNow;
            model.Case.UpdatedAt = DateTime.UtcNow;
            
            var response = await _caseService.CreateCaseAsync(model.Case);
            
            if (response.Success)
            {
                TempData["SuccessMessage"] = "個案建立成功";

                // 留在建立頁，並重設表單預設值與下拉資料
                var resetViewModel = new CaseCreateViewModel
                {
                    Case = new CanLove_Backend.Data.Models.Core.Case
                    {
                        CaseId = string.Empty,
                        AssessmentDate = DateOnly.FromDateTime(DateTime.Today)
                    },
                    Cities = await _context.Cities.OrderBy(c => c.CityId).ToListAsync(),
                    Districts = new List<District>(),
                    Schools = await _schoolService.GetAllSchoolsAsync(),
                    GenderOptions = await _optionService.GetGenderOptionsAsync()
                };

                return View("Create", resetViewModel);
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
    /// 搜尋個案頁面（用於開案記錄表）
    /// </summary>
    [HttpGet]
    public IActionResult SearchForOpenCase()
    {
        // 設置 Sidebar 項目名稱
        ViewData["Sidebar.CreateCase"] = "新增開案紀錄表";
        return View();
    }

    /// <summary>
    /// 搜尋個案 API（AJAX）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> SearchCases(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Json(new { success = true, cases = new List<object>() });
        }

        try
        {
            var searchTerm = query.Trim();

            var cases = await _context.Cases
                .Include(c => c.City)
                .Include(c => c.District)
                .Include(c => c.School)
                .Where(c => c.Deleted != true && (
                    c.CaseId.Contains(searchTerm) ||
                    (c.Name != null && c.Name.Contains(searchTerm)) ||
                    (c.Phone != null && c.Phone.Contains(searchTerm))
                ))
                .OrderByDescending(c => c.CreatedAt)
                .Take(50)
                .Select(c => new
                {
                    caseId = c.CaseId,
                    name = c.Name,
                    phone = c.Phone ?? "",
                    birthDate = c.BirthDate.ToString("yyyy-MM-dd"),
                    city = c.City != null ? c.City.CityName : "",
                    district = c.District != null ? c.District.DistrictName : "",
                    school = c.School != null ? c.School.SchoolName : ""
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
    /// 個案編輯頁面
    /// </summary>
    [HttpGet]
    // [Authorize(Policy = "RequireSocialWorker")] // 暫時註解掉進行測試
    public async Task<IActionResult> Edit(string id)
    {
        // 設置麵包屑父級
        ViewData["BreadcrumbParent"] = "個案管理";
        ViewData["BreadcrumbParentUrl"] = Url.Action("Index", "Case");
        
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
    // [Authorize(Policy = "RequireSocialWorker")] // 暫時註解掉進行測試
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
    // [Authorize(Policy = "RequireAdmin")] // 暫時註解掉進行測試
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
    // [Authorize(Policy = "RequireAdmin")] // 暫時註解掉進行測試
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
    // [Authorize(Policy = "RequireViewer")] // 暫時註解掉進行測試
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
    // [Authorize(Policy = "RequireAssistant")] // 暫時註解掉進行測試
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
        caseItem.Status = "PendingReview";
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
    // [Authorize(Policy = "RequireSocialWorker")] // 暫時註解掉進行測試
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
        
        if (approved)
        {
            // 審核通過
            caseItem.Status = "Approved";
        }
        else
        {
            // 退回：重置提交狀態
            caseItem.SubmittedAt = null;
            caseItem.Status = "Rejected";
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
    // [Authorize(Policy = "RequireSocialWorker")] // 暫時註解掉進行測試
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

    /// <summary>
    /// 開案記錄表頁面（重定向到開案流程 Step1）
    /// </summary>
    [HttpGet]
    public IActionResult OpenCaseRecord()
    {
        // 重定向到開案記錄表 Step1（會顯示選擇個案搜尋功能）
        return RedirectToAction("Step1", "CaseWizardOpenCase");
    }

    /// <summary>
    /// 關懷訪視記錄表頁面
    /// </summary>
    [HttpGet]
    public IActionResult CareVisitRecord()
    {
        // 設置 Sidebar 項目名稱
        ViewData["Sidebar.CareVisitRecord"] = "關懷訪視記錄表";
        return View();
    }

    /// <summary>
    /// 會談服務記錄表頁面
    /// </summary>
    [HttpGet]
    public IActionResult ConsultationRecord()
    {
        // 設置 Sidebar 項目名稱
        ViewData["Sidebar.ConsultationRecord"] = "會談服務紀錄表";
        return View();
    }

    /// <summary>
    /// 個案審核頁面
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Review()
    {
        ViewData["Title"] = "個案審核";
        var userRoles = User.FindAll("roles").Select(c => c.Value).ToList();
        
        IQueryable<Case> query = _context.Cases
            .Include(c => c.City)
            .Include(c => c.District)
            .Include(c => c.School)
            .Where(c => c.Deleted != true);

        // 只顯示待審閱的個案
        query = query.Where(c => c.Status == "PendingReview");

        var cases = await query
            .OrderByDescending(c => c.SubmittedAt)
            .Select(c => new CanLove_Backend.Data.Models.Core.Case
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

        return View("Review", cases); // 使用專用 Review 視圖顯示待審閱個案
    }

    /// <summary>
    /// 審閱用的基本資料表單（僅 Case 資料）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ReviewForm(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return View("NotFound");

        var item = await _context.Cases
            .Where(c => c.CaseId == id)
            .Select(c => new CanLove_Backend.Data.Models.Core.Case
            {
                CaseId = c.CaseId,
                AssessmentDate = c.AssessmentDate,
                Name = c.Name,
                Gender = c.Gender,
                SchoolId = c.SchoolId,
                BirthDate = c.BirthDate,
                IdNumber = c.IdNumber,
                Address = c.Address,
                CityId = c.CityId,
                DistrictId = c.DistrictId,
                Phone = c.Phone,
                Email = c.Email,
                Status = c.Status
            })
            .AsNoTracking()
            .SingleOrDefaultAsync();

        if (item == null) return View("NotFound");
        return View("ReviewForm", item);
    }

    /// <summary>
    /// 審閱用基本資料更新（僅更新 Case 基本欄位）
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateBasic(CanLove_Backend.Data.Models.Core.Case form)
    {
        if (string.IsNullOrWhiteSpace(form.CaseId)) return View("NotFound");

        var item = await _context.Cases.FindAsync(form.CaseId);
        if (item == null) return View("NotFound");

        // 僅更新基本欄位
        item.Name = form.Name;
        item.Gender = form.Gender;
        item.BirthDate = form.BirthDate;
        item.Phone = form.Phone;
        item.Email = form.Email;
        item.Address = form.Address;
        item.CityId = form.CityId;
        item.DistrictId = form.DistrictId;
        item.SchoolId = form.SchoolId;
        item.UpdatedAt = DateTime.UtcNow;

        _context.Update(item);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "已儲存基本資料";
        return RedirectToAction(nameof(ReviewForm), new { id = form.CaseId });
    }

    private bool CaseExists(string id)
    {
        return _context.Cases.Any(e => e.CaseId == id);
    }
}
