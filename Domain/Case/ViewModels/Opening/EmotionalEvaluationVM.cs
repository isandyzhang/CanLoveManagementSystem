using System.ComponentModel.DataAnnotations;
using CanLove_Backend.Domain.Case.ViewModels.Basic;
using CanLove_Backend.Domain.Case.ViewModels.Opening;

namespace CanLove_Backend.Domain.Case.ViewModels.Opening
{
    /// <summary>
    /// 步驟6: 情緒評估視圖模型 (CaseEQemotionalEvaluation 表格)
    /// </summary>
    public class EmotionalEvaluationVM : ICaseWizardVM
    {
        [Required]
        public string CaseId { get; set; } = string.Empty;

        public CaseFormMode Mode { get; set; } = CaseFormMode.Create;

        public int CurrentStep { get; set; } = 6;

        public string? SubmitAction { get; set; }

        [Display(Name = "孩子的情緒大部分是穩定的，遇到不順心的事情或陌生環境時，孩子能隨著年紀增加逐漸適應的能力")]
        [Range(1, 3, ErrorMessage = "EQ問題1評分必須在1-3之間")]
        public int? EqQ1 { get; set; }

        [Display(Name = "孩子自己有處理情緒的方式")]
        [Range(1, 3, ErrorMessage = "EQ問題2評分必須在1-3之間")]
        public int? EqQ2 { get; set; }

        [Display(Name = "孩子大多數時候可以制度控制自己的負面情緒")]
        [Range(1, 3, ErrorMessage = "EQ問題3評分必須在1-3之間")]
        public int? EqQ3 { get; set; }

        [Display(Name = "孩子遇到挫折時，不會長時間低落抑鬱，也不會把情緒直接遷怒到不相干的人身上")]
        [Range(1, 3, ErrorMessage = "EQ問題4評分必須在1-3之間")]
        public int? EqQ4 { get; set; }

        [Display(Name = "孩子對引發情緒的原因可以理解並說明")]
        [Range(1, 3, ErrorMessage = "EQ問題5評分必須在1-3之間")]
        public int? EqQ5 { get; set; }

        [Display(Name = "和過去相比，孩子遇到挫折等負面經驗，重新打起精神需花費的時間，有減少的趨勢")]
        [Range(1, 3, ErrorMessage = "EQ問題6評分必須在1-3之間")]
        public int? EqQ6 { get; set; }

        [Display(Name = "孩子對自己的情緒可以描述得很清楚")]
        [Range(1, 3, ErrorMessage = "EQ問題7評分必須在1-3之間")]
        public int? EqQ7 { get; set; }
    }
}

