using Microsoft.AspNetCore.Mvc;
using CanLove_Backend.Infrastructure.Data.Contexts;
using CanLove_Backend.Domain.Case.Models.Basic;
using CanLove_Backend.Domain.Case.Shared.Models;
using CanLove_Backend.Domain.Case.ViewModels.Basic;
using CanLove_Backend.Domain.Case.Services.Basic;
using CanLove_Backend.Domain.Case.Shared.Services;
using CanLove_Backend.Infrastructure.Storage.Blob;
using CanLove_Backend.Infrastructure.Storage.Encryption;
using CanLove_Backend.Infrastructure.Options.Services;
using CanLove_Backend.Infrastructure.Options.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using CanLove_Backend.Core.Extensions;

namespace CanLove_Backend.Application.Controllers.Case;

/// <summary>
/// 個案基本資料管理控制器
/// </summary>
public class CaseBasicController : Controller
{
    private readonly CanLoveDbContext _context;
    private readonly SchoolService _schoolService;
    private readonly OptionService _optionService;
    private readonly CaseService _caseService;
    private readonly IBlobService _blobService;
    private readonly DataEncryptionService _encryptionService;

    public CaseBasicController(CanLoveDbContext context, SchoolService schoolService, OptionService optionService, CaseService caseService, IBlobService blobService, DataEncryptionService encryptionService)
    {
        _context = context;
        _schoolService = schoolService;
        _optionService = optionService;
        _caseService = caseService;
        _blobService = blobService;
        _encryptionService = encryptionService;
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
                    // 解密身分證字號
                    idNumber = _encryptionService.DecryptSafely(caseData.IdNumber),
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
    /// 個案建立頁面
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        // 設置麵包屑父級
        ViewData["BreadcrumbParent"] = "個案管理";
        ViewData["BreadcrumbParentUrl"] = Url.Action("Query", "CaseBasic");
        
        // 設置 Sidebar 項目名稱（父層維持「新增個案」）
        ViewData["Sidebar.CreateCase"] = "新增個案";
        
        try
        {
            var viewModel = new CaseFormViewModel
            {
                Mode = CaseFormMode.Create,
                Case = new CanLove_Backend.Domain.Case.Models.Basic.Case
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

            return View("~/Views/Case/Basic/Create.cshtml", viewModel);
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = $"載入頁面時發生錯誤：{ex.Message}";
            return View("~/Views/Case/Basic/Create.cshtml", new CaseFormViewModel
            {
                Mode = CaseFormMode.Create,
                Case = new CanLove_Backend.Domain.Case.Models.Basic.Case(),
                Cities = new List<City>(),
                Districts = new List<District>(),
                Schools = new List<School>(),
                GenderOptions = new List<CanLove_Backend.Infrastructure.Options.Models.OptionSetValue>()
            });
        }
    }

    /// <summary>
    /// 個案建立處理
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    // [Authorize(Policy = "RequireAssistant")] // 暫時註解掉進行測試
    public async Task<IActionResult> Create(CaseFormViewModel model)
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
                                var staff = await _context.Staffs
                                    .FirstOrDefaultAsync(s => s.Email == User.Identity.Name && !s.Deleted);
                                uploadedBy = staff?.StaffId;
                            }

