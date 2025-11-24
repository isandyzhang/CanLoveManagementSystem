using Microsoft.AspNetCore.Mvc;

namespace CanLove_Backend.Application.ViewComponents;

/// <summary>
/// 選擇個案 ViewComponent
/// 用於新增開案、訪視、會談記錄前選擇個案
/// </summary>
public class SelectCaseViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(
        string targetController,
        string targetAction = "Create",
        string targetTab = "",
        string targetStep = "",
        string mode = "Create",
        bool autoLoad = false)
    {
        // 根據 targetController 自動判斷顏色主題、圖示和標題（3b 方案）
        var theme = GetTheme(targetController);
        
        var model = new SelectCaseViewModel
        {
            TargetController = targetController,
            TargetAction = targetAction,
            TargetTab = targetTab,
            TargetStep = targetStep,
            Mode = mode,
            AutoLoad = autoLoad,
            BorderColor = theme.borderColor,
            BackgroundColor = theme.bgColor,
            Icon = theme.icon,
            Title = theme.title
        };
        
        return View(model);
    }
    
    /// <summary>
    /// 根據 Controller 名稱自動判斷顏色主題
    /// </summary>
    private (string borderColor, string bgColor, string icon, string title) GetTheme(string controller)
    {
        return controller switch
        {
            "CaseOpening" => ("border-orange-400", "bg-orange-50", "bi-card-text", "開案紀錄表"),
            "CaseCareVisit" => ("border-pink-400", "bg-pink-50", "bi-heart", "關懷訪視記錄表"),
            "CaseConsultation" => ("border-blue-400", "bg-blue-50", "bi-chat-dots", "會談服務紀錄表"),
            _ => ("border-slate-400", "bg-slate-50", "bi-person", "選擇個案")
        };
    }
}

/// <summary>
/// SelectCase ViewComponent 的 ViewModel
/// </summary>
public class SelectCaseViewModel
{
    public string TargetController { get; set; } = string.Empty;
    public string TargetAction { get; set; } = "Create";
    public string TargetTab { get; set; } = string.Empty;
    public string TargetStep { get; set; } = string.Empty;
    public string Mode { get; set; } = "Create";
    public bool AutoLoad { get; set; } = false;
    public string BorderColor { get; set; } = "border-slate-400";
    public string BackgroundColor { get; set; } = "bg-slate-50";
    public string Icon { get; set; } = "bi-person";
    public string Title { get; set; } = "選擇個案";
}

