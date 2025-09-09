using CanLove_Backend.Data.Contexts;
using CanLove_Backend.Data.Models.Core;
using CanLove_Backend.Models.Api.Responses;
using Microsoft.EntityFrameworkCore;

namespace CanLove_Backend.Services
{
    /// <summary>
    /// 個案服務類別
    /// </summary>
    public class CaseService
    {
        private readonly CanLoveDbContext _context;

        public CaseService(CanLoveDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 取得個案列表的核心方法
        /// </summary>
        private async Task<(List<Case> Cases, int TotalCount)> GetCasesCoreAsync(int page = 1, int pageSize = 20)
        {
            var totalCount = await _context.Cases
                .Where(c => c.Deleted != true)
                .CountAsync();

            var cases = await _context.Cases
                .Include(c => c.City)
                .Include(c => c.District)
                .Include(c => c.School)
                .Where(c => c.Deleted != true)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (cases, totalCount);
        }

        /// <summary>
        /// 取得個案列表（MVC 用）
        /// </summary>
        public async Task<(List<Case> Cases, int TotalCount)> GetCasesForMvcAsync(int page = 1, int pageSize = 20)
        {
            return await GetCasesCoreAsync(page, pageSize);
        }

        /// <summary>
        /// 取得個案列表（API 用）
        /// </summary>
        public async Task<ApiResponse<List<CaseResponse>>> GetCasesForApiAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (cases, totalCount) = await GetCasesCoreAsync(page, pageSize);
                
                var caseResponses = cases.Select(c => new CaseResponse
                {
                    CaseId = c.CaseId,
                    Name = c.Name,
                    Gender = c.Gender,
                    BirthDate = c.BirthDate,
                    SchoolName = c.School != null ? c.School.SchoolName : null,
                    CityName = c.City != null ? c.City.CityName : null,
                    CreatedAt = c.CreatedAt ?? DateTime.UtcNow
                }).ToList();

                return ApiResponse<List<CaseResponse>>.SuccessResponse(caseResponses, "取得個案列表成功");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<CaseResponse>>.ErrorResponse($"取得個案列表失敗：{ex.Message}");
            }
        }

        /// <summary>
        /// 建立個案
        /// </summary>
        public async Task<ApiResponse<CaseResponse>> CreateCaseAsync(Case caseData)
        {
            try
            {
                caseData.CaseId = Guid.NewGuid().ToString();
                caseData.CreatedAt = DateTime.UtcNow;
                caseData.UpdatedAt = DateTime.UtcNow;
                caseData.DraftStatus = true;
                caseData.Deleted = false;

                _context.Cases.Add(caseData);
                await _context.SaveChangesAsync();

                var response = new CaseResponse
                {
                    CaseId = caseData.CaseId,
                    Name = caseData.Name,
                    Gender = caseData.Gender,
                    BirthDate = caseData.BirthDate,
                    CreatedAt = caseData.CreatedAt ?? DateTime.UtcNow
                };

                return ApiResponse<CaseResponse>.SuccessResponse(response, "個案建立成功");
            }
            catch (Exception ex)
            {
                return ApiResponse<CaseResponse>.ErrorResponse($"個案建立失敗：{ex.Message}");
            }
        }
    }

}
