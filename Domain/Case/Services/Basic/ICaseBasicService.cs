using CanLove_Backend.Domain.Case.Models.Basic;
using CaseEntity = CanLove_Backend.Domain.Case.Models.Basic.Case;

namespace CanLove_Backend.Domain.Case.Services.Basic;

/// <summary>
/// 個案基本資料服務介面
/// </summary>
public interface ICaseBasicService
{
    Task<(List<CaseEntity> Cases, int TotalCount)> GetCasesForMvcAsync(int page = 1, int pageSize = 20);
    Task<(bool Success, string Message)> CreateCaseAsync(CaseEntity caseData);
    Task<(bool Success, string Message)> UpdateCaseAsync(CaseEntity caseData, string? currentUser = null);
    Task<CaseEntity?> GetCaseForEditAsync(string caseId);
    Task<CaseEntity?> GetCaseForViewAsync(string caseId);
    Task<CaseEntity?> GetCaseForReviewAsync(string caseId);
    Task<(bool Success, string Message)> ReviewCaseAsync(string caseId, bool approved, string? reviewer, string? reviewComment = null);
    Task<(bool Success, string Message)> ToggleLockAsync(string caseId, string? currentUser);
    Task<(bool Success, string Message)> SubmitForReviewAsync(string caseId, string? submittedBy);
    Task<(bool Success, string Message)> DeleteCaseAsync(string caseId, string? deletedBy);
}

