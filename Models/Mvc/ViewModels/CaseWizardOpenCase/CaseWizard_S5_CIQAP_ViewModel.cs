using System.ComponentModel.DataAnnotations;

namespace CanLove_Backend.Models.Mvc.ViewModels.CaseWizardOpenCase
{
    /// <summary>
    /// 步驟5: 學業表現評估視圖模型 (CaseIQacademicPerformance 表格)
    /// </summary>
    public class CaseWizard_S5_CIQAP_ViewModel
    {
        [Required]
        public string CaseId { get; set; } = string.Empty;

        [Display(Name = "學業表現描述")]
        [StringLength(100, ErrorMessage = "學業表現描述不能超過100個字元")]
        public string? AcademicPerformanceSummary { get; set; }
    }
}
