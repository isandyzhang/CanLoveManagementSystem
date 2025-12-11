using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CanLove_Backend.Infrastructure.Storage.Blob;
using CanLove_Backend.Infrastructure.Storage.Encryption;
using CanLove_Backend.Infrastructure.Options.Services;
using CanLove_Backend.Domain.Case.Shared.Services;
using CanLove_Backend.Domain.Staff.Services;
using CanLove_Backend.Domain.Staff.ViewModels;
using Microsoft.Extensions.Logging;

namespace CanLove_Backend.Application.Controllers;

public class StaffController : Controller
{
    private readonly IStaffService _staffService;
    private readonly OptionService _optionService;
    private readonly ILogger<StaffController> _logger;

    public StaffController(IStaffService staffService, OptionService optionService, ILogger<StaffController> logger)
    {
        _staffService = staffService;
        _optionService = optionService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("收到員工清單查詢請求。User: {User}", User.Identity?.Name);
        
        try
        {
            var staffList = await _staffService.GetAllForListAsync();
            var deptOptions = await _optionService.GetStaffDepartmentOptionsAsync();
            var jobOptions = await _optionService.GetStaffJobTitleOptionsAsync();

            var deptMap = deptOptions.ToDictionary(o => o.ValueCode, o => o.ValueName);
            var jobMap = jobOptions.ToDictionary(o => o.ValueCode, o => o.ValueName);

            string? MapName(Dictionary<string, string> map, string? codeOrName)
            {
                if (string.IsNullOrWhiteSpace(codeOrName)) return null;
                return map.TryGetValue(codeOrName!, out var name) ? name : codeOrName; // 兼容舊資料（已是中文）
            }

            var vm = staffList.Select(s => new StaffListItemViewModel
            {
                StaffId = s.StaffId,
                DisplayName = s.DisplayName,
                DepartmentName = MapName(deptMap, s.Department),
                JobTitleName = MapName(jobMap, s.JobTitle),
                LastLoginAt = s.LastLoginAt
            }).ToList();
            
            _logger.LogInformation("成功返回員工清單。數量: {Count}, User: {User}", vm.Count, User.Identity?.Name);
            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查詢員工清單時發生錯誤。User: {User}", User.Identity?.Name);
            throw;  // 讓 ExceptionHandlingMiddleware 處理
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        _logger.LogInformation("收到員工編輯頁面請求。StaffId: {StaffId}, User: {User}", id, User.Identity?.Name);
        
        try
        {
            var allStaff = await _staffService.GetAllForListAsync();
            var staff = allStaff.FirstOrDefault(s => s.StaffId == id);
            if (staff == null)
            {
                _logger.LogWarning("找不到員工資料。StaffId: {StaffId}, User: {User}", id, User.Identity?.Name);
                return NotFound();
            }

            var departmentOptions = await _optionService.GetStaffDepartmentOptionsAsync();
            var jobTitleOptions = await _optionService.GetStaffJobTitleOptionsAsync();

            var vm = new StaffEditViewModel
            {
                StaffId = staff.StaffId,
                DisplayName = staff.DisplayName,
                DepartmentCode = staff.Department,
                JobTitleCode = staff.JobTitle,
                DepartmentName = departmentOptions.FirstOrDefault(o => o.ValueCode == staff.Department)?.ValueName ?? staff.Department,
                JobTitleName = jobTitleOptions.FirstOrDefault(o => o.ValueCode == staff.JobTitle)?.ValueName ?? staff.JobTitle,
                DepartmentOptions = departmentOptions,
                JobTitleOptions = jobTitleOptions
            };

            _logger.LogInformation("成功載入員工編輯頁面。StaffId: {StaffId}, User: {User}", id, User.Identity?.Name);
            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "載入員工編輯頁面時發生錯誤。StaffId: {StaffId}, User: {User}", id, User.Identity?.Name);
            throw;  // 讓 ExceptionHandlingMiddleware 處理
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(StaffEditPostModel model)
    {
        _logger.LogInformation("收到員工更新請求。StaffId: {StaffId}, Department: {Department}, JobTitle: {JobTitle}, User: {User}", 
            model.StaffId, model.DepartmentCode, model.JobTitleCode, User.Identity?.Name);
        
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("模型驗證失敗。StaffId: {StaffId}, User: {User}", model.StaffId, User.Identity?.Name);
                var departmentOptions = await _optionService.GetStaffDepartmentOptionsAsync();
                var jobTitleOptions = await _optionService.GetStaffJobTitleOptionsAsync();
                var vm = new StaffEditViewModel
                {
                    StaffId = model.StaffId,
                    DisplayName = model.DisplayName,
                    DepartmentCode = model.DepartmentCode,
                    JobTitleCode = model.JobTitleCode,
                    DepartmentOptions = departmentOptions,
                    JobTitleOptions = jobTitleOptions
                };
                return View(vm);
            }

            await _staffService.UpdateDepartmentAndJobTitleAsync(model.StaffId, model.DepartmentCode, model.JobTitleCode);
            _logger.LogInformation("成功更新員工資料。StaffId: {StaffId}, User: {User}", model.StaffId, User.Identity?.Name);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新員工資料時發生錯誤。StaffId: {StaffId}, User: {User}", model.StaffId, User.Identity?.Name);
            throw;  // 讓 ExceptionHandlingMiddleware 處理
        }
    }
}
