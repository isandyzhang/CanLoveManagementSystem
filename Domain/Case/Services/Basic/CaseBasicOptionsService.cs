using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore.Infrastructure;
using CanLove_Backend.Infrastructure.Data.Contexts;
using CanLove_Backend.Domain.Case.Shared.Models;
using CanLove_Backend.Infrastructure.Options.Models;
using CanLove_Backend.Infrastructure.Options.Services;
using CanLove_Backend.Domain.Case.Shared.Services;

namespace CanLove_Backend.Domain.Case.Services.Basic;

/// <summary>
/// 個案基本資料選項服務 - 統一載入選項資料並提供快取機制
/// </summary>
public class CaseBasicOptionsService
{
    private readonly CanLoveDbContext _context;
    private readonly IDbContextFactory<CanLoveDbContext> _contextFactory;
    private readonly IMemoryCache _cache;
    private readonly OptionService _optionService;
    private readonly SchoolService _schoolService;
    private const int CacheExpirationMinutes = 30; // 快取過期時間：30分鐘

    public CaseBasicOptionsService(
        CanLoveDbContext context,
        IDbContextFactory<CanLoveDbContext> contextFactory,
        IMemoryCache cache,
        OptionService optionService,
        SchoolService schoolService)
    {
        _context = context;
        _contextFactory = contextFactory;
        _cache = cache;
        _optionService = optionService;
        _schoolService = schoolService;
    }

