using Microsoft.AspNetCore.Mvc;
using CanLove_Backend.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CanLove_Backend.Application.Controllers.Case;

/// <summary>
/// 會談服務記錄表控制器（預留，功能未開發）
/// </summary>
public class CaseConsultationController : Controller
{
    private readonly CanLoveDbContext _context;

    public CaseConsultationController(CanLoveDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 新增會談記錄（預留，功能未開發）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create(string? caseId = null)
    {
        ViewData["Sidebar.ConsultationRecord"] = "會談服務紀錄表";
        ViewData["Title"] = "會談服務紀錄表";
        ViewBag.CurrentPage = "Create";
        ViewBag.CurrentTab = "Consultation";
        ViewBag.CaseId = caseId;
        
        // 如果有 caseId，載入個案基本資訊
        if (!string.IsNullOrEmpty(caseId))
        {
            var caseInfo = await _context.Cases
                .Include(c => c.City)
                .Include(c => c.District)
                .Include(c => c.School)
                .FirstOrDefaultAsync(c => c.CaseId == caseId && c.Deleted != true);
            
            ViewBag.CaseInfo = caseInfo;
        }
        
        return View("~/Views/Case/Consultation/Consultation/ConsultationRecord.cshtml");
    }

    /// <summary>
    /// 新增會談記錄 - POST（預留，功能未開發）
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create()
    {
        // 功能未開發
        TempData["InfoMessage"] = "此功能尚未開發";
        return RedirectToAction(nameof(Create));
    }

    /// <summary>
    /// 查詢會談記錄（預留，功能未開發）
    /// </summary>
    [HttpGet]
    public IActionResult Query(string? caseId = null)
    {
        ViewData["Title"] = "查詢個案 - 會談服務紀錄表";
        ViewBag.CurrentPage = "Search";
        ViewBag.CurrentTab = "Consultation";
        return View("~/Views/Case/Consultation/Consultation/SearchConsultation.cshtml");
    }

    /// <summary>
    /// 審核會談記錄（預留，功能未開發）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Review(string? caseId = null)
    {
        ViewData["Title"] = "個案審核 - 會談服務紀錄表";
        ViewBag.CurrentPage = "Review";
        ViewBag.CurrentTab = "Consultation";
        
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
        
        // 功能未開發，返回空列表
        var emptyList = new List<object>();
        return View("~/Views/Case/Consultation/Review.cshtml", emptyList);
    }

    /// <summary>
    /// 審核會談記錄詳情（預留，功能未開發）
    /// </summary>
    [HttpGet]
    public IActionResult ReviewItem(string caseId)
    {
        ViewData["Title"] = "個案審核 - 會談服務紀錄表";
        ViewBag.CaseId = caseId;
        return View("~/Views/Case/Consultation/ReviewItem.cshtml");
    }

    /// <summary>
    /// 審核決策（預留，功能未開發）
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ReviewItemDecision(string caseId, bool approved, string? reviewComment = null)
    {
        // 功能未開發
        TempData["InfoMessage"] = "此功能尚未開發";
        return RedirectToAction(nameof(Review));
    }
}

