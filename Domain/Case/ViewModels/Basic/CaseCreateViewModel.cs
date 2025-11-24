using CanLove_Backend.Domain.Case.Models.Basic;
using CanLove_Backend.Domain.Case.Shared.Models;
using CanLove_Backend.Infrastructure.Options.Models;
using Microsoft.AspNetCore.Http;

namespace CanLove_Backend.Domain.Case.ViewModels.Basic;

    /// <summary>
    /// 個案建立頁面的 ViewModel
    /// </summary>
    public class CaseCreateViewModel
    {
        /// <summary>
        /// 個案資料
        /// </summary>
        public CanLove_Backend.Domain.Case.Models.Basic.Case Case { get; set; } = new CanLove_Backend.Domain.Case.Models.Basic.Case();

        /// <summary>
        /// 個案照片檔案
        /// </summary>
        public IFormFile? PhotoFile { get; set; }

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
        public CanLove_Backend.Domain.Case.Models.Basic.Case Case { get; set; } = new CanLove_Backend.Domain.Case.Models.Basic.Case();

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
