using Microsoft.AspNetCore.Mvc;

namespace CanLove_Backend.Application.Controllers;

/// <summary>
/// 考勤管理控制器
/// </summary>
public class AttendanceController : Controller
{
    /// <summary>
    /// 考勤記錄
    /// </summary>
    [HttpGet]
    public IActionResult Record()
    {
        ViewData["Title"] = "考勤記錄";
        return View();
    }

    /// <summary>
    /// 請假申請
    /// </summary>
    [HttpGet]
    public IActionResult LeaveRequest()
    {
        ViewData["Title"] = "請假申請";
        return View();
    }
}


