using System.ComponentModel.DataAnnotations;
using CanLove_Backend.Domain.Case.ViewModels.Basic;
using CanLove_Backend.Domain.Case.ViewModels.Opening;

namespace CanLove_Backend.Domain.Case.ViewModels.Opening
{
    /// <summary>
    /// 步驟1: 個案詳細資料視圖模型 (CaseDetail 表格)
    /// </summary>
    public class CaseWizard_S1_CD_ViewModel : ICaseWizardViewModel
    {
        [Required]
        public string CaseId { get; set; } = string.Empty;

        public CaseFormMode Mode { get; set; } = CaseFormMode.Create;

        public int CurrentStep { get; set; } = 1;

        public string? SubmitAction { get; set; }

        [Display(Name = "聯絡人姓名")]
        [StringLength(20, ErrorMessage = "聯絡人姓名不能超過20個字元")]
        public string? ContactName { get; set; }

        [Display(Name = "與案主關係")]
        public int? ContactRelationValueId { get; set; }

        [Display(Name = "聯絡人電話")]
        [StringLength(15, ErrorMessage = "聯絡人電話不能超過15個字元")]
        [Phone]
        public string? ContactPhone { get; set; }

        [Display(Name = "住家電話")]
        [StringLength(15, ErrorMessage = "住家電話不能超過15個字元")]
        [Phone]
        public string? HomePhone { get; set; }

        [Display(Name = "家庭結構類型")]
        public int? FamilyStructureTypeId { get; set; }

        [Display(Name = "其他結構描述")]
        [StringLength(100, ErrorMessage = "其他結構描述不能超過100個字元")]
        public string? FamilyStructureOtherDesc { get; set; }

        [Display(Name = "父親國籍")]
        public int? ParentNationFatherId { get; set; }

        [Display(Name = "母親國籍")]
        public int? ParentNationMotherId { get; set; }

        [Display(Name = "主要照顧者姓名")]
        [StringLength(20, ErrorMessage = "主要照顧者姓名不能超過20個字元")]
        public string? MainCaregiverName { get; set; }

        [Display(Name = "主要照顧者與案主關係")]
        public int? MainCaregiverRelationValueId { get; set; }

        [Display(Name = "主要照顧者身分證字號(加密)")]
        [StringLength(255, ErrorMessage = "主要照顧者身分證字號不能超過255個字元")]
        public string? MainCaregiverId { get; set; }

        [Display(Name = "主要照顧者生日")]
        [DataType(DataType.Date)]
        public DateOnly? MainCaregiverBirth { get; set; }

        [Display(Name = "主要照顧者職業")]
        [StringLength(30, ErrorMessage = "主要照顧者職業不能超過30個字元")]
        public string? MainCaregiverJob { get; set; }

        [Display(Name = "主要照顧者婚姻狀況")]
        public int? MainCaregiverMarryStatusValueId { get; set; }

        [Display(Name = "主要照顧者教育程度")]
        public int? MainCaregiverEduValueId { get; set; }

        [Display(Name = "個案來源")]
        public int? SourceValueId { get; set; }

        [Display(Name = "求助經驗")]
        public int? HelpExperienceValueId { get; set; }

        [Display(Name = "備註")]
        [StringLength(1000, ErrorMessage = "備註不能超過1000個字元")]
        [DataType(DataType.MultilineText)]
        public string? Note { get; set; }

        // 選項資料
        public List<CanLove_Backend.Infrastructure.Options.Models.OptionSetValue> ContactRelationOptions { get; set; } = new List<CanLove_Backend.Infrastructure.Options.Models.OptionSetValue>();
        public List<CanLove_Backend.Infrastructure.Options.Models.OptionSetValue> MainCaregiverRelationOptions { get; set; } = new List<CanLove_Backend.Infrastructure.Options.Models.OptionSetValue>();
        public List<CanLove_Backend.Infrastructure.Options.Models.FamilyStructureType> FamilyStructureTypeOptions { get; set; } = new List<CanLove_Backend.Infrastructure.Options.Models.FamilyStructureType>();
        public List<CanLove_Backend.Infrastructure.Options.Models.Nationality> NationalityOptions { get; set; } = new List<CanLove_Backend.Infrastructure.Options.Models.Nationality>();
        public List<CanLove_Backend.Infrastructure.Options.Models.OptionSetValue> MarryStatusOptions { get; set; } = new List<CanLove_Backend.Infrastructure.Options.Models.OptionSetValue>();
        public List<CanLove_Backend.Infrastructure.Options.Models.OptionSetValue> EducationLevelOptions { get; set; } = new List<CanLove_Backend.Infrastructure.Options.Models.OptionSetValue>();
        public List<CanLove_Backend.Infrastructure.Options.Models.OptionSetValue> SourceOptions { get; set; } = new List<CanLove_Backend.Infrastructure.Options.Models.OptionSetValue>();
        public List<CanLove_Backend.Infrastructure.Options.Models.OptionSetValue> HelpExperienceOptions { get; set; } = new List<CanLove_Backend.Infrastructure.Options.Models.OptionSetValue>();
    }
}
