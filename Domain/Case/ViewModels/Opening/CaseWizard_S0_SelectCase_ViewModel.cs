using System.ComponentModel.DataAnnotations;
using CanLove_Backend.Domain.Case.ViewModels.Basic;

namespace CanLove_Backend.Domain.Case.ViewModels.Opening
{
    /// <summary>
    /// 步驟0: 選擇個案視圖模型
    /// </summary>
    public class CaseWizard_S0_SelectCase_ViewModel : ICaseWizardViewModel
    {
        [Required(ErrorMessage = "請選擇個案")]
        [Display(Name = "個案編號")]
        public string CaseId { get; set; } = string.Empty;

        public CaseFormMode Mode { get; set; } = CaseFormMode.Create;

        public int CurrentStep { get; set; } = 0;

        public string? SubmitAction { get; set; }
    }
}


