using System.ComponentModel.DataAnnotations;
using CanLove_Backend.Models.Mvc.ViewModels;

namespace CanLove_Backend.Models.Mvc.ViewModels.CaseWizardOpenCase
{
    /// <summary>
    /// 步驟4: 健康狀況評估視圖模型 (CaseHQhealthStatus 表格)
    /// </summary>
    public class CaseWizard_S4_CHQHS_ViewModel : ICaseWizardViewModel
    {
        [Required]
        public string CaseId { get; set; } = string.Empty;

        public CaseFormMode Mode { get; set; } = CaseFormMode.Create;

        public int CurrentStep { get; set; } = 4;

        public string? SubmitAction { get; set; }

        [Display(Name = "照顧者編號")]
        public int CaregiverId { get; set; }

        [Required(ErrorMessage = "照顧者角色為必填")]
        [Display(Name = "照顧者角色")]
        public int CaregiverRoleValueId { get; set; }

        [Display(Name = "照顧者姓名")]
        [StringLength(50, ErrorMessage = "照顧者姓名不能超過50個字元")]
        public string? CaregiverName { get; set; }

        [Display(Name = "是否主要照顧者")]
        public bool? IsPrimary { get; set; }

        [Display(Name = "情緒表現分數")]
        [Range(1, 4, ErrorMessage = "情緒表現分數必須在1-4之間")]
        public byte? EmotionalExpressionRating { get; set; }

        [Display(Name = "情緒表現說明")]
        [StringLength(50, ErrorMessage = "情緒表現說明不能超過50個字元")]
        public string? EmotionalExpressionNote { get; set; }

        [Display(Name = "身體健康分數")]
        [Range(1, 4, ErrorMessage = "身體健康分數必須在1-4之間")]
        public byte? HealthStatusRating { get; set; }

        [Display(Name = "身體健康說明")]
        [StringLength(50, ErrorMessage = "身體健康說明不能超過50個字元")]
        public string? HealthStatusNote { get; set; }

        [Display(Name = "兒少健康狀態分數")]
        [Range(1, 4, ErrorMessage = "兒少健康狀態分數必須在1-4之間")]
        public byte? ChildHealthStatusRating { get; set; }

        [Display(Name = "兒少健康狀態說明")]
        [StringLength(50, ErrorMessage = "兒少健康狀態說明不能超過50個字元")]
        public string? ChildHealthStatusNote { get; set; }

        [Display(Name = "兒少受照顧狀態分數")]
        [Range(1, 4, ErrorMessage = "兒少受照顧狀態分數必須在1-4之間")]
        public byte? ChildCareStatusRating { get; set; }

        [Display(Name = "兒少受照顧狀態說明")]
        [StringLength(50, ErrorMessage = "兒少受照顧狀態說明不能超過50個字元")]
        public string? ChildCareStatusNote { get; set; }

        // 選項資料
        public List<CanLove_Backend.Data.Models.Options.OptionSetValue> CaregiverRoleOptions { get; set; } = new List<CanLove_Backend.Data.Models.Options.OptionSetValue>();
    }
}