    /// <summary>
    /// 取得所有選項資料（含快取）
    /// </summary>
    public async Task<CaseBasicOptionsData> GetAllOptionsAsync()
    {
        var cacheKey = "CaseBasic_AllOptions";
        
        // 嘗試從快取取得
        if (_cache.TryGetValue(cacheKey, out CaseBasicOptionsData? cachedData) && cachedData != null)
        {
            return cachedData;
        }

        // 使用獨立的 DbContext 實例進行並行載入，避免並發衝突
        var citiesTask = GetCitiesWithFactoryAsync();
        var districtsTask = GetDistrictsWithFactoryAsync();
        var schoolsTask = GetSchoolsAsync();
        var genderOptionsTask = GetGenderOptionsAsync();

        await Task.WhenAll(citiesTask, districtsTask, schoolsTask, genderOptionsTask);

        var data = new CaseBasicOptionsData
        {
            Cities = await citiesTask,
            Districts = await districtsTask,
            Schools = await schoolsTask,
            GenderOptions = await genderOptionsTask,
            DistrictsByCity = await GetDistrictsByCityWithFactoryAsync()
        };

        // 存入快取
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes),
            SlidingExpiration = TimeSpan.FromMinutes(10) // 滑動過期時間：10分鐘
        };
        _cache.Set(cacheKey, data, cacheOptions);

        return data;
    }

    /// <summary>
    /// 取得城市列表（含快取）
    /// </summary>
    public async Task<List<City>> GetCitiesAsync()
    {
        var cacheKey = "CaseBasic_Cities";
        
        if (_cache.TryGetValue(cacheKey, out List<City>? cachedCities) && cachedCities != null)
        {
            return cachedCities;
        }

        var cities = await _context.Cities
            .OrderBy(c => c.CityId)
            .AsNoTracking()
            .ToListAsync();

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes),
            SlidingExpiration = TimeSpan.FromMinutes(10)
        };
        _cache.Set(cacheKey, cities, cacheOptions);

        return cities;
    }

    /// <summary>
    /// 取得地區列表（含快取）
    /// </summary>
    public async Task<List<District>> GetDistrictsAsync()
    {
        var cacheKey = "CaseBasic_Districts";
        
        if (_cache.TryGetValue(cacheKey, out List<District>? cachedDistricts) && cachedDistricts != null)
        {
            return cachedDistricts;
        }

        var districts = await _context.Districts
            .Include(d => d.City)
            .OrderBy(d => d.CityId)
            .ThenBy(d => d.DistrictName)
            .AsNoTracking()
            .ToListAsync();

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes),
            SlidingExpiration = TimeSpan.FromMinutes(10)
        };
        _cache.Set(cacheKey, districts, cacheOptions);

        return districts;
    }

    /// <summary>
    /// 取得學校列表（含快取）
    /// </summary>
    public async Task<List<School>> GetSchoolsAsync()
    {
        var cacheKey = "CaseBasic_Schools";
        
        if (_cache.TryGetValue(cacheKey, out List<School>? cachedSchools) && cachedSchools != null)
        {
            return cachedSchools;
        }

        var schools = await _schoolService.GetAllSchoolsAsync();

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes),
            SlidingExpiration = TimeSpan.FromMinutes(10)
        };
        _cache.Set(cacheKey, schools, cacheOptions);

        return schools;
    }

    /// <summary>
    /// 取得性別選項（含快取）
    /// </summary>
    public async Task<List<OptionSetValue>> GetGenderOptionsAsync()
    {
        return await _optionService.GetGenderOptionsAsync();
    }

    /// <summary>
    /// 取得按城市分組的地區資料（含快取）
    /// </summary>
    public async Task<Dictionary<int, List<DistrictGroupItem>>> GetDistrictsByCityAsync()
    {
        var cacheKey = "CaseBasic_DistrictsByCity";
        
        if (_cache.TryGetValue(cacheKey, out Dictionary<int, List<DistrictGroupItem>>? cachedData) && cachedData != null)
        {
            return cachedData;
        }

        var allDistricts = await _context.Districts
            .Select(d => new { 
                DistrictId = d.DistrictId, 
                DistrictName = d.DistrictName,
                CityId = d.CityId
            })
            .OrderBy(d => d.CityId)
            .ThenBy(d => d.DistrictName)
            .AsNoTracking()
            .ToListAsync();

        var districtsByCity = allDistricts
            .GroupBy(d => d.CityId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(d => new DistrictGroupItem
                { 
                    DistrictId = d.DistrictId, 
                    DistrictName = d.DistrictName 
                }).ToList()
            );

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes),
            SlidingExpiration = TimeSpan.FromMinutes(10)
        };
        _cache.Set(cacheKey, districtsByCity, cacheOptions);

        return districtsByCity;
    }

    /// <summary>
    /// 使用 Factory 創建獨立 DbContext 取得城市列表（用於並行操作）
    /// </summary>
    private async Task<List<City>> GetCitiesWithFactoryAsync()
    {
        var cacheKey = "CaseBasic_Cities";
        
        if (_cache.TryGetValue(cacheKey, out List<City>? cachedCities) && cachedCities != null)
        {
            return cachedCities;
        }

        await using var context = await _contextFactory.CreateDbContextAsync();
        var cities = await context.Cities
            .OrderBy(c => c.CityId)
            .AsNoTracking()
            .ToListAsync();

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes),
            SlidingExpiration = TimeSpan.FromMinutes(10)
        };
        _cache.Set(cacheKey, cities, cacheOptions);

        return cities;
    }

    /// <summary>
    /// 使用 Factory 創建獨立 DbContext 取得地區列表（用於並行操作）
    /// </summary>
    private async Task<List<District>> GetDistrictsWithFactoryAsync()
    {
        var cacheKey = "CaseBasic_Districts";
        
        if (_cache.TryGetValue(cacheKey, out List<District>? cachedDistricts) && cachedDistricts != null)
        {
            return cachedDistricts;
        }

        await using var context = await _contextFactory.CreateDbContextAsync();
        var districts = await context.Districts
            .Include(d => d.City)
            .OrderBy(d => d.CityId)
            .ThenBy(d => d.DistrictName)
            .AsNoTracking()
            .ToListAsync();

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes),
            SlidingExpiration = TimeSpan.FromMinutes(10)
        };
        _cache.Set(cacheKey, districts, cacheOptions);

        return districts;
    }

    /// <summary>
    /// 使用 Factory 創建獨立 DbContext 取得按城市分組的地區資料（用於並行操作）
    /// </summary>
    private async Task<Dictionary<int, List<DistrictGroupItem>>> GetDistrictsByCityWithFactoryAsync()
    {
        var cacheKey = "CaseBasic_DistrictsByCity";
        
        if (_cache.TryGetValue(cacheKey, out Dictionary<int, List<DistrictGroupItem>>? cachedData) && cachedData != null)
        {
            return cachedData;
        }

        await using var context = await _contextFactory.CreateDbContextAsync();
        var allDistricts = await context.Districts
            .Select(d => new { 
                DistrictId = d.DistrictId, 
                DistrictName = d.DistrictName,
                CityId = d.CityId
            })
            .OrderBy(d => d.CityId)
            .ThenBy(d => d.DistrictName)
            .AsNoTracking()
            .ToListAsync();

        var districtsByCity = allDistricts
            .GroupBy(d => d.CityId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(d => new DistrictGroupItem
                { 
                    DistrictId = d.DistrictId, 
                    DistrictName = d.DistrictName 
                }).ToList()
            );

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes),
            SlidingExpiration = TimeSpan.FromMinutes(10)
        };
        _cache.Set(cacheKey, districtsByCity, cacheOptions);

        return districtsByCity;
    }

    /// <summary>
    /// 清除所有選項資料快取
    /// </summary>
    public void InvalidateCache()
    {
        _cache.Remove("CaseBasic_AllOptions");
        _cache.Remove("CaseBasic_Cities");
        _cache.Remove("CaseBasic_Districts");
        _cache.Remove("CaseBasic_Schools");
        _cache.Remove("CaseBasic_DistrictsByCity");
    }
}

/// <summary>
/// 個案基本資料選項資料容器
/// </summary>
public class CaseBasicOptionsData
{
    public List<City> Cities { get; set; } = new();
    public List<District> Districts { get; set; } = new();
    public List<School> Schools { get; set; } = new();
    public List<OptionSetValue> GenderOptions { get; set; } = new();
    public Dictionary<int, List<DistrictGroupItem>> DistrictsByCity { get; set; } = new();
}

/// <summary>
/// 地區分組項目
/// </summary>
public class DistrictGroupItem
{
    public int DistrictId { get; set; }
    public string DistrictName { get; set; } = string.Empty;
}
