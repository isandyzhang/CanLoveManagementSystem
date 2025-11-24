using Microsoft.AspNetCore.Mvc;

namespace CanLove_Backend.Application.ViewComponents;

/// <summary>
/// 搜尋個案 ViewComponent
/// 封裝 _SearchCase.cshtml 的邏輯，提供更好的參數化控制
/// </summary>
public class SearchCaseViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(
        string mode = "Create",
        string targetAction = "Create",
        string targetController = "CaseOpening",
        string targetStep = "CaseDetail",
        bool autoLoad = false)
    {
        var model = new SearchCaseViewModel
        {
            Mode = mode,
            TargetAction = targetAction,
            TargetController = targetController,
            TargetStep = targetStep,
            AutoLoad = autoLoad
        };
        
        return View(model);
    }
}

/// <summary>
/// SearchCase ViewComponent 的 ViewModel
/// </summary>
public class SearchCaseViewModel
{
    public string Mode { get; set; } = "Create";
    public string TargetAction { get; set; } = "Create";
    public string TargetController { get; set; } = "CaseOpening";
    public string TargetStep { get; set; } = "CaseDetail";
    public bool AutoLoad { get; set; } = false;
}

