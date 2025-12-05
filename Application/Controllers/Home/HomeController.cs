using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanLove_Backend.Application.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult About()
    {
        ViewData["Message"] = "CanLove 個案管理系統";
        return View();
    }

    public IActionResult Contact()
    {
        ViewData["Message"] = "聯絡資訊";
        return View();
    }

    /// <summary>
    /// 錯誤頁面
    /// </summary>
    [AllowAnonymous]
    public IActionResult Error()
    {
        var statusCode = Request.Query["statusCode"].ToString();
        ViewData["StatusCode"] = int.TryParse(statusCode, out var code) ? code : 500;
        return View();
    }
}
