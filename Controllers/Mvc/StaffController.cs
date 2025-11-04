using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CanLove_Backend.Services.Shared;
using CanLove_Backend.Models.Mvc.ViewModels.Staff;

namespace CanLove_Backend.Controllers.Mvc;

public class StaffController : Controller
{
    private readonly IStaffService _staffService;
    private readonly OptionService _optionService;

    public StaffController(IStaffService staffService, OptionService optionService)
    {
        _staffService = staffService;
        _optionService = optionService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
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
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var allStaff = await _staffService.GetAllForListAsync();
        var staff = allStaff.FirstOrDefault(s => s.StaffId == id);
        if (staff == null) return NotFound();

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

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(StaffEditPostModel model)
    {
        if (!ModelState.IsValid)
        {
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
        return RedirectToAction(nameof(Index));
    }
}
