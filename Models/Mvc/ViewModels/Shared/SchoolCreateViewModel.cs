using System.ComponentModel.DataAnnotations;

namespace CanLove_Backend.Models.Mvc.ViewModels.Shared
{
    /// <summary>
    /// 建立學校視圖模型
    /// </summary>
    public class SchoolCreateViewModel
    {
        [Required(ErrorMessage = "學校名稱為必填")]
        [Display(Name = "學校名稱")]
        [StringLength(100, ErrorMessage = "學校名稱不能超過100個字元")]
        public string SchoolName { get; set; } = string.Empty;

        [Required(ErrorMessage = "學校類型為必填")]
        [Display(Name = "學校類型")]
        public string SchoolType { get; set; } = string.Empty;

        public List<string> AvailableSchoolTypes => new List<string>
        {
            "Elementary",
            "JuniorHigh", 
            "HighSchool"
        };
    }
}
