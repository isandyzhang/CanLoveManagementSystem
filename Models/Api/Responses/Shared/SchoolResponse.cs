namespace CanLove_Backend.Models.Api.Responses.Shared
{
    /// <summary>
    /// 學校響應模型
    /// </summary>
    public class SchoolResponse
    {
        /// <summary>
        /// 學校編號
        /// </summary>
        public int SchoolId { get; set; }

        /// <summary>
        /// 學校名稱
        /// </summary>
        public string SchoolName { get; set; } = string.Empty;

        /// <summary>
        /// 學校類型
        /// </summary>
        public string SchoolType { get; set; } = string.Empty;

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 是否被使用中
        /// </summary>
        public bool IsInUse { get; set; }
    }

    /// <summary>
    /// 學校列表響應模型
    /// </summary>
    public class SchoolListResponse
    {
        /// <summary>
        /// 學校列表
        /// </summary>
        public List<SchoolResponse> Schools { get; set; } = new List<SchoolResponse>();

        /// <summary>
        /// 總數量
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 分頁資訊
        /// </summary>
        public PaginationInfo? Pagination { get; set; }
    }

    /// <summary>
    /// 分頁資訊
    /// </summary>
    public class PaginationInfo
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }
}
