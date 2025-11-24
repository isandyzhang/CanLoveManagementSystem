using System.Security.Claims;
using System.Threading.Tasks;
using CanLove_Backend.Domain.Staff.Models;

namespace CanLove_Backend.Domain.Staff.Services;

public interface IStaffService
{
    /// <summary>
    /// 根據Azure ObjectId取得員工
    /// </summary>
    Task<Models.Staff?> GetStaffByAzureObjectIdAsync(string azureObjectId);

    /// <summary>
    /// 從Azure AD Claims建立員工
    /// </summary>
    Task<Models.Staff> CreateStaffFromAzureAsync(ClaimsPrincipal principal);

    /// <summary>
    /// 從Azure AD Claims更新員工資訊（包含頭像URL）
    /// </summary>
    Task<Models.Staff> UpdateStaffFromAzureAsync(Models.Staff staff, ClaimsPrincipal principal);

    /// <summary>
    /// 記錄員工登入
    /// </summary>
    Task LogStaffLoginAsync(int staffId, string? ipAddress, string? userAgent);

    /// <summary>
    /// 取得所有員工（僅必要欄位）
    /// </summary>
    Task<List<Models.Staff>> GetAllForListAsync();

    /// <summary>
    /// 更新員工的部門與職稱
    /// </summary>
    Task UpdateDepartmentAndJobTitleAsync(int staffId, string? department, string? jobTitle);
}

