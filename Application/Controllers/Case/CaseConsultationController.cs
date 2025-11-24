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
    /// 選擇個案（用於新增會談記錄）
    /// </summary>
    [HttpGet]
    public IActionResult SelectCase()
    {
        ViewData["Sidebar.ConsultationRecord"] = "會談服務紀錄表";
        ViewData["Title"] = "選擇個案";
        ViewData["Breadcrumbs"] = new List<(string Text, string Url)>
        {
            ("個案管理", Url.Action("Query", "CaseBasic") ?? string.Empty),
            ("新增個案", Url.Action("Create", "CaseBasic") ?? string.Empty),
            ("會談服務紀錄表", string.Empty)
        };
        
        ViewBag.TargetController = "CaseConsultation";
        ViewBag.TargetAction = "Create";
        ViewBag.TargetTab = "Consultation";
        ViewBag.TargetStep = "Consultation";
        ViewBag.Mode = "Create";
        ViewBag.AutoLoad = false;
        
        return View("~/Views/Shared/SelectCase.cshtml");
    }

    /// <summary>
    /// 新增會談記錄（預留，功能未開發）
    /// </summary>
    [HttpGet]
    public IActionResult Create(string caseId)
    {
        ViewData["Title"] = "會談服務紀錄表";
        ViewBag.CurrentPage = "Create";
        ViewBag.CurrentTab = "Consultation";
        ViewBag.CaseId = caseId;
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
        return RedirectToAction(nameof(SelectCase));
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
    public IActionResult Review(string? caseId = null)
    {
        ViewData["Title"] = "個案審核 - 會談服務紀錄表";
        ViewBag.CurrentPage = "Review";
        ViewBag.CurrentTab = "Consultation";
        
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

