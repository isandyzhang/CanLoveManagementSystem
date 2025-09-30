using Microsoft.AspNetCore.Mvc;
using CanLove_Backend.Data.Models.Core;
using CanLove_Backend.Models.Api.Responses;
using CanLove_Backend.Models.Api.Requests;
using CanLove_Backend.Services.Case;

namespace CanLove_Backend.Controllers.Api
{
    [ApiController]
    [Route("api/case")]
    public class CaseApiController : ControllerBase
    {
        private readonly CaseService _caseService;

        public CaseApiController(CaseService caseService)
        {
            _caseService = caseService;
        }

        /// <summary>
        /// 取得個案列表
        /// </summary>
        /// <param name="page">頁碼</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResponse<List<CaseResponse>>> GetCases(int page = 1, int pageSize = 10)
        {
            return await _caseService.GetCasesForApiAsync(page, pageSize);
        }

        /// <summary>
        /// 取得個案詳情
        /// </summary>
        /// <param name="id">個案ID</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public ApiResponse<CaseResponse> GetCase(string id)
        {
            try
            {
                // 這裡可以實作取得單一個案的邏輯
                return ApiResponse<CaseResponse>.ErrorResponse("功能尚未實作");
            }
            catch (Exception ex)
            {
                return ApiResponse<CaseResponse>.ErrorResponse($"取得個案失敗：{ex.Message}");
            }
        }

        /// <summary>
        /// 建立個案
        /// </summary>
        /// <param name="request">個案資料</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResponse<CaseResponse>> CreateCase([FromBody] CreateCaseRequest request)
        {
            try
            {
                var caseData = new Case
                {
                    Name = request.Name,
                    Gender = request.Gender,
                    BirthDate = request.BirthDate,
                    IdNumber = request.IdNumber,
                    Address = request.Address,
                    Phone = request.Phone,
                    Email = request.Email,
                    SchoolId = request.SchoolId,
                    CityId = request.CityId,
                    DistrictId = request.DistrictId
                };

                return await _caseService.CreateCaseAsync(caseData);
            }
            catch (Exception ex)
            {
                return ApiResponse<CaseResponse>.ErrorResponse($"建立個案失敗：{ex.Message}");
            }
        }

        /// <summary>
        /// 更新個案
        /// </summary>
        /// <param name="id">個案ID</param>
        /// <param name="request">個案資料</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public ApiResponse<CaseResponse> UpdateCase(string id, [FromBody] UpdateCaseRequest request)
        {
            try
            {
                // 這裡可以實作更新個案的邏輯
                return ApiResponse<CaseResponse>.ErrorResponse("功能尚未實作");
            }
            catch (Exception ex)
            {
                return ApiResponse<CaseResponse>.ErrorResponse($"更新個案失敗：{ex.Message}");
            }
        }

        /// <summary>
        /// 刪除個案
        /// </summary>
        /// <param name="id">個案ID</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public ApiResponse DeleteCase(string id)
        {
            try
            {
                // 這裡可以實作刪除個案的邏輯
                return ApiResponse.ErrorResult("功能尚未實作");
            }
            catch (Exception ex)
            {
                return ApiResponse.ErrorResult($"刪除個案失敗：{ex.Message}");
            }
        }
    }

    /// <summary>
    /// 建立個案請求模型
    /// </summary>
    public class CreateCaseRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public DateOnly BirthDate { get; set; }
        public string IdNumber { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public int? SchoolId { get; set; }
        public int? CityId { get; set; }
        public int? DistrictId { get; set; }
    }

    /// <summary>
    /// 更新個案請求模型
    /// </summary>
    public class UpdateCaseRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public DateOnly BirthDate { get; set; }
        public string IdNumber { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public int? SchoolId { get; set; }
        public int? CityId { get; set; }
        public int? DistrictId { get; set; }
    }
}
