using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CanLove_Backend.Infrastructure.Data.Contexts;
using CanLove_Backend.Core.Extensions;

namespace CanLove_Backend.Application.Controllers.Case;

/// <summary>
/// 開案記錄管理控制器（目前僅提供刪除）
/// </summary>
public class CaseOpeningController : Controller
{
    private readonly CanLoveDbContext _context;

    public CaseOpeningController(CanLoveDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 刪除開案記錄（軟刪除）
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Delete(string caseId)
    {
        if (string.IsNullOrWhiteSpace(caseId))
        {
            return Json(new { success = false, message = "個案編號不能為空" });
        }

        try
        {
            var opening = await _context.CaseOpenings
                .Include(o => o.Case)
                .FirstOrDefaultAsync(o => o.CaseId == caseId);

            if (opening == null)
            {
                return Json(new { success = false, message = "找不到指定的開案記錄" });
            }

            opening.Deleted = true;
            opening.DeletedAt = DateTimeExtensions.TaiwanTime;
            opening.DeletedBy = User.Identity?.Name ?? "System";
            opening.UpdatedAt = DateTimeExtensions.TaiwanTime;

            _context.Update(opening);
            await _context.SaveChangesAsync();

            var caseName = opening.Case?.Name ?? string.Empty;
            var displayName = string.IsNullOrWhiteSpace(caseName) ? opening.CaseId : $"「{caseName}」({opening.CaseId})";
            return Json(new { success = true, message = $"開案記錄 {displayName} 已成功刪除" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"刪除開案記錄時發生錯誤：{ex.Message}" });
        }
    }
}