                            // 上傳檔案到 Blob Storage
                            // 使用個案編號作為檔案名稱的一部分
                            var timestamp = DateTimeExtensions.TaiwanTime.ToString("yyyyMMddHHmmss");
                            var blobName = $"avatars/{model.Case.CaseId}/photo_{timestamp}{fileExtension}";
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
                return View("~/Views/Case/Basic/Create.cshtml", model);
            }

            // 設定個案為待審閱狀態（點擊「提交審核」表示要送審）
            model.Case.Status = "PendingReview";
            model.Case.SubmittedBy = User.Identity?.Name ?? "";
            model.Case.SubmittedAt = DateTimeExtensions.TaiwanTime;
            model.Case.CreatedAt = DateTimeExtensions.TaiwanTime;
            model.Case.UpdatedAt = DateTimeExtensions.TaiwanTime;
            
            var response = await _caseService.CreateCaseAsync(model.Case);
            
            if (response.Success)
            {
                TempData["SuccessMessage"] = "個案建立成功";

                // 留在建立頁，並重設表單預設值與下拉資料
                var resetViewModel = new CaseFormViewModel
                {
                    Mode = CaseFormMode.Create,
                    Case = new CanLove_Backend.Domain.Case.Models.Basic.Case
                    {
                        CaseId = string.Empty,
                        AssessmentDate = DateOnly.FromDateTime(DateTime.Today)
                    },
                    Cities = await _context.Cities.OrderBy(c => c.CityId).ToListAsync(),
                    Districts = new List<District>(),
                    Schools = await _schoolService.GetAllSchoolsAsync(),
                    GenderOptions = await _optionService.GetGenderOptionsAsync()
                };

                return View("~/Views/Case/Basic/Create.cshtml", resetViewModel);
            }
            else
            {
                // 將錯誤訊息加入 ModelState，顯示在表單上方
                ModelState.AddModelError("", response.Message);
                // 如果錯誤訊息包含 CaseId，也加入 CaseId 欄位
                if (response.Message.Contains("個案編號") || response.Message.Contains("CaseId"))
                {
                    ModelState.AddModelError("Case.CaseId", response.Message);
                }
            }
        }

        // 如果驗證失敗，重新載入下拉選單資料
        model.Cities = await _context.Cities.OrderBy(c => c.CityId).ToListAsync();
        model.Districts = new List<District>(); // 初始為空，等選擇城市後載入
        model.Schools = await _schoolService.GetAllSchoolsAsync(); // 載入所有學校供獨立選擇
        model.GenderOptions = await _optionService.GetGenderOptionsAsync();

        return View("~/Views/Case/Basic/Create.cshtml", model);
    }

    /// <summary>
    /// 搜尋個案 API（AJAX）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> SearchCases(string query)
    {
        try
        {
            var queryable = _context.Cases
                .Include(c => c.City)
                .Include(c => c.District)
                .Include(c => c.School)
                .Where(c => c.Deleted != true && c.Status == "Approved");

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
    /// 個案編輯頁面（別名：EditItem）
    /// </summary>
    [HttpGet]
    // [Authorize(Policy = "RequireSocialWorker")] // 暫時註解掉進行測試
    public async Task<IActionResult> EditItem(string id)
    {
        return await Edit(id);
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
        ViewData["BreadcrumbParentUrl"] = Url.Action("Query", "CaseBasic");
        
        if (id == null)
        {
            return View("NotFound");
        }

        var caseItem = await _context.Cases
            .Where(c => c.CaseId == id)
            .Select(c => new
            {
                CaseId = c.CaseId,
                AssessmentDate = c.AssessmentDate,
                Name = c.Name,
                Gender = c.Gender,
                BirthDate = c.BirthDate,
                IdNumber = c.IdNumber,
                SchoolId = c.SchoolId,
                CityId = c.CityId,
                DistrictId = c.DistrictId,
                Phone = c.Phone,
                Email = c.Email,
                Address = c.Address,
                PhotoBlobId = c.PhotoBlobId
            })
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (caseItem == null)
        {
            return View("NotFound");
        }

        // 檢查個案是否被鎖定（需要重新查詢完整資料）
        var fullCase = await _context.Cases.FindAsync(id);
        if (fullCase?.IsLocked == true && fullCase.LockedBy != User.Identity?.Name)
        {
            TempData["ErrorMessage"] = "此個案已被其他使用者鎖定，無法編輯";
            return RedirectToAction(nameof(Index));
        }

        // 建立 Case 物件
        var caseModel = new CanLove_Backend.Domain.Case.Models.Basic.Case
        {
            CaseId = caseItem.CaseId,
            AssessmentDate = caseItem.AssessmentDate,
            Name = caseItem.Name,
            Gender = caseItem.Gender,
            BirthDate = caseItem.BirthDate,
            IdNumber = caseItem.IdNumber,
            SchoolId = caseItem.SchoolId,
            CityId = caseItem.CityId,
            DistrictId = caseItem.DistrictId,
            Phone = caseItem.Phone,
            Email = caseItem.Email,
            Address = caseItem.Address,
            PhotoBlobId = caseItem.PhotoBlobId
        };

        // 解密身分證字號以便顯示
        if (!string.IsNullOrWhiteSpace(caseModel.IdNumber))
        {
            caseModel.IdNumber = _encryptionService.DecryptSafely(caseModel.IdNumber);
        }

        // 載入選項資料
        var cities = await _context.Cities.OrderBy(c => c.CityId).AsNoTracking().ToListAsync();
        var schools = await _schoolService.GetAllSchoolsAsync();
        var genderOptions = await _optionService.GetGenderOptionsAsync();

        // 載入地區資料（按城市分組）
        var allDistricts = await _context.Districts
            .Select(d => new { 
                DistrictId = d.DistrictId, 
                DistrictName = d.DistrictName,
                CityId = d.CityId
            })
            .OrderBy(d => d.CityId)
            .ThenBy(d => d.DistrictName)
            .AsNoTracking()
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

        // 取得照片 URL（如果有 PhotoBlobId）
        string? photoUrl = null;
        if (caseModel.PhotoBlobId.HasValue)
        {
            try
            {
                photoUrl = await _blobService.GetFileUrlAsync(caseModel.PhotoBlobId.Value);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"取得照片 URL 失敗 (BlobId: {caseModel.PhotoBlobId.Value}): {ex.Message}");
            }
        }
        ViewBag.PhotoUrl = photoUrl;

        // 建立 CaseFormViewModel
        var viewModel = new CaseFormViewModel
        {
            Mode = CaseFormMode.Create, // 編輯模式使用 Create（可編輯）
            Case = caseModel,
            Cities = cities,
            Schools = schools,
            GenderOptions = genderOptions,
            Districts = new List<District>() // 地區資料通過 ViewBag.DistrictsByCity 傳遞
        };
        
        return View("~/Views/Case/Basic/Edit/Item.cshtml", viewModel);
    }

    /// <summary>
    /// 個案編輯處理
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    // [Authorize(Policy = "RequireSocialWorker")] // 暫時註解掉進行測試
    public async Task<IActionResult> Edit(string id, CaseFormViewModel model)
    {
        if (id != model.Case.CaseId)
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

                // 處理照片上傳（如果有新照片）
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
                                // 如果已有舊照片，先刪除舊的 blob
                                if (existingCase?.PhotoBlobId.HasValue == true)
                                {
                                    try
                                    {
                                        await _blobService.DeleteFileAsync(existingCase.PhotoBlobId.Value);
                                    }
                                    catch
                                    {
                                        // 如果刪除失敗，繼續上傳新照片
                                    }
                                }

                                // 取得目前使用者ID（如果有的話）
                                int? uploadedBy = null;
                                if (User.Identity?.IsAuthenticated == true && !string.IsNullOrEmpty(User.Identity.Name))
                                {
                                    var staff = await _context.Staffs
                                        .FirstOrDefaultAsync(s => s.Email == User.Identity.Name && !s.Deleted);
                                    uploadedBy = staff?.StaffId;
                                }

                                // 上傳新檔案到 Blob Storage
                                var timestamp = DateTimeExtensions.TaiwanTime.ToString("yyyyMMddHHmmss");
                                var blobName = $"avatars/{model.Case.CaseId}/photo_{timestamp}{fileExtension}";
                                var blobStorage = await _blobService.UploadFileAsync(
                                    model.PhotoFile.OpenReadStream(),
                                    "cases",
                                    blobName,
                                    model.PhotoFile.ContentType,
                                    uploadedBy,
                                    false);

                                // 設定新的 PhotoBlobId
                                model.Case.PhotoBlobId = blobStorage.BlobId;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("PhotoFile", $"照片上傳失敗：{ex.Message}");
                    }
                }
                else
                {
                    // 如果沒有上傳新照片，保留原有的 PhotoBlobId
                    if (existingCase != null)
                    {
                        model.Case.PhotoBlobId = existingCase.PhotoBlobId;
                    }
                }

                // 如果照片上傳失敗，不繼續更新個案
                if (!ModelState.IsValid)
                {
                    model.Cities = await _context.Cities.OrderBy(c => c.CityId).ToListAsync();
                    model.Schools = await _schoolService.GetAllSchoolsAsync();
                    model.GenderOptions = await _optionService.GetGenderOptionsAsync();
                    model.Districts = new List<District>();
                    
                    // 載入地區資料
                    var allDistrictsForError = await _context.Districts
                        .Select(d => new { 
                            DistrictId = d.DistrictId, 
                            DistrictName = d.DistrictName,
                            CityId = d.CityId
                        })
                        .OrderBy(d => d.CityId)
                        .ThenBy(d => d.DistrictName)
                        .AsNoTracking()
                        .ToListAsync();

                    var districtsByCityForError = allDistrictsForError
                        .GroupBy(d => d.CityId)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(d => new { 
                                districtId = d.DistrictId, 
                                districtName = d.DistrictName 
                            }).ToList()
                        );

                    ViewBag.DistrictsByCity = districtsByCityForError;
                    
                    // 取得照片 URL
                    string? photoUrlForError = null;
                    if (model.Case.PhotoBlobId.HasValue)
                    {
                        try
                        {
                            photoUrlForError = await _blobService.GetFileUrlAsync(model.Case.PhotoBlobId.Value);
                        }
                        catch { }
                    }
                    ViewBag.PhotoUrl = photoUrlForError;
                    
                    return View("~/Views/Case/Basic/Edit/Item.cshtml", model);
                }

                // 加密身分證字號
                if (!string.IsNullOrWhiteSpace(model.Case.IdNumber))
                {
                    model.Case.IdNumber = _encryptionService.Encrypt(model.Case.IdNumber);
                }
                
                // 保留原有的建立時間和審核相關欄位
                if (existingCase != null)
                {
                    model.Case.CreatedAt = existingCase.CreatedAt;
                    model.Case.Status = existingCase.Status;
                    model.Case.SubmittedBy = existingCase.SubmittedBy;
                    model.Case.SubmittedAt = existingCase.SubmittedAt;
                    model.Case.ReviewedBy = existingCase.ReviewedBy;
                    model.Case.ReviewedAt = existingCase.ReviewedAt;
                }
                
                model.Case.UpdatedAt = DateTimeExtensions.TaiwanTime;
                _context.Update(model.Case);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "個案更新成功";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CaseExists(model.Case.CaseId))
                {
                    return View("NotFound");
                }
                else
                {
                    throw;
                }
            }
        }

        // 如果驗證失敗，重新載入選項資料
        model.Cities = await _context.Cities.OrderBy(c => c.CityId).ToListAsync();
        model.Schools = await _schoolService.GetAllSchoolsAsync();
        model.GenderOptions = await _optionService.GetGenderOptionsAsync();
        model.Districts = new List<District>();
        
        // 載入地區資料
        var allDistricts = await _context.Districts
            .Select(d => new { 
                DistrictId = d.DistrictId, 
                DistrictName = d.DistrictName,
                CityId = d.CityId
            })
            .OrderBy(d => d.CityId)
            .ThenBy(d => d.DistrictName)
            .AsNoTracking()
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
        
        // 取得照片 URL
        string? photoUrl = null;
        if (model.Case.PhotoBlobId.HasValue)
        {
            try
            {
                photoUrl = await _blobService.GetFileUrlAsync(model.Case.PhotoBlobId.Value);
            }
            catch { }
        }
        ViewBag.PhotoUrl = photoUrl;
        
        return View("~/Views/Case/Basic/Edit/Item.cshtml", model);
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

        return View("~/Views/Case/Basic/Details.cshtml", caseItem);
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
            caseItem.DeletedAt = DateTimeExtensions.TaiwanTime;
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

        return View("~/Views/Case/Basic/Details.cshtml", caseItem);
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

        caseItem.SubmittedAt = DateTimeExtensions.TaiwanTime;
        caseItem.Status = "PendingReview";
        caseItem.UpdatedAt = DateTimeExtensions.TaiwanTime;
        
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
        caseItem.ReviewedAt = DateTimeExtensions.TaiwanTime;
        caseItem.UpdatedAt = DateTimeExtensions.TaiwanTime;
        
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
            caseItem.LockedAt = DateTimeExtensions.TaiwanTime;
            TempData["SuccessMessage"] = "個案已鎖定";
        }
        
        caseItem.UpdatedAt = DateTimeExtensions.TaiwanTime;
        _context.Update(caseItem);
        await _context.SaveChangesAsync();
        
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// 查詢 - 基本資料
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Query(string? caseId = null, bool showAll = false)
    {
        ViewData["Title"] = "查詢個案 - 基本資料";
        ViewBag.CurrentPage = "Search";
        ViewBag.CurrentTab = "CaseBasic";
        
        // 預設載入所有已審核個案列表
        try
        {
            var allCases = await _context.Cases
                .Include(c => c.City)
                .Include(c => c.District)
                .Include(c => c.School)
                .Where(c => c.Deleted != true && c.Status == "Approved")
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CanLove_Backend.Domain.Case.Models.Basic.Case
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
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = $"查詢資料時發生錯誤：{ex.Message}";
            ViewBag.AllCases = new List<CanLove_Backend.Domain.Case.Models.Basic.Case>();
            ViewBag.ShowAllCases = true;
        }
        
        return View("~/Views/Case/Basic/Search/Index.cshtml");
    }

    /// <summary>
    /// 查看個案詳細資料（只讀模式）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ViewItem(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return View("NotFound");
        }

        var caseItem = await _context.Cases
            .Where(c => c.CaseId == id)
            .Select(c => new
            {
                CaseId = c.CaseId,
                AssessmentDate = c.AssessmentDate,
                Name = c.Name,
                Gender = c.Gender,
                BirthDate = c.BirthDate,
                IdNumber = c.IdNumber,
                SchoolId = c.SchoolId,
                CityId = c.CityId,
                DistrictId = c.DistrictId,
                Phone = c.Phone,
                Email = c.Email,
                Address = c.Address,
                PhotoBlobId = c.PhotoBlobId
            })
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (caseItem == null)
        {
            return View("NotFound");
        }

        // 手動映射到 Case 物件
        var item = new CanLove_Backend.Domain.Case.Models.Basic.Case
        {
            CaseId = caseItem.CaseId,
            AssessmentDate = caseItem.AssessmentDate,
            Name = caseItem.Name,
            Gender = caseItem.Gender,
            BirthDate = caseItem.BirthDate,
            IdNumber = caseItem.IdNumber,
            SchoolId = caseItem.SchoolId,
            CityId = caseItem.CityId,
            DistrictId = caseItem.DistrictId,
            Phone = caseItem.Phone,
            Email = caseItem.Email,
            Address = caseItem.Address,
            PhotoBlobId = caseItem.PhotoBlobId
        };
        
        // 解密身分證字號
        if (!string.IsNullOrWhiteSpace(item.IdNumber))
        {
            item.IdNumber = _encryptionService.DecryptSafely(item.IdNumber);
        }

        // 載入選項資料
        var cities = await _context.Cities
            .Select(c => new City { 
                CityId = c.CityId, 
                CityName = c.CityName 
            })
            .OrderBy(c => c.CityId)
            .AsNoTracking()
            .ToListAsync();
        var schools = await _schoolService.GetAllSchoolsAsync();
        var genderOptions = await _optionService.GetGenderOptionsAsync();
        
        // 載入地區資料（按城市分組）
        var allDistricts = await _context.Districts
            .Select(d => new { 
                DistrictId = d.DistrictId, 
                DistrictName = d.DistrictName,
                CityId = d.CityId
            })
            .OrderBy(d => d.CityId)
            .ThenBy(d => d.DistrictName)
            .AsNoTracking()
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
        
        // 取得照片 URL（如果有 PhotoBlobId）
        string? photoUrl = null;
        if (item.PhotoBlobId.HasValue)
        {
            try
            {
                photoUrl = await _blobService.GetFileUrlAsync(item.PhotoBlobId.Value);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"取得照片 URL 失敗 (BlobId: {item.PhotoBlobId.Value}): {ex.Message}");
            }
        }
        ViewBag.PhotoUrl = photoUrl;
        ViewBag.PhotoBlobId = item.PhotoBlobId;
        
        // 建立 CaseFormViewModel 並設置為 ReadOnly 模式
        var viewModel = new CaseFormViewModel
        {
            Mode = CaseFormMode.ReadOnly,
            Case = item,
            Cities = cities,
            Schools = schools,
            GenderOptions = genderOptions,
            Districts = new List<District>()
        };
        
        return View("~/Views/Case/Basic/View/Item.cshtml", viewModel);
    }

    /// <summary>
    /// 個案審核頁面（個案基本資料）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Review(string? caseId)
    {
        ViewData["Title"] = "個案審核 - 個案基本資料";
        ViewBag.CurrentPage = "Review";
        ViewBag.CurrentTab = "CaseBasic";
        
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
        
        // 查詢 Cases 表中狀態為 PendingReview 的記錄
        var cases = await _context.Cases
            .Where(c => c.Status == "PendingReview" && c.Deleted != true)
            .OrderByDescending(c => c.SubmittedAt)
            .AsNoTracking()
            .ToListAsync();

        // 取得提交者顯示名稱（以 Staff.DisplayName 對照 SubmittedBy Email）
        var emails = cases
            .Where(c => !string.IsNullOrWhiteSpace(c.SubmittedBy))
            .Select(c => c.SubmittedBy!)
            .Distinct()
            .ToList();

        var staffMap = await _context.Staffs
            .Where(s => emails.Contains(s.Email))
            .Select(s => new { s.Email, s.DisplayName })
            .ToDictionaryAsync(x => x.Email, x => x.DisplayName);

        ViewBag.SubmitterNameMap = staffMap;

        return View("~/Views/Case/Basic/Review/Index.cshtml", cases);
    }

    /// <summary>
    /// 審核個案詳細資料（Review 模式）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ReviewItem(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return View("NotFound");
        }

        var selectedCase = await _context.Cases
            .Where(c => c.CaseId == id && c.Deleted != true)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (selectedCase == null || selectedCase.Status != "PendingReview")
        {
            TempData["ErrorMessage"] = "找不到指定的待審個案";
            return RedirectToAction(nameof(Review));
        }

        // 查詢個案資料
        CanLove_Backend.Domain.Case.Models.Basic.Case? item = null;
        try
        {
            var caseData = await _context.Cases
                .Where(c => c.CaseId == id && c.Deleted != true)
                .Select(c => new
                {
                    CaseId = c.CaseId,
                    AssessmentDate = c.AssessmentDate,
                    Name = c.Name,
                    Gender = c.Gender,
                    BirthDate = c.BirthDate,
                    IdNumber = c.IdNumber,
                    SchoolId = c.SchoolId,
                    CityId = c.CityId,
                    DistrictId = c.DistrictId,
                    Phone = c.Phone,
                    Email = c.Email,
                    Address = c.Address,
                    PhotoBlobId = c.PhotoBlobId
                })
                .AsNoTracking()
                .SingleOrDefaultAsync();

            if (caseData != null)
            {
                item = new CanLove_Backend.Domain.Case.Models.Basic.Case
                {
                    CaseId = caseData.CaseId,
                    AssessmentDate = caseData.AssessmentDate,
                    Name = caseData.Name,
                    Gender = caseData.Gender,
                    BirthDate = caseData.BirthDate,
                    IdNumber = caseData.IdNumber,
                    SchoolId = caseData.SchoolId,
                    CityId = caseData.CityId,
                    DistrictId = caseData.DistrictId,
                    Phone = caseData.Phone,
                    Email = caseData.Email,
                    Address = caseData.Address,
                    PhotoBlobId = caseData.PhotoBlobId
                };
                
                // 解密身分證字號
                if (!string.IsNullOrWhiteSpace(item.IdNumber))
                {
                    item.IdNumber = _encryptionService.DecryptSafely(item.IdNumber);
                }
            }
        }
        catch (InvalidCastException)
        {
            // 如果型別轉換失敗，使用原始 SQL 查詢並手動處理型別轉換
            var caseDataWithPhoto = await _context.Cases
                .Where(c => c.CaseId == id && c.Deleted != true)
                .Select(c => new
                {
                    CaseId = c.CaseId,
                    AssessmentDate = c.AssessmentDate,
                    Name = c.Name,
                    Gender = c.Gender,
                    BirthDate = c.BirthDate,
                    IdNumber = c.IdNumber,
                    SchoolId = c.SchoolId,
                    CityId = c.CityId,
                    DistrictId = c.DistrictId,
                    Phone = c.Phone,
                    Email = c.Email,
                    Address = c.Address,
                    PhotoBlobIdStr = c.PhotoBlobId != null ? c.PhotoBlobId.ToString() : null
                })
                .AsNoTracking()
                .SingleOrDefaultAsync();

            if (caseDataWithPhoto != null)
            {
                int? photoBlobId = null;
                if (!string.IsNullOrEmpty(caseDataWithPhoto.PhotoBlobIdStr) && int.TryParse(caseDataWithPhoto.PhotoBlobIdStr, out int parsedId))
                {
                    photoBlobId = parsedId;
                }
                
                item = new CanLove_Backend.Domain.Case.Models.Basic.Case
                {
                    CaseId = caseDataWithPhoto.CaseId,
                    AssessmentDate = caseDataWithPhoto.AssessmentDate,
                    Name = caseDataWithPhoto.Name,
                    Gender = caseDataWithPhoto.Gender,
                    BirthDate = caseDataWithPhoto.BirthDate,
                    IdNumber = caseDataWithPhoto.IdNumber,
                    SchoolId = caseDataWithPhoto.SchoolId,
                    CityId = caseDataWithPhoto.CityId,
                    DistrictId = caseDataWithPhoto.DistrictId,
                    Phone = caseDataWithPhoto.Phone,
                    Email = caseDataWithPhoto.Email,
                    Address = caseDataWithPhoto.Address,
                    PhotoBlobId = photoBlobId
                };
                
                // 解密身分證字號
                if (!string.IsNullOrWhiteSpace(item.IdNumber))
                {
                    item.IdNumber = _encryptionService.DecryptSafely(item.IdNumber);
                }
            }
        }

        if (item == null)
        {
            TempData["ErrorMessage"] = "找不到指定的個案";
            return RedirectToAction(nameof(Review));
        }

        // 載入選項資料
        var cities = await _context.Cities
            .Select(c => new City { 
                CityId = c.CityId, 
                CityName = c.CityName 
            })
            .OrderBy(c => c.CityId)
            .AsNoTracking()
            .ToListAsync();
        var schools = await _schoolService.GetAllSchoolsAsync();
        var genderOptions = await _optionService.GetGenderOptionsAsync();
        
        // 載入地區資料（按城市分組）
        var allDistricts = await _context.Districts
            .Select(d => new { 
                DistrictId = d.DistrictId, 
                DistrictName = d.DistrictName,
                CityId = d.CityId
            })
            .OrderBy(d => d.CityId)
            .ThenBy(d => d.DistrictName)
            .AsNoTracking()
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
        
        // 取得照片 URL
        string? photoUrl = null;
        if (item.PhotoBlobId.HasValue)
        {
            try
            {
                photoUrl = await _blobService.GetFileUrlAsync(item.PhotoBlobId.Value);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"取得照片 URL 失敗 (BlobId: {item.PhotoBlobId.Value}): {ex.Message}");
            }
        }
        ViewBag.PhotoUrl = photoUrl;
        ViewBag.PhotoBlobId = item.PhotoBlobId;
        
        // 建立 CaseFormViewModel
        var viewModel = new CaseFormViewModel
        {
            Mode = CaseFormMode.Review,
            Case = item,
            Cities = cities,
            Schools = schools,
            GenderOptions = genderOptions,
            Districts = new List<District>()
        };
        
        return View("~/Views/Case/Basic/Review/Item.cshtml", viewModel);
    }

    /// <summary>
    /// 審核決策（直接更新 Cases 表狀態）
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

        var caseItem = await _context.Cases.FindAsync(caseId);
        if (caseItem == null)
        {
            TempData["ErrorMessage"] = "找不到指定的個案";
            return RedirectToAction(nameof(Review));
        }

        // 檢查狀態是否為待審閱
        if (caseItem.Status != "PendingReview")
        {
            TempData["ErrorMessage"] = "此個案不是待審閱狀態";
            return RedirectToAction(nameof(Review));
        }

        var reviewer = User.Identity?.Name ?? string.Empty;
        
        // 直接更新 Cases 表狀態
        caseItem.Status = approved ? "Approved" : "Rejected";
        caseItem.ReviewedBy = reviewer;
        caseItem.ReviewedAt = DateTimeExtensions.TaiwanTime;
        caseItem.UpdatedAt = DateTimeExtensions.TaiwanTime;
        
        // 如果拒絕，清除提交時間
        if (!approved)
        {
            caseItem.SubmittedAt = null;
        }

        _context.Update(caseItem);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = approved ? "審核通過" : "已退回";
        return RedirectToAction(nameof(Review));
    }

    /// <summary>
    /// 審閱用基本資料更新（僅更新 Case 基本欄位）
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateBasic(CanLove_Backend.Domain.Case.Models.Basic.Case form)
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
        item.UpdatedAt = DateTimeExtensions.TaiwanTime;

        _context.Update(item);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "已儲存基本資料";
        return RedirectToAction(nameof(Review), new { caseId = form.CaseId });
    }

    private bool CaseExists(string id)
    {
        return _context.Cases.Any(e => e.CaseId == id);
    }
}
