using CanLove_Backend.Infrastructure.Data.Contexts;
using CanLove_Backend.Domain.Case.Models.Basic;
using Microsoft.EntityFrameworkCore;
using CaseEntity = CanLove_Backend.Domain.Case.Models.Basic.Case;
using AutoMapper;
using CanLove_Backend.Infrastructure.Storage.Encryption;
using CanLove_Backend.Core.Extensions;

namespace CanLove_Backend.Domain.Case.Services.Basic;

/// <summary>
/// 個案服務類別 - 使用 AutoMapper 改善版
/// </summary>
public class CaseService
{
    private readonly CanLoveDbContext _context;
    private readonly IMapper _mapper;
    private readonly DataEncryptionService _encryptionService;

    public CaseService(CanLoveDbContext context, IMapper mapper, DataEncryptionService encryptionService)
    {
        _context = context;
        _mapper = mapper;
        _encryptionService = encryptionService;
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
    /// 建立個案 - 使用 AutoMapper 改善
    /// </summary>
    public async Task<(bool Success, string Message)> CreateCaseAsync(CaseEntity caseData)
    {
        try
        {
            // 驗證 CaseId
            if (string.IsNullOrWhiteSpace(caseData.CaseId))
            {
                return (false, "CaseId 不能為空");
            }

            // 檢查 CaseId 是否已存在
            var existingCase = await _context.Cases
                .FirstOrDefaultAsync(c => c.CaseId == caseData.CaseId);
            
            if (existingCase != null)
            {
                return (false, $"CaseId '{caseData.CaseId}' 已存在，請使用不同的 ID");
            }

            // 檢查身分證字號是否已存在（需要先加密後再比對）
            if (!string.IsNullOrWhiteSpace(caseData.IdNumber))
            {
                // 先加密身分證字號
                var encryptedIdNumber = _encryptionService.Encrypt(caseData.IdNumber);
                
                // 查詢資料庫中是否有相同的身分證字號（已加密）
                var existingIdNumber = await _context.Cases
                    .Where(c => c.IdNumber == encryptedIdNumber && c.Deleted != true)
                    .FirstOrDefaultAsync();
                
                if (existingIdNumber != null)
                {
                    return (false, "此身分證字號已被使用，無法重複申請個案。如為同一人，請使用原有的個案編號。");
                }
                
                // 設定加密後的身分證字號
                caseData.IdNumber = encryptedIdNumber;
            }

            caseData.CreatedAt = DateTimeExtensions.TaiwanTime;
            caseData.UpdatedAt = DateTimeExtensions.TaiwanTime;
            // 若外部（控制器）已設定狀態則沿用；否則一律設為 PendingReview（不再使用 Draft）
            caseData.Status = string.IsNullOrWhiteSpace(caseData.Status) ? "PendingReview" : caseData.Status;
            caseData.Deleted = false;

            // 驗證外鍵是否存在
            if (caseData.CityId.HasValue)
            {
                var cityExists = await _context.Cities.AnyAsync(c => c.CityId == caseData.CityId.Value);
                if (!cityExists)
                {
                    return (false, $"城市 ID {caseData.CityId.Value} 不存在");
                }
            }

            if (caseData.DistrictId.HasValue)
            {
                var districtExists = await _context.Districts.AnyAsync(d => d.DistrictId == caseData.DistrictId.Value);
                if (!districtExists)
                {
                    return (false, $"地區 ID {caseData.DistrictId.Value} 不存在");
                }
            }

            if (caseData.SchoolId.HasValue)
            {
                var schoolExists = await _context.Schools.AnyAsync(s => s.SchoolId == caseData.SchoolId.Value);
                if (!schoolExists)
                {
                    return (false, $"學校 ID {caseData.SchoolId.Value} 不存在");
                }
            }

            // 驗證必填欄位
            if (string.IsNullOrWhiteSpace(caseData.Name))
            {
                return (false, "個案姓名為必填欄位");
            }

            if (caseData.BirthDate == default(DateOnly))
            {
                return (false, "出生日期為必填欄位");
            }

            _context.Cases.Add(caseData);
            await _context.SaveChangesAsync();

            return (true, "個案建立成功");
        }
        catch (DbUpdateException dbEx)
        {
            // 處理資料庫更新錯誤
            var errorMessage = $"個案建立失敗：{dbEx.Message}";
            
            // 如果有 InnerException，加入更詳細的錯誤訊息
            if (dbEx.InnerException != null)
            {
                errorMessage += $" ({dbEx.InnerException.Message})";
            }

            // 檢查是否為外鍵約束錯誤
            if (dbEx.InnerException != null && dbEx.InnerException.Message.Contains("FOREIGN KEY"))
            {
                errorMessage = "個案建立失敗：所選擇的城市、地區或學校不存在，請重新選擇";
            }
            // 檢查是否為主鍵重複錯誤
            else if (dbEx.InnerException != null && (dbEx.InnerException.Message.Contains("PRIMARY KEY") || 
                     dbEx.InnerException.Message.Contains("duplicate key")))
            {
                // 檢查是否為身分證字號唯一約束錯誤
                if (dbEx.InnerException.Message.Contains("UQ__Cases__D58CDE11C0544CB6") || 
                    dbEx.InnerException.Message.Contains("id_number") ||
                    dbEx.InnerException.Message.Contains("IdNumber"))
                {
                    errorMessage = "個案建立失敗：此身分證字號已被使用，無法重複申請個案。如為同一人，請用修改個案功能。";
                }
                else
                {
                    errorMessage = $"個案建立失敗：個案編號 '{caseData.CaseId}' 已存在，請使用不同的編號";
                }
            }
            // 檢查是否為必填欄位錯誤
            else if (dbEx.InnerException != null && dbEx.InnerException.Message.Contains("NOT NULL"))
            {
                errorMessage = "個案建立失敗：請確認所有必填欄位都已填寫";
            }

            return (false, errorMessage);
        }
        catch (Exception ex)
        {
            // 處理其他錯誤
            var errorMessage = $"個案建立失敗：{ex.Message}";
            if (ex.InnerException != null)
            {
                errorMessage += $" ({ex.InnerException.Message})";
            }
            return (false, errorMessage);
        }
    }
}
