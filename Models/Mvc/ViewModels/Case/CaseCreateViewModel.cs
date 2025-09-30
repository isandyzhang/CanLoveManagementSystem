using CanLove_Backend.Data.Models.Core;
using CanLove_Backend.Data.Models.Options;

namespace CanLove_Backend.Models.Mvc.ViewModels;

    /// <summary>
    /// 個案建立頁面的 ViewModel
    /// </summary>
    public class CaseCreateViewModel
    {
        /// <summary>
        /// 個案資料
        /// </summary>
        public Case Case { get; set; } = new Case();

        /// <summary>
        /// 城市選項
        /// </summary>
        public List<City> Cities { get; set; } = new List<City>();

        /// <summary>
        /// 區域選項
        /// </summary>
        public List<District> Districts { get; set; } = new List<District>();

        /// <summary>
        /// 學校選項
        /// </summary>
        public List<School> Schools { get; set; } = new List<School>();

        /// <summary>
        /// 性別選項
        /// </summary>
        public List<OptionSetValue> GenderOptions { get; set; } = new List<OptionSetValue>();
    }

    /// <summary>
    /// 個案編輯頁面的 ViewModel
    /// </summary>
    public class CaseEditViewModel
    {
        /// <summary>
        /// 個案資料
        /// </summary>
        public Case Case { get; set; } = new Case();

        /// <summary>
        /// 城市選項
        /// </summary>
        public List<City> Cities { get; set; } = new List<City>();

        /// <summary>
        /// 區域選項
        /// </summary>
        public List<District> Districts { get; set; } = new List<District>();

        /// <summary>
        /// 學校選項
        /// </summary>
        public List<School> Schools { get; set; } = new List<School>();

        /// <summary>
        /// 性別選項
        /// </summary>
        public List<OptionSetValue> GenderOptions { get; set; } = new List<OptionSetValue>();
    }
