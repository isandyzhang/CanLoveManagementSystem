using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CanLove_Backend.Infrastructure.Data.Contexts;
using CanLove_Backend.Domain.Case.Models.Basic;
using CanLove_Backend.Application.ViewModels.Case.Basic;
using CanLove_Backend.Domain.Case.Services.Basic;
using CanLove_Backend.Domain.Case.Shared.Services;
using CanLove_Backend.Domain.Case.Exceptions;
using CanLove_Backend.Infrastructure.Storage.Encryption;
using CanLove_Backend.Core.Extensions;
using CaseEntity = CanLove_Backend.Domain.Case.Models.Basic.Case;

namespace CanLove_Backend.Application.Controllers.Case;

/// <summary>
/// 個案基本資料審核控制器：專責處理審核相關功能
/// </summary>
public class CaseBasicReviewController : CaseBasicBaseController
{
    private readonly CanLoveDbContext _context;
    private readonly ICaseBasicService _caseService;
    private readonly CaseBasicPhotoService _photoService;
    private readonly DataEncryptionService _encryptionService;

    public CaseBasicReviewController(
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
    /// 個案審核頁面（個案基本資料）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Review(string? caseId)
    {
        ViewData["Title"] = "個案審核 - 個案基本資料";
        ViewBag.CurrentPage = "Review";
        ViewBag.CurrentTab = "CaseBasic";
        SetNavigationContext("Review");
        
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
        var validationResult = ValidateCaseId(id);
        if (validationResult != null)
        {
            return validationResult;
        }

        try
        {
            var caseItem = await _caseService.GetCaseForReviewAsync(id);
            if (caseItem == null)
            {
                TempData["ErrorMessage"] = "找不到指定的待審個案";
                return RedirectToAction(nameof(Review));
            }
            
            // 解密身分證字號
            if (!string.IsNullOrWhiteSpace(caseItem.IdNumber))
            {
                caseItem.IdNumber = _encryptionService.DecryptSafely(caseItem.IdNumber);
            }

            // 載入選項資料
            var optionsData = await LoadOptionsDataAsync();
            
            // 取得照片 URL
            ViewBag.PhotoUrl = await _photoService.GetPhotoUrlAsync(caseItem.PhotoBlobId);
            ViewBag.PhotoBlobId = caseItem.PhotoBlobId;
            
            // 建立 CaseFormVM
            var viewModel = new CaseFormVM
            {
                Mode = CaseFormMode.Review,
                Case = caseItem,
                Cities = optionsData.Cities,
                Schools = optionsData.Schools,
                GenderOptions = optionsData.GenderOptions,
                Districts = new List<Domain.Case.Shared.Models.District>()
            };
            
            SetNavigationContext("Review");
            return View("~/Views/Case/Basic/Review/Item.cshtml", viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"載入個案資料時發生錯誤：{ex.Message}";
            return RedirectToAction(nameof(Review));
        }
    }

    /// <summary>
    /// 審核決策（直接更新 Cases 表狀態）
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReviewItemDecision(string caseId, bool approved, string? reviewComment = null)
    {
        try
        {
            var result = await _caseService.ReviewCaseAsync(caseId, approved, User.Identity?.Name, reviewComment);
            
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }
            
            return RedirectToAction(nameof(Review));
        }
        catch (CaseBasicException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Review));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"審核決策時發生錯誤：{ex.Message}";
            return RedirectToAction(nameof(Review));
        }
    }

    /// <summary>
    /// 審核個案（從其他頁面調用）
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize] // TODO: 之後可根據需求改為 [Authorize(Policy = "RequireSocialWorker")]
    public async Task<IActionResult> ReviewCase(string id, bool approved, string? reviewComment = null)
    {
        try
        {
            var result = await _caseService.ReviewCaseAsync(id, approved, User.Identity?.Name, reviewComment);
            
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }
            
            return RedirectToAction("Query", "CaseBasicQuery");
        }
        catch (CaseBasicException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Query", "CaseBasicQuery");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"審核個案時發生錯誤：{ex.Message}";
            return RedirectToAction("Query", "CaseBasicQuery");
        }
    }

    /// <summary>
    /// 審閱用基本資料更新（僅更新 Case 基本欄位）
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateBasic(CaseEntity form)
    {
        if (string.IsNullOrWhiteSpace(form.CaseId)) 
        {
            return View("NotFound");
        }

        var item = await _context.Cases.FindAsync(form.CaseId);
        if (item == null) 
        {
            return View("NotFound");
        }

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
}
