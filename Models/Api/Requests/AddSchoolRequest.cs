namespace CanLove_Backend.Models.Api.Requests
{
    /// <summary>
    /// 新增學校請求模型
    /// </summary>
    public class AddSchoolRequest
    {
        /// <summary>
        /// 學校名稱
        /// </summary>
        public string SchoolName { get; set; } = string.Empty;

        /// <summary>
        /// 學校類型
        /// </summary>
        public string SchoolType { get; set; } = string.Empty;
    }
}
