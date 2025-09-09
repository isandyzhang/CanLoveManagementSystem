using Microsoft.AspNetCore.Mvc;
using CanLove_Backend.Data.Contexts;
using CanLove_Backend.Data.Models.Core;
using CanLove_Backend.Data.Models.Options;
using CanLove_Backend.Models.Mvc.ViewModels;
using CanLove_Backend.Services;
using CanLove_Backend.Models.Api.Responses;
using Microsoft.EntityFrameworkCore;

namespace CanLove_Backend.Controllers.Mvc;

public class CaseController : Controller
{
    private readonly CanLoveDbContext _context;
    private readonly SchoolService _schoolService;
    private readonly CaseService _caseService;

    public CaseController(CanLoveDbContext context, SchoolService schoolService, CaseService caseService)
    {
        _context = context;
        _schoolService = schoolService;
        _caseService = caseService;
    }

    // GET: Case
    public async Task<IActionResult> Index(int page = 1)
    {
        // 使用 Service 來取得資料，支援分頁
        var (cases, totalCount) = await _caseService.GetCasesForMvcAsync(page, 20);
        
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / 20);
        ViewBag.CurrentPage = page;
        
        return View(cases);
    }

    // GET: Case/Details/5
    public async Task<IActionResult> Details(string id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var caseItem = await _context.Cases
            .Include(c => c.City)
            .Include(c => c.District)
            .Include(c => c.School)
            .Include(c => c.CaseDetail)
            .FirstOrDefaultAsync(m => m.CaseId == id);

        if (caseItem == null)
        {
            return NotFound();
        }

        return View(caseItem);
    }

    // GET: Case/Create
    public async Task<IActionResult> Create()
    {
        var viewModel = new CaseCreateViewModel
        {
            Case = new Case 
            { 
                CityId = 1 // 預設為台北市
            },
            Cities = _context.Cities.OrderBy(c => c.CityId).ToList(),
            Districts = _context.Districts.OrderBy(d => d.CityId).ThenBy(d => d.DistrictName).ToList(),
            Schools = await _schoolService.GetAllSchoolsAsync(),
            GenderOptions = _context.OptionSetValues
                .Where(o => o.OptionSet.OptionKey == "GENDER")
                .OrderBy(o => o.ValueCode)
                .ToList()
        };
        
        return View(viewModel);
    }

    // POST: Case/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CaseCreateViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var caseItem = viewModel.Case;
            // 設定基本欄位
            caseItem.CaseId = Guid.NewGuid().ToString();
            caseItem.CreatedAt = DateTime.UtcNow;
            caseItem.UpdatedAt = DateTime.UtcNow;
            caseItem.DraftStatus = true; // 預設為草稿
            caseItem.Deleted = false;

            _context.Add(caseItem);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "個案新增成功！";
            return RedirectToAction(nameof(Index));
        }

        // 如果驗證失敗，重新載入下拉選單資料
        viewModel.Cities = _context.Cities.OrderBy(c => c.CityId).ToList();
        viewModel.Districts = _context.Districts.OrderBy(d => d.CityId).ThenBy(d => d.DistrictName).ToList();
        viewModel.Schools = await _schoolService.GetAllSchoolsAsync();
        viewModel.GenderOptions = _context.OptionSetValues
            .Where(o => o.OptionSet.OptionKey == "GENDER")
            .OrderBy(o => o.ValueCode)
            .ToList();
        
        return View(viewModel);
    }

    // GET: Case/Edit/5
    public async Task<IActionResult> Edit(string id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var caseItem = await _context.Cases.FindAsync(id);
        if (caseItem == null)
        {
            return NotFound();
        }

        ViewBag.Cities = _context.Cities.ToList();
        ViewBag.Districts = _context.Districts.ToList();
        ViewBag.Schools = _context.Schools.ToList();
        
        return View(caseItem);
    }

    // POST: Case/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, Case caseItem)
    {
        if (id != caseItem.CaseId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                caseItem.UpdatedAt = DateTime.UtcNow;
                _context.Update(caseItem);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "個案更新成功！";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CaseExists(caseItem.CaseId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Cities = _context.Cities.ToList();
        ViewBag.Districts = _context.Districts.ToList();
        ViewBag.Schools = _context.Schools.ToList();
        
        return View(caseItem);
    }

    // GET: Case/Delete/5
    public async Task<IActionResult> Delete(string id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var caseItem = await _context.Cases
            .Include(c => c.City)
            .Include(c => c.District)
            .Include(c => c.School)
            .FirstOrDefaultAsync(m => m.CaseId == id);

        if (caseItem == null)
        {
            return NotFound();
        }

        return View(caseItem);
    }

    // POST: Case/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var caseItem = await _context.Cases.FindAsync(id);
        if (caseItem != null)
        {
            // 軟刪除
            caseItem.Deleted = true;
            caseItem.DeletedAt = DateTime.UtcNow;
            caseItem.DeletedBy = User.Identity?.Name ?? "System";
            
            _context.Update(caseItem);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "個案已刪除！";
        }

        return RedirectToAction(nameof(Index));
    }


    private bool CaseExists(string id)
    {
        return _context.Cases.Any(e => e.CaseId == id);
    }

}
