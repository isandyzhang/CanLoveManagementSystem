using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
/// 個案基本資料新增和編輯控制器
/// </summary>
public class CaseBasicCreateEditController : CaseBasicBaseController
{
    private readonly ICaseBasicService _caseService;
    private readonly CaseBasicPhotoService _photoService;
    private readonly DataEncryptionService _encryptionService;

    public CaseBasicCreateEditController(
        ICaseBasicService caseService,
        CaseBasicPhotoService photoService,
        DataEncryptionService encryptionService,
        CaseBasicValidationService validationService,
        CaseBasicOptionsService optionsService,
        CaseInfoService caseInfoService)
        : base(validationService, optionsService, caseInfoService)
    {
        _caseService = caseService;
        _photoService = photoService;
        _encryptionService = encryptionService;
    }

    /// <summary>
    /// 個案建立頁面
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        SetNavigationContext("Create");
        ViewData["Sidebar.CreateCase"] = "新增個案";
        
        try
        {
            var optionsData = await LoadOptionsDataAsync();
            
            var viewModel = new CaseFormVM
            {
                Mode = CaseFormMode.Create,
                Case = new CaseEntity
                {
                    CaseId = string.Empty,
                    AssessmentDate = DateOnly.FromDateTime(DateTime.Today)
                },
                Cities = optionsData.Cities,
                Districts = new List<Domain.Case.Shared.Models.District>(),
                Schools = optionsData.Schools,
                GenderOptions = optionsData.GenderOptions
            };

            return View("~/Views/Case/Basic/Create/Item.cshtml", viewModel);
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = $"載入頁面時發生錯誤：{ex.Message}";
            return View("~/Views/Case/Basic/Create/Item.cshtml", new CaseFormVM
            {
                Mode = CaseFormMode.Create,
                Case = new CaseEntity(),
                Cities = new List<Domain.Case.Shared.Models.City>(),
                Districts = new List<Domain.Case.Shared.Models.District>(),
                Schools = new List<Domain.Case.Shared.Models.School>(),
                GenderOptions = new List<CanLove_Backend.Infrastructure.Options.Models.OptionSetValue>()
            });
        }
    }

    /// <summary>
    /// 個案建立處理
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize] // TODO: 之後可根據需求改為 [Authorize(Policy = "RequireAssistant")]
    public async Task<IActionResult> Create(CaseFormVM model)
    {
        if (ModelState.IsValid)
        {
            // 處理照片上傳
            if (model.PhotoFile != null && model.PhotoFile.Length > 0)
            {
                var validationError = _photoService.ValidatePhotoFile(model.PhotoFile);
                if (validationError != null)
                {
                    ModelState.AddModelError("PhotoFile", validationError);
                }
                else
                {
                    var (success, blobId, errorMessage) = await _photoService.UploadPhotoAsync(
                        model.PhotoFile, 
                        model.Case.CaseId, 
                        User.Identity?.Name);
                    
                    if (success && blobId.HasValue)
                    {
                        model.Case.PhotoBlobId = blobId;
                    }
                    else
                    {
                        ModelState.AddModelError("PhotoFile", errorMessage ?? "照片上傳失敗");
                    }
                }
            }

            // 如果照片上傳失敗，不繼續建立個案
            if (!ModelState.IsValid)
            {
                var optionsData = await LoadOptionsDataAsync();
                model.Cities = optionsData.Cities;
                model.Districts = new List<Domain.Case.Shared.Models.District>();
                model.Schools = optionsData.Schools;
                model.GenderOptions = optionsData.GenderOptions;
                return View("~/Views/Case/Basic/Create/Item.cshtml", model);
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
                TempData["SuccessMessage"] = "個案已送交審閱！";

                // 使用 PRG 模式重定向回 Create 頁面，確保表單完全清空
                return RedirectToAction("Create");
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
        var errorOptionsData = await LoadOptionsDataAsync();
        model.Cities = errorOptionsData.Cities;
        model.Districts = new List<Domain.Case.Shared.Models.District>(); // 初始為空，等選擇城市後載入
        model.Schools = errorOptionsData.Schools; // 載入所有學校供獨立選擇
        model.GenderOptions = errorOptionsData.GenderOptions;

        return View("~/Views/Case/Basic/Create/Item.cshtml", model);
    }

    /// <summary>
    /// 個案編輯頁面（別名：EditItem）
    /// </summary>
    [HttpGet]
    [Authorize] // TODO: 之後可根據需求改為 [Authorize(Policy = "RequireSocialWorker")]
    public async Task<IActionResult> EditItem(string id)
    {
        return await Edit(id);
    }

    /// <summary>
    /// 個案編輯頁面
    /// </summary>
    [HttpGet]
    [Authorize] // TODO: 之後可根據需求改為 [Authorize(Policy = "RequireSocialWorker")]
    public async Task<IActionResult> Edit(string id)
    {
        SetNavigationContext("Edit");
        
        var validationResult = ValidateCaseId(id);
        if (validationResult != null)
        {
            return validationResult;
        }

        try
        {
            var caseItem = await _caseService.GetCaseForEditAsync(id);
            if (caseItem == null)
            {
                return View("NotFound");
            }

            // 檢查個案是否被鎖定
            if (_validationService.IsLocked(caseItem, User.Identity?.Name))
            {
                TempData["ErrorMessage"] = $"此個案已被其他使用者鎖定，無法編輯。鎖定者：{caseItem.LockedBy}";
                return RedirectToAction("Query", "CaseBasicQuery");
            }

            // 解密身分證字號以便顯示
            if (!string.IsNullOrWhiteSpace(caseItem.IdNumber))
            {
                caseItem.IdNumber = _encryptionService.DecryptSafely(caseItem.IdNumber);
            }

            // 載入選項資料
            var optionsData = await LoadOptionsDataAsync();

            // 取得照片 URL（如果有 PhotoBlobId）
            ViewBag.PhotoUrl = await _photoService.GetPhotoUrlAsync(caseItem.PhotoBlobId);

            // 建立 CaseFormVM
            var viewModel = new CaseFormVM
            {
                Mode = CaseFormMode.Create, // 編輯模式使用 Create（可編輯）
                Case = caseItem,
                Cities = optionsData.Cities,
                Schools = optionsData.Schools,
                GenderOptions = optionsData.GenderOptions,
                Districts = new List<Domain.Case.Shared.Models.District>() // 地區資料通過 ViewBag.DistrictsByCity 傳遞
            };
            
            return View("~/Views/Case/Basic/Edit/Item.cshtml", viewModel);
        }
        catch (CaseBasicException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Query", "CaseBasicQuery");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"載入個案資料時發生錯誤：{ex.Message}";
            return RedirectToAction("Query", "CaseBasicQuery");
        }
    }

    /// <summary>
    /// 個案編輯處理
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize] // TODO: 之後可根據需求改為 [Authorize(Policy = "RequireSocialWorker")]
    public async Task<IActionResult> Edit(string id, CaseFormVM model)
    {
        if (id != model.Case.CaseId)
        {
            return View("NotFound");
        }

        if (ModelState.IsValid)
        {
            try
            {
                // 檢查個案是否存在並是否被鎖定
                var existingCase = await _caseService.GetCaseForEditAsync(id);
                if (existingCase == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的個案";
                    return RedirectToAction("Query", "CaseBasicQuery");
                }

                if (_validationService.IsLocked(existingCase, User.Identity?.Name))
                {
                    TempData["ErrorMessage"] = $"此個案已被其他使用者鎖定，無法編輯。鎖定者：{existingCase.LockedBy}";
                    return RedirectToAction("Query", "CaseBasicQuery");
                }

                // 處理照片上傳（如果有新照片）
                if (model.PhotoFile != null && model.PhotoFile.Length > 0)
                {
                    var validationError = _photoService.ValidatePhotoFile(model.PhotoFile);
                    if (validationError != null)
                    {
                        ModelState.AddModelError("PhotoFile", validationError);
                    }
                    else
                    {
                        // 如果已有舊照片，先刪除舊的 blob
                        if (existingCase.PhotoBlobId.HasValue)
                        {
                            await _photoService.DeletePhotoAsync(existingCase.PhotoBlobId.Value);
                        }

                        // 上傳新照片
                        var (success, blobId, errorMessage) = await _photoService.UploadPhotoAsync(
                            model.PhotoFile, 
                            model.Case.CaseId, 
                            User.Identity?.Name);
                        
                        if (success && blobId.HasValue)
                        {
                            model.Case.PhotoBlobId = blobId;
                        }
                        else
                        {
                            ModelState.AddModelError("PhotoFile", errorMessage ?? "照片上傳失敗");
                        }
                    }
                }
                else
                {
                    // 如果沒有上傳新照片，保留原有的 PhotoBlobId
                    model.Case.PhotoBlobId = existingCase.PhotoBlobId;
                }

                // 如果照片上傳失敗，不繼續更新個案
                if (!ModelState.IsValid)
                {
                    var errorOptionsData = await LoadOptionsDataAsync();
                    model.Cities = errorOptionsData.Cities;
                    model.Schools = errorOptionsData.Schools;
                    model.GenderOptions = errorOptionsData.GenderOptions;
                    model.Districts = new List<Domain.Case.Shared.Models.District>();
                    
                    // 取得照片 URL
                    ViewBag.PhotoUrl = await _photoService.GetPhotoUrlAsync(model.Case.PhotoBlobId);
                    
                    return View("~/Views/Case/Basic/Edit/Item.cshtml", model);
                }

                // 使用服務層更新個案
                var updateResult = await _caseService.UpdateCaseAsync(model.Case, User.Identity?.Name);
                
                if (updateResult.Success)
                {
                    TempData["SuccessMessage"] = "個案更新成功";
                    return RedirectToAction("Query", "CaseBasicQuery");
                }
                else
                {
                    ModelState.AddModelError("", updateResult.Message);
                }
            }
            catch (CaseBasicException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Query", "CaseBasicQuery");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"更新個案時發生錯誤：{ex.Message}";
                return RedirectToAction("Query", "CaseBasicQuery");
            }
        }

        // 如果驗證失敗，重新載入選項資料
        var optionsData = await LoadOptionsDataAsync();
        model.Cities = optionsData.Cities;
        model.Schools = optionsData.Schools;
        model.GenderOptions = optionsData.GenderOptions;
        model.Districts = new List<Domain.Case.Shared.Models.District>();
        
        // 取得照片 URL
        ViewBag.PhotoUrl = await _photoService.GetPhotoUrlAsync(model.Case.PhotoBlobId);
        
        return View("~/Views/Case/Basic/Edit/Item.cshtml", model);
    }
}
