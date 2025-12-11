using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using CanLove_Backend.Infrastructure.Data.Contexts;
using CanLove_Backend.Domain.Case.Shared.Models;
using CanLove_Backend.Infrastructure.Options.Models;
using Microsoft.Extensions.Caching.Memory;

namespace CanLove_Backend.Infrastructure.Options.Services;

/// <summary>
/// 選項資料服務
/// </summary>
public class OptionService
{
    private readonly CanLoveDbContext _context;
    private readonly IDbContextFactory<CanLoveDbContext> _contextFactory;
    private readonly IMemoryCache _cache;
    private const int CacheExpirationMinutes = 30; // 快取過期時間：30分鐘

    public OptionService(
        CanLoveDbContext context,
        IDbContextFactory<CanLoveDbContext> contextFactory,
        IMemoryCache cache)
    {
        _context = context;
        _contextFactory = contextFactory;
        _cache = cache;
    }

    /// <summary>
    /// 根據選項鍵值取得選項清單（含快取）
    /// 使用獨立的 DbContext 實例以避免並發問題
    /// </summary>
    /// <param name="optionKey">選項鍵值 (例如: "GENDER", "CONTACT_RELATION")</param>
    /// <returns>選項清單</returns>
    public async Task<List<OptionSetValue>> GetOptionsByKeyAsync(string optionKey)
    {
        var cacheKey = $"OptionSet_{optionKey}";
        
        // 嘗試從快取取得
        if (_cache.TryGetValue(cacheKey, out List<OptionSetValue>? cachedOptions) && cachedOptions != null)
        {
            return cachedOptions;
        }

        // 使用 Factory 創建獨立的 DbContext 實例
        await using var context = await _contextFactory.CreateDbContextAsync();
        var options = await context.OptionSetValues
            .Include(o => o.OptionSet)
            .Where(o => o.OptionSet.OptionKey == optionKey)
            .OrderBy(o => o.ValueCode)
            .AsNoTracking()
            .ToListAsync();

        // 存入快取
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes),
            SlidingExpiration = TimeSpan.FromMinutes(10) // 滑動過期時間：10分鐘
        };
        _cache.Set(cacheKey, options, cacheOptions);

        return options;
    }

    /// <summary>
    /// 取得性別選項
    /// </summary>
    public async Task<List<OptionSetValue>> GetGenderOptionsAsync()
    {
        return await GetOptionsByKeyAsync("GENDER");
    }

    /// <summary>
    /// 取得與案主關係選項
    /// </summary>
    public async Task<List<OptionSetValue>> GetContactRelationOptionsAsync()
    {
        return await GetOptionsByKeyAsync("CONTACT_RELATION");
    }

    /// <summary>
    /// 取得家庭結構類型選項
    /// </summary>
    public async Task<List<OptionSetValue>> GetFamilyStructureTypeOptionsAsync()
    {
        return await GetOptionsByKeyAsync("FAMILY_STRUCTURE_TYPE");
    }

    /// <summary>
    /// 取得國籍選項
    /// </summary>
    public async Task<List<OptionSetValue>> GetNationalityOptionsAsync()
    {
        return await GetOptionsByKeyAsync("NATIONALITY");
    }

    /// <summary>
    /// 取得婚姻狀況選項
    /// </summary>
    public async Task<List<OptionSetValue>> GetMarryStatusOptionsAsync()
    {
        return await GetOptionsByKeyAsync("MARITAL_STATUS");
    }

    /// <summary>
    /// 取得教育程度選項
    /// </summary>
    public async Task<List<OptionSetValue>> GetEducationLevelOptionsAsync()
    {
        return await GetOptionsByKeyAsync("EDUCATION_LEVEL");
    }

    /// <summary>
    /// 取得個案來源選項
    /// </summary>
    public async Task<List<OptionSetValue>> GetSourceOptionsAsync()
    {
        return await GetOptionsByKeyAsync("CASE_SOURCE");
    }

    /// <summary>
    /// 取得求助經驗選項
    /// </summary>
    public async Task<List<OptionSetValue>> GetHelpExperienceOptionsAsync()
    {
        return await GetOptionsByKeyAsync("HELP_EXPERIENCE");
    }

    /// <summary>
    /// 取得居住地型態選項
    /// </summary>
    public async Task<List<OptionSetValue>> GetResidenceTypeOptionsAsync()
    {
        return await GetOptionsByKeyAsync("RESIDENCE_TYPE");
    }

    /// <summary>
    /// 取得照顧者角色選項
    /// </summary>
    public async Task<List<OptionSetValue>> GetCaregiverRoleOptionsAsync()
    {
        return await GetOptionsByKeyAsync("CARE_GIVER_ROLE");
    }

    /// <summary>
    /// 取得所有選項鍵值清單
    /// </summary>
    public async Task<List<string>> GetAllOptionKeysAsync()
    {
        return await _context.OptionSets
            .Select(o => o.OptionKey)
            .OrderBy(o => o)
            .ToListAsync();
    }

    /// <summary>
    /// 取得員工部門選項
    /// </summary>
    public async Task<List<OptionSetValue>> GetStaffDepartmentOptionsAsync()
    {
        return await GetOptionsByKeyAsync("STAFF_DEPARTMENT");
    }

    /// <summary>
    /// 取得員工職稱選項
    /// </summary>
    public async Task<List<OptionSetValue>> GetStaffJobTitleOptionsAsync()
    {
        return await GetOptionsByKeyAsync("STAFF_JOB_TITLE");
    }

    /// <summary>
    /// 取得家庭結構類型清單（含快取）
    /// </summary>
    public async Task<List<FamilyStructureType>> GetFamilyStructureTypesAsync()
    {
        const string cacheKey = "FamilyStructureTypes";
        
        // 嘗試從快取取得
        if (_cache.TryGetValue(cacheKey, out List<FamilyStructureType>? cachedTypes) && cachedTypes != null)
        {
            return cachedTypes;
        }

        // 從資料庫查詢
        var types = await _context.FamilyStructureTypes
            .OrderBy(f => f.StructureTypeId)
            .AsNoTracking()
            .ToListAsync();

        // 存入快取
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes),
            SlidingExpiration = TimeSpan.FromMinutes(10) // 滑動過期時間：10分鐘
        };
        _cache.Set(cacheKey, types, cacheOptions);

        return types;
    }

    /// <summary>
    /// 取得國籍清單（含快取）
    /// </summary>
    public async Task<List<Nationality>> GetNationalitiesAsync()
    {
        const string cacheKey = "Nationalities";
        
        // 嘗試從快取取得
        if (_cache.TryGetValue(cacheKey, out List<Nationality>? cachedNationalities) && cachedNationalities != null)
        {
            return cachedNationalities;
        }

        // 從資料庫查詢
        var nationalities = await _context.Nationalities
            .OrderBy(n => n.NationalityId)
            .AsNoTracking()
            .ToListAsync();

        // 存入快取
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes),
            SlidingExpiration = TimeSpan.FromMinutes(10) // 滑動過期時間：10分鐘
        };
        _cache.Set(cacheKey, nationalities, cacheOptions);

        return nationalities;
    }
}
