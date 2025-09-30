using CanLove_Backend.Data.Models.Options;

namespace CanLove_Backend.Models.Mvc.ViewModels.Shared
{
    /// <summary>
    /// 學校管理頁面視圖模型
    /// </summary>
    public class SchoolIndexViewModel
    {
        /// <summary>
        /// 學校列表
        /// </summary>
        public List<School> Schools { get; set; } = new List<School>();

        /// <summary>
        /// 搜尋關鍵字
        /// </summary>
        public string? SearchKeyword { get; set; }

        /// <summary>
        /// 學校類型篩選
        /// </summary>
        public string? SchoolTypeFilter { get; set; }

        /// <summary>
        /// 可用的學校類型
        /// </summary>
        public List<string> AvailableSchoolTypes { get; set; } = new List<string>();

        /// <summary>
        /// 總數量
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 目前頁碼
        /// </summary>
        public int CurrentPage { get; set; } = 1;

        /// <summary>
        /// 每頁數量
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// 總頁數
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
