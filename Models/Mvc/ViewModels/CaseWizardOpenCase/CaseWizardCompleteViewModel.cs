namespace CanLove_Backend.Models.Mvc.ViewModels.CaseWizardOpenCase
{
    /// <summary>
    /// 步驟表單完成視圖模型
    /// </summary>
    public class CaseWizardCompleteViewModel
    {
        public string CaseId { get; set; } = string.Empty;
        public string CaseName { get; set; } = string.Empty;
        public DateTime CompletedAt { get; set; }
        public string? Message { get; set; }
    }
}
