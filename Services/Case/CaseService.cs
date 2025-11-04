using CanLove_Backend.Data.Contexts;
using CanLove_Backend.Data.Models.Core;
using CanLove_Backend.Models.Api.Responses;
using Microsoft.EntityFrameworkCore;
using CaseEntity = CanLove_Backend.Data.Models.Core.Case;
using AutoMapper;

namespace CanLove_Backend.Services.Case;

/// <summary>
/// 個案服務類別 - 使用 AutoMapper 改善版
/// </summary>
public class CaseService
{
    private readonly CanLoveDbContext _context;
    private readonly IMapper _mapper;

    public CaseService(CanLoveDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// 取得個案列表的核心方法
    /// </summary>
    private async Task<(List<CaseEntity> Cases, int TotalCount)> GetCasesCoreAsync(int page = 1, int pageSize = 20)
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
    public async Task<(List<CaseEntity> Cases, int TotalCount)> GetCasesForMvcAsync(int page = 1, int pageSize = 20)
    {
        return await GetCasesCoreAsync(page, pageSize);
    }

    /// <summary>
    /// 取得個案列表（API 用） - 使用 AutoMapper 改善
    /// </summary>
    public async Task<ApiResponse<List<CaseResponse>>> GetCasesForApiAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            var (cases, totalCount) = await GetCasesCoreAsync(page, pageSize);
            
            // 🎯 原本需要手動對應每個屬性，現在只需要 1 行！
            var caseResponses = _mapper.Map<List<CaseResponse>>(cases);

            return ApiResponse<List<CaseResponse>>.SuccessResponse(caseResponses, "取得個案列表成功");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<CaseResponse>>.ErrorResponse($"取得個案列表失敗：{ex.Message}");
        }
    }

    /// <summary>
    /// 建立個案 - 使用 AutoMapper 改善
    /// </summary>
    public async Task<ApiResponse<CaseResponse>> CreateCaseAsync(CaseEntity caseData)
    {
        try
        {
            // 驗證 CaseId
            if (string.IsNullOrWhiteSpace(caseData.CaseId))
            {
                return new ApiResponse<CaseResponse>
                {
                    Success = false,
                    Message = "CaseId 不能為空"
                };
            }

            // 檢查 CaseId 是否已存在
            var existingCase = await _context.Cases
                .FirstOrDefaultAsync(c => c.CaseId == caseData.CaseId);
            
            if (existingCase != null)
            {
                return new ApiResponse<CaseResponse>
                {
                    Success = false,
                    Message = $"CaseId '{caseData.CaseId}' 已存在，請使用不同的 ID"
                };
            }

            caseData.CreatedAt = DateTime.UtcNow;
            caseData.UpdatedAt = DateTime.UtcNow;
            caseData.Status = caseData.Status ?? "Draft"; // 如果已有狀態就保留，否則設為 Draft
            caseData.Deleted = false;

            _context.Cases.Add(caseData);
            await _context.SaveChangesAsync();

            // 為避免個別環境的 AutoMapper/導覽屬性延遲載入造成型別轉換異常，
            // 這裡回傳最小成功響應，僅帶必需欄位。
            var response = new CaseResponse
            {
                CaseId = caseData.CaseId,
                Name = caseData.Name,
                Gender = caseData.Gender,
                BirthDate = caseData.BirthDate,
                CityName = caseData.City?.CityName,
                SchoolName = caseData.School?.SchoolName,
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
