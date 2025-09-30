using Microsoft.EntityFrameworkCore;
using CanLove_Backend.Data.Contexts;
using CanLove_Backend.Data.Models.CaseDetails;
using CanLove_Backend.Models.Mvc.ViewModels.CaseWizardOpenCase;
using AutoMapper;

namespace CanLove_Backend.Services.CaseWizardOpenCase.Steps;

/// <summary>
/// 個案開案流程步驟5服務 - 學業表現評估 (CaseIqacademicPerformance)
/// </summary>
public class CaseWizard_S5_CIQAP_Service
{
    private readonly CanLoveDbContext _context;
    private readonly IMapper _mapper;

    public CaseWizard_S5_CIQAP_Service(CanLoveDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// 取得步驟5資料
    /// </summary>
    public async Task<CaseWizard_S5_CIQAP_ViewModel> GetStep5DataAsync(string caseId)
    {
        var academicPerformance = await _context.CaseIqacademicPerformances
            .FirstOrDefaultAsync(cap => cap.CaseId == caseId);

        // 🎯 使用 AutoMapper 自動轉換，原本需要 3+ 行手動對應，現在只需要 1 行！
        var viewModel = academicPerformance != null 
            ? _mapper.Map<CaseWizard_S5_CIQAP_ViewModel>(academicPerformance)
            : new CaseWizard_S5_CIQAP_ViewModel { CaseId = caseId };

        return viewModel;
    }

    /// <summary>
    /// 儲存步驟5資料
    /// </summary>
    public async Task<(bool Success, string Message)> SaveStep5DataAsync(CaseWizard_S5_CIQAP_ViewModel model)
    {
        try
        {
            var academicPerformance = await _context.CaseIqacademicPerformances
                .FirstOrDefaultAsync(cap => cap.CaseId == model.CaseId);

            if (academicPerformance == null)
            {
                academicPerformance = new CaseIqacademicPerformance
                {
                    CaseId = model.CaseId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.CaseIqacademicPerformances.Add(academicPerformance);
            }

            academicPerformance.AcademicPerformanceSummary = model.AcademicPerformanceSummary;
            academicPerformance.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return (true, "步驟5完成，請繼續下一步");
        }
        catch (Exception ex)
        {
            return (false, $"儲存步驟5資料失敗：{ex.Message}");
        }
    }
}
