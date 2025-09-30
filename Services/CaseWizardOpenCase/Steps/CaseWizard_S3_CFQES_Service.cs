using Microsoft.EntityFrameworkCore;
using CanLove_Backend.Data.Contexts;
using CanLove_Backend.Data.Models.CaseDetails;
using CanLove_Backend.Models.Mvc.ViewModels.CaseWizardOpenCase;
using AutoMapper;

namespace CanLove_Backend.Services.CaseWizardOpenCase.Steps;

/// <summary>
/// 個案開案流程步驟3服務 - 經濟狀況評估 (CaseFQeconomicStatus)
/// </summary>
public class CaseWizard_S3_CFQES_Service
{
    private readonly CanLoveDbContext _context;
    private readonly IMapper _mapper;

    public CaseWizard_S3_CFQES_Service(CanLoveDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// 取得步驟3資料
    /// </summary>
    public async Task<CaseWizard_S3_CFQES_ViewModel> GetStep3DataAsync(string caseId)
    {
        var economicStatus = await _context.CaseFqeconomicStatuses
            .FirstOrDefaultAsync(cfs => cfs.CaseId == caseId);

        // 🎯 使用 AutoMapper 自動轉換，原本需要 8+ 行手動對應，現在只需要 1 行！
        var viewModel = economicStatus != null 
            ? _mapper.Map<CaseWizard_S3_CFQES_ViewModel>(economicStatus)
            : new CaseWizard_S3_CFQES_ViewModel { CaseId = caseId };

        return viewModel;
    }

    /// <summary>
    /// 儲存步驟3資料
    /// </summary>
    public async Task<(bool Success, string Message)> SaveStep3DataAsync(CaseWizard_S3_CFQES_ViewModel model)
    {
        try
        {
            var economicStatus = await _context.CaseFqeconomicStatuses
                .FirstOrDefaultAsync(cfs => cfs.CaseId == model.CaseId);

            if (economicStatus == null)
            {
                economicStatus = new CaseFqeconomicStatus
                {
                    CaseId = model.CaseId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.CaseFqeconomicStatuses.Add(economicStatus);
            }

            economicStatus.EconomicOverview = model.EconomicOverview;
            economicStatus.WorkSituation = model.WorkSituation;
            economicStatus.CivilWelfareResources = model.CivilWelfareResources;
            economicStatus.MonthlyIncome = model.MonthlyIncome;
            economicStatus.MonthlyExpense = model.MonthlyExpense;
            economicStatus.MonthlyExpenseNote = model.MonthlyExpenseNote;
            economicStatus.Description = model.Description;
            economicStatus.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return (true, "步驟3完成，請繼續下一步");
        }
        catch (Exception ex)
        {
            return (false, $"儲存步驟3資料失敗：{ex.Message}");
        }
    }
}
