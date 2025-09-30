using System.ComponentModel.DataAnnotations;

namespace CanLove_Backend.Models.Mvc.ViewModels.CaseWizardOpenCase
{
    /// <summary>
    /// 步驟2: 社會工作服務內容視圖模型 (CaseSocialWorkerContent 表格)
    /// </summary>
    public class CaseWizard_S2_CSWC_ViewModel
    {
        [Required]
        public string CaseId { get; set; } = string.Empty;

        [Display(Name = "家系圖圖片")]
        [StringLength(250, ErrorMessage = "家系圖圖片路徑不能超過250個字元")]
        public string? FamilyTreeImg { get; set; }

        [Display(Name = "居住地型態")]
        public int? ResidenceTypeValueId { get; set; }

        [Display(Name = "居家整潔分數")]
        [Range(1, 4, ErrorMessage = "居家整潔分數必須在1-4之間")]
        public byte? HouseCleanlinessRating { get; set; }

        [Display(Name = "居家整潔說明")]
        [StringLength(50, ErrorMessage = "居家整潔說明不能超過50個字元")]
        public string? HouseCleanlinessNote { get; set; }

        [Display(Name = "居家安全分數")]
        [Range(1, 4, ErrorMessage = "居家安全分數必須在1-4之間")]
        public byte? HouseSafetyRating { get; set; }

        [Display(Name = "居家安全說明")]
        [StringLength(50, ErrorMessage = "居家安全說明不能超過50個字元")]
        public string? HouseSafetyNote { get; set; }

        [Display(Name = "照顧者與兒少互動情形分數")]
        [Range(1, 4, ErrorMessage = "照顧者與兒少互動情形分數必須在1-4之間")]
        public byte? CaregiverChildInteractionRating { get; set; }

        [Display(Name = "照顧者與兒少互動情形說明")]
        [StringLength(50, ErrorMessage = "照顧者與兒少互動情形說明不能超過50個字元")]
        public string? CaregiverChildInteractionNote { get; set; }

        [Display(Name = "照顧者與整體同住家人互動情形分數")]
        [Range(1, 4, ErrorMessage = "照顧者與整體同住家人互動情形分數必須在1-4之間")]
        public byte? CaregiverFamilyInteractionRating { get; set; }

        [Display(Name = "照顧者與整體同住家人互動情形說明")]
        [StringLength(50, ErrorMessage = "照顧者與整體同住家人互動情形說明不能超過50個字元")]
        public string? CaregiverFamilyInteractionNote { get; set; }

        [Display(Name = "家庭資源連結能力分數")]
        [Range(1, 4, ErrorMessage = "家庭資源連結能力分數必須在1-4之間")]
        public byte? FamilyResourceAbilityRating { get; set; }

        [Display(Name = "家庭資源連結能力說明")]
        [StringLength(50, ErrorMessage = "家庭資源連結能力說明不能超過50個字元")]
        public string? FamilyResourceAbilityNote { get; set; }

        [Display(Name = "社會家庭支持獲得分數")]
        [Range(1, 4, ErrorMessage = "社會家庭支持獲得分數必須在1-4之間")]
        public byte? FamilySocialSupportRating { get; set; }

        [Display(Name = "社會家庭支持獲得說明")]
        [StringLength(50, ErrorMessage = "社會家庭支持獲得說明不能超過50個字元")]
        public string? FamilySocialSupportNote { get; set; }

        [Display(Name = "其他特殊情形描述")]
        [StringLength(50, ErrorMessage = "其他特殊情形描述不能超過50個字元")]
        public string? SpecialCircumstancesDescription { get; set; }

        // 選項資料
        public List<CanLove_Backend.Data.Models.Options.OptionSetValue> ResidenceTypeOptions { get; set; } = new List<CanLove_Backend.Data.Models.Options.OptionSetValue>();
    }
}
