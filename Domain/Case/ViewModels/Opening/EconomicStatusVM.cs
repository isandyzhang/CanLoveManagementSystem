using System.ComponentModel.DataAnnotations;
using CanLove_Backend.Domain.Case.ViewModels.Basic;
using CanLove_Backend.Domain.Case.ViewModels.Opening;

namespace CanLove_Backend.Domain.Case.ViewModels.Opening
{
    /// <summary>
    /// 步驟3: 經濟狀況評估視圖模型 (CaseFQeconomicStatus 表格)
    /// </summary>
    public class EconomicStatusVM : ICaseWizardViewModel
    {
        [Required]
        public string CaseId { get; set; } = string.Empty;

        public CaseFormMode Mode { get; set; } = CaseFormMode.Create;

        public int CurrentStep { get; set; } = 3;

        public string? SubmitAction { get; set; }

        [Display(Name = "家庭經濟概況描述")]
        [StringLength(50, ErrorMessage = "家庭經濟概況描述不能超過50個字元")]
        public string? EconomicOverview { get; set; }

        [Display(Name = "工作情形")]
        [StringLength(50, ErrorMessage = "工作情形不能超過50個字元")]
        public string? WorkSituation { get; set; }

        [Display(Name = "民間福利資源")]
        [StringLength(50, ErrorMessage = "民間福利資源不能超過50個字元")]
        public string? CivilWelfareResources { get; set; }

        [Display(Name = "月收入")]
        [DataType(DataType.Currency)]
        [Range(0, 99999999.99, ErrorMessage = "月收入必須在0-99999999.99之間")]
        public decimal? MonthlyIncome { get; set; }

        [Display(Name = "月支出")]
        [DataType(DataType.Currency)]
        [Range(0, 99999999.99, ErrorMessage = "月支出必須在0-99999999.99之間")]
        public decimal? MonthlyExpense { get; set; }

        [Display(Name = "月支出說明")]
        [StringLength(50, ErrorMessage = "月支出說明不能超過50個字元")]
        public string? MonthlyExpenseNote { get; set; }

        [Display(Name = "描述")]
        [StringLength(50, ErrorMessage = "描述不能超過50個字元")]
        public string? Description { get; set; }
    }
}

