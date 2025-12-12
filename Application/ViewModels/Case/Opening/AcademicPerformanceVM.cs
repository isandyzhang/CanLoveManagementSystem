using System.ComponentModel.DataAnnotations;
using CanLove_Backend.Application.ViewModels.Case.Basic;

namespace CanLove_Backend.Application.ViewModels.Case.Opening
{
    /// <summary>
    /// 步驟5: 學業表現評估視圖模型 (CaseIQacademicPerformance 表格)
    /// </summary>
    public class AcademicPerformanceVM : ICaseWizardVM
    {
        [Required]
        public string CaseId { get; set; } = string.Empty;

        public CaseFormMode Mode { get; set; } = CaseFormMode.Create;

        public int CurrentStep { get; set; } = 5;

        public string? SubmitAction { get; set; }

        [Display(Name = "學業表現描述")]
        [StringLength(100, ErrorMessage = "學業表現描述不能超過100個字元")]
        public string? AcademicPerformanceSummary { get; set; }
    }
}

