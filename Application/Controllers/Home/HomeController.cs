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
}
