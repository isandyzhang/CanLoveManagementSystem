using Microsoft.AspNetCore.Mvc;
using CanLove_Backend.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CanLove_Backend.Application.Controllers.Case;

/// <summary>
/// 關懷訪視記錄表控制器（預留，功能未開發）
/// </summary>
public class CaseCareVisitController : Controller
{
    private readonly CanLoveDbContext _context;

    public CaseCareVisitController(CanLoveDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 選擇個案（用於新增訪視記錄）
    /// </summary>
    [HttpGet]
    public IActionResult SelectCase()
    {
        ViewData["Sidebar.CareVisitRecord"] = "關懷訪視記錄表";
        ViewData["Title"] = "選擇個案";
        ViewData["Breadcrumbs"] = new List<(string Text, string Url)>
        {
            ("個案管理", Url.Action("Query", "CaseBasic") ?? string.Empty),
            ("新增個案", Url.Action("Create", "CaseBasic") ?? string.Empty),
            ("關懷訪視記錄表", string.Empty)
        };
        
        ViewBag.TargetController = "CaseCareVisit";
        ViewBag.TargetAction = "Create";
        ViewBag.TargetTab = "CareVisitRecord";
        ViewBag.TargetStep = "CareVisit";
        ViewBag.Mode = "Create";
        ViewBag.AutoLoad = false;
        
        return View("~/Views/Shared/SelectCase.cshtml");
    }

    /// <summary>
    /// 新增訪視記錄（預留，功能未開發）
    /// </summary>
    [HttpGet]
    public IActionResult Create(string caseId)
    {
        ViewData["Title"] = "關懷訪視記錄表";
        ViewBag.CurrentPage = "Create";
        ViewBag.CurrentTab = "CareVisitRecord";
        ViewBag.CaseId = caseId;
        return View("~/Views/Case/CareVisitRecord/CareVisitRecord/CareVisitRecord.cshtml");
    }

    /// <summary>
    /// 新增訪視記錄 - POST（預留，功能未開發）
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create()
    {
        // 功能未開發
        TempData["InfoMessage"] = "此功能尚未開發";
        return RedirectToAction(nameof(SelectCase));
    }

    /// <summary>
    /// 查詢訪視記錄（預留，功能未開發）
    /// </summary>
    [HttpGet]
    public IActionResult Query(string? caseId = null)
    {
        ViewData["Title"] = "查詢個案 - 關懷訪視記錄表";
        ViewBag.CurrentPage = "Search";
        ViewBag.CurrentTab = "CareVisitRecord";
        return View("~/Views/Case/CareVisitRecord/CareVisitRecord/SearchCareVisit.cshtml");
    }

    /// <summary>
    /// 審核訪視記錄（預留，功能未開發）
    /// </summary>
    [HttpGet]
    public IActionResult Review(string? caseId = null)
    {
        ViewData["Title"] = "個案審核 - 關懷訪視記錄表";
        ViewBag.CurrentPage = "Review";
        ViewBag.CurrentTab = "CareVisitRecord";
        
        // 功能未開發，返回空列表
        var emptyList = new List<object>();
        return View("~/Views/Case/CareVisitRecord/Review.cshtml", emptyList);
    }

    /// <summary>
    /// 審核訪視記錄詳情（預留，功能未開發）
    /// </summary>
    [HttpGet]
    public IActionResult ReviewItem(string caseId)
    {
        ViewData["Title"] = "個案審核 - 關懷訪視記錄表";
        ViewBag.CaseId = caseId;
        return View("~/Views/Case/CareVisitRecord/ReviewItem.cshtml");
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

