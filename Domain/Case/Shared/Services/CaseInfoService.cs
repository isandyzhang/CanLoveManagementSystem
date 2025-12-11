using Microsoft.EntityFrameworkCore;
using CanLove_Backend.Infrastructure.Data.Contexts;
using CanLove_Backend.Domain.Case.Models.Basic;
using Microsoft.Extensions.Caching.Memory;
using CaseEntity = CanLove_Backend.Domain.Case.Models.Basic.Case;

namespace CanLove_Backend.Domain.Case.Shared.Services;

/// <summary>
/// 個案資訊服務 - 統一處理個案基本資訊載入，並提供快取機制
/// </summary>
public class CaseInfoService
{
    private readonly CanLoveDbContext _context;
    private readonly IMemoryCache _cache;
    private const int CacheExpirationMinutes = 5; // 快取過期時間：5分鐘（個案基本資料變更頻率低）

    public CaseInfoService(CanLoveDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    /// <summary>
    /// 載入個案基本資訊（包含 City, District, School），並使用快取機制
    /// </summary>
    /// <param name="caseId">個案編號</param>
    /// <returns>個案實體，如果不存在則返回 null</returns>
    public async Task<CaseEntity?> GetCaseInfoAsync(string? caseId)
    {
        if (string.IsNullOrEmpty(caseId))
        {
            return null;
        }

        var cacheKey = $"CaseInfo_{caseId}";

        // 嘗試從快取取得
        if (_cache.TryGetValue(cacheKey, out CaseEntity? cachedCase) && cachedCase != null)
        {
            return cachedCase;
        }

        // 從資料庫查詢（使用 AsNoTracking 避免不必要的實體追蹤）
        var caseInfo = await _context.Cases
            .Include(c => c.City)
            .Include(c => c.District)
            .Include(c => c.School)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CaseId == caseId && c.Deleted != true);

        if (caseInfo != null)
        {
            // 存入快取
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes),
                SlidingExpiration = TimeSpan.FromMinutes(2) // 滑動過期時間：2分鐘
            };
            _cache.Set(cacheKey, caseInfo, cacheOptions);
        }

        return caseInfo;
    }

    /// <summary>
    /// 清除指定個案的快取
    /// </summary>
    /// <param name="caseId">個案編號</param>
    public void InvalidateCache(string caseId)
    {
        if (string.IsNullOrEmpty(caseId))
        {
            return;
        }

        var cacheKey = $"CaseInfo_{caseId}";
        _cache.Remove(cacheKey);
    }
}
