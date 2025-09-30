using Microsoft.AspNetCore.Mvc;
using CanLove_Backend.Data.Contexts;
using CanLove_Backend.Data.Models.Core;
using CanLove_Backend.Data.Models.Options;
using CanLove_Backend.Models.Mvc.ViewModels;
using CanLove_Backend.Services.Case;
using CanLove_Backend.Services.Shared;
using Microsoft.EntityFrameworkCore;

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
    /// 個案列表頁面
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var cases = await _context.Cases
            .Include(c => c.City)
            .Include(c => c.District)
            .Include(c => c.School)
            .Where(c => c.Deleted != true)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return View(cases);
    }

    /// <summary>
    /// 個案建立頁面
    /// </summary>
    [HttpGet]
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
    public async Task<IActionResult> Create(CaseCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
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
    /// 個案刪除處理
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var caseItem = await _context.Cases.FindAsync(id);
        if (caseItem != null)
        {
            caseItem.Deleted = true;
            caseItem.DeletedAt = DateTime.UtcNow;
            caseItem.DeletedBy = User.Identity?.Name ?? "System";
            
            _context.Update(caseItem);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "個案已刪除";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// 個案詳情頁面
    /// </summary>
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

    private bool CaseExists(string id)
    {
        return _context.Cases.Any(e => e.CaseId == id);
    }
}
