namespace CanLove_Backend.Models.Api.Responses.Shared
{
    /// <summary>
    /// 區域響應模型
    /// </summary>
    public class DistrictResponse
    {
        public int DistrictId { get; set; }
        public string DistrictName { get; set; } = string.Empty;
        public int CityId { get; set; }
    }
}
