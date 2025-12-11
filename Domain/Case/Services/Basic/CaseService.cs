using CanLove_Backend.Infrastructure.Data.Contexts;
using CanLove_Backend.Domain.Case.Models.Basic;
using CanLove_Backend.Domain.Case.Exceptions;
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

    /// <summary>
    /// 更新個案 - 使用交易確保資料一致性
    /// </summary>
    public async Task<(bool Success, string Message)> UpdateCaseAsync(CaseEntity caseData, string? currentUser = null)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var existingCase = await _context.Cases.FindAsync(caseData.CaseId);
            if (existingCase == null)
            {
                throw new CaseBasicNotFoundException(caseData.CaseId);
            }

            // 檢查鎖定狀態
            if (existingCase.IsLocked == true && existingCase.LockedBy != currentUser)
            {
                throw new CaseBasicLockedException(caseData.CaseId, existingCase.LockedBy ?? "未知");
            }

            // 加密身分證字號（如果已變更）
            if (!string.IsNullOrWhiteSpace(caseData.IdNumber) && caseData.IdNumber != existingCase.IdNumber)
            {
                // 檢查是否為已加密的格式（簡單判斷：如果長度超過正常身分證字號長度，可能是已加密）
                if (caseData.IdNumber.Length <= 10)
                {
                    // 可能是明文，需要加密
                    var encryptedIdNumber = _encryptionService.Encrypt(caseData.IdNumber);
                    
                    // 檢查加密後的身分證字號是否已被其他個案使用
                    var existingIdNumber = await _context.Cases
                        .Where(c => c.IdNumber == encryptedIdNumber && c.CaseId != caseData.CaseId && c.Deleted != true)
                        .FirstOrDefaultAsync();
                    
                    if (existingIdNumber != null)
                    {
                        throw new CaseBasicValidationException("此身分證字號已被其他個案使用");
                    }
                    
                    caseData.IdNumber = encryptedIdNumber;
                }
            }

            // 保留原有的建立時間和審核相關欄位
            caseData.CreatedAt = existingCase.CreatedAt;
            caseData.Status = existingCase.Status;
            caseData.SubmittedBy = existingCase.SubmittedBy;
            caseData.SubmittedAt = existingCase.SubmittedAt;
            caseData.ReviewedBy = existingCase.ReviewedBy;
            caseData.ReviewedAt = existingCase.ReviewedAt;
            caseData.Deleted = existingCase.Deleted;
            caseData.DeletedAt = existingCase.DeletedAt;
            caseData.DeletedBy = existingCase.DeletedBy;
            caseData.IsLocked = existingCase.IsLocked;
            caseData.LockedBy = existingCase.LockedBy;
            caseData.LockedAt = existingCase.LockedAt;
            
            caseData.UpdatedAt = DateTimeExtensions.TaiwanTime;

            _context.Update(caseData);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, "個案更新成功");
        }
        catch (CaseBasicException)
        {
            await transaction.RollbackAsync();
            throw;
        }
        catch (DbUpdateException dbEx)
        {
            await transaction.RollbackAsync();
            throw new CaseBasicSaveException("更新個案", dbEx);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new CaseBasicSaveException("更新個案", ex);
        }
    }

    /// <summary>
    /// 取得個案資料（用於編輯）
    /// </summary>
    public async Task<CaseEntity?> GetCaseForEditAsync(string caseId)
    {
        return await _context.Cases
            .Where(c => c.CaseId == caseId && c.Deleted != true)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// 取得個案資料（用於檢視）
    /// </summary>
    public async Task<CaseEntity?> GetCaseForViewAsync(string caseId)
    {
        return await _context.Cases
            .Include(c => c.City)
            .Include(c => c.District)
            .Include(c => c.School)
            .Where(c => c.CaseId == caseId && c.Deleted != true)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// 取得個案資料（用於審核）
    /// </summary>
    public async Task<CaseEntity?> GetCaseForReviewAsync(string caseId)
    {
        return await _context.Cases
            .Include(c => c.City)
            .Include(c => c.District)
            .Include(c => c.School)
            .Where(c => c.CaseId == caseId && c.Deleted != true && c.Status == "PendingReview")
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// 審核個案
    /// </summary>
    public async Task<(bool Success, string Message)> ReviewCaseAsync(string caseId, bool approved, string? reviewer, string? reviewComment = null)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var caseItem = await _context.Cases.FindAsync(caseId);
            if (caseItem == null)
            {
                throw new CaseBasicNotFoundException(caseId);
            }

            if (caseItem.Status != "PendingReview")
            {
                throw new CaseBasicInvalidStatusException(caseId, caseItem.Status, "PendingReview");
            }

            caseItem.ReviewedBy = reviewer ?? string.Empty;
            caseItem.ReviewedAt = DateTimeExtensions.TaiwanTime;
            caseItem.UpdatedAt = DateTimeExtensions.TaiwanTime;
            
            if (approved)
            {
                caseItem.Status = "Approved";
            }
            else
            {
                caseItem.Status = "Rejected";
                caseItem.SubmittedAt = null; // 退回時清除提交時間
            }

            _context.Update(caseItem);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var message = approved ? "個案審核通過" : "個案已退回";
            return (true, message);
        }
        catch (CaseBasicException)
        {
            await transaction.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new CaseBasicSaveException("審核個案", ex);
        }
    }

    /// <summary>
    /// 鎖定/解鎖個案
    /// </summary>
    public async Task<(bool Success, string Message)> ToggleLockAsync(string caseId, string? currentUser)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var caseItem = await _context.Cases.FindAsync(caseId);
            if (caseItem == null)
            {
                throw new CaseBasicNotFoundException(caseId);
            }

            if (caseItem.IsLocked == true)
            {
                // 解鎖
                if (caseItem.LockedBy != currentUser)
                {
                    throw new CaseBasicLockedException(caseId, caseItem.LockedBy ?? "未知");
                }

                caseItem.IsLocked = false;
                caseItem.LockedBy = null;
                caseItem.LockedAt = null;
            }
            else
            {
                // 鎖定
                caseItem.IsLocked = true;
                caseItem.LockedBy = currentUser ?? string.Empty;
                caseItem.LockedAt = DateTimeExtensions.TaiwanTime;
            }
            
            caseItem.UpdatedAt = DateTimeExtensions.TaiwanTime;
            _context.Update(caseItem);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var message = caseItem.IsLocked == true ? "個案已鎖定" : "個案已解鎖";
            return (true, message);
        }
        catch (CaseBasicException)
        {
            await transaction.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new CaseBasicSaveException("鎖定/解鎖個案", ex);
        }
    }

    /// <summary>
    /// 提交個案審核
    /// </summary>
    public async Task<(bool Success, string Message)> SubmitForReviewAsync(string caseId, string? submittedBy)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var caseItem = await _context.Cases.FindAsync(caseId);
            if (caseItem == null)
            {
                throw new CaseBasicNotFoundException(caseId);
            }

            // 檢查是否為建立者
            if (caseItem.SubmittedBy != submittedBy)
            {
                throw new CaseBasicValidationException("您只能提交自己建立的個案");
            }

            // 檢查是否已經提交過
            if (caseItem.SubmittedAt != null)
            {
                throw new CaseBasicValidationException("此個案已經提交審核");
            }

            caseItem.SubmittedAt = DateTimeExtensions.TaiwanTime;
            caseItem.Status = "PendingReview";
            caseItem.UpdatedAt = DateTimeExtensions.TaiwanTime;
            
            _context.Update(caseItem);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, "個案已提交審核");
        }
        catch (CaseBasicException)
        {
            await transaction.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new CaseBasicSaveException("提交個案審核", ex);
        }
    }

    /// <summary>
    /// 軟刪除個案
    /// </summary>
    public async Task<(bool Success, string Message)> DeleteCaseAsync(string caseId, string? deletedBy)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var caseItem = await _context.Cases.FindAsync(caseId);
            if (caseItem == null)
            {
                throw new CaseBasicNotFoundException(caseId);
            }

            if (caseItem.Deleted == true)
            {
                throw new CaseBasicValidationException("此個案已經被刪除");
            }

            caseItem.Deleted = true;
            caseItem.DeletedAt = DateTimeExtensions.TaiwanTime;
            caseItem.DeletedBy = deletedBy ?? "System";
            caseItem.UpdatedAt = DateTimeExtensions.TaiwanTime;

            _context.Update(caseItem);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, $"個案「{caseItem.Name}」已成功刪除");
        }
        catch (CaseBasicException)
        {
            await transaction.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new CaseBasicSaveException("刪除個案", ex);
        }
    }
}
