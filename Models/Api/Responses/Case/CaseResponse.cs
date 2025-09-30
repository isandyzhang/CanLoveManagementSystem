namespace CanLove_Backend.Models.Api.Responses
{
    /// <summary>
    /// 個案響應模型
    /// </summary>
    public class CaseResponse
    {
        public string CaseId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public DateOnly? BirthDate { get; set; }
        public string? SchoolName { get; set; }
        public string? CityName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
