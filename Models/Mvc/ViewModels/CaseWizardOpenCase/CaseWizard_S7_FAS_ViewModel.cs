using System.ComponentModel.DataAnnotations;
using CanLove_Backend.Models.Mvc.ViewModels;

namespace CanLove_Backend.Models.Mvc.ViewModels.CaseWizardOpenCase
{
    /// <summary>
    /// 步驟7: 最後評估表視圖模型 (FinalAssessmentSummary 表格)
    /// </summary>
    public class CaseWizard_S7_FAS_ViewModel : ICaseWizardViewModel
    {
        [Required]
        public string CaseId { get; set; } = string.Empty;

        public CaseFormMode Mode { get; set; } = CaseFormMode.Create;

        public int CurrentStep { get; set; } = 7;

        public string? SubmitAction { get; set; }

        [Display(Name = "FQ 簡述評估")]
        [DataType(DataType.MultilineText)]
        public string? FqSummary { get; set; }

        [Display(Name = "HQ 簡述評估")]
        [DataType(DataType.MultilineText)]
        public string? HqSummary { get; set; }

        [Display(Name = "IQ 簡述評估")]
        [DataType(DataType.MultilineText)]
        public string? IqSummary { get; set; }

        [Display(Name = "EQ 簡述評估")]
        [DataType(DataType.MultilineText)]
        public string? EqSummary { get; set; }
    }
}
