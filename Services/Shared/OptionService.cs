using Microsoft.EntityFrameworkCore;
using CanLove_Backend.Data.Contexts;
using CanLove_Backend.Data.Models.Options;

namespace CanLove_Backend.Services.Shared;

/// <summary>
/// 選項資料服務
/// </summary>
public class OptionService
{
    private readonly CanLoveDbContext _context;

    public OptionService(CanLoveDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 根據選項鍵值取得選項清單
    /// </summary>
    /// <param name="optionKey">選項鍵值 (例如: "GENDER", "CONTACT_RELATION")</param>
    /// <returns>選項清單</returns>
    public async Task<List<OptionSetValue>> GetOptionsByKeyAsync(string optionKey)
    {
        return await _context.OptionSetValues
            .Include(o => o.OptionSet)
            .Where(o => o.OptionSet.OptionKey == optionKey)
            .OrderBy(o => o.ValueCode)
            .ToListAsync();
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
}
