using Microsoft.AspNetCore.Mvc;

namespace CanLove_Backend.Application.Controllers;

/// <summary>
/// 物資管理控制器
/// </summary>
public class SupplyController : Controller
{
    /// <summary>
    /// 物資清單
    /// </summary>
    [HttpGet]
    public IActionResult Inventory()
    {
        ViewData["Title"] = "物資清單";
        return View();
    }

    /// <summary>
    /// 物資入庫
    /// </summary>
    [HttpGet]
    public IActionResult StockIn()
    {
        ViewData["Title"] = "物資入庫";
        return View();
    }
}


