
using Microsoft.EntityFrameworkCore;
using CanLove_Backend.Infrastructure.Data.Contexts;
using CanLove_Backend.Domain.Case.Models.Opening;
using CanLove_Backend.Domain.Case.ViewModels.Opening;
using CanLove_Backend.Core.Extensions;
using AutoMapper;

namespace CanLove_Backend.Domain.Case.Services.Opening.Steps;

/// <summary>
/// 個案開案流程步驟7服務 - 最後評估表 (FinalAssessmentSummary)
/// </summary>
public class CaseWizard_S7_FAS_Service
{
    private readonly CanLoveDbContext _context;
    private readonly IMapper _mapper;

    public CaseWizard_S7_FAS_Service(CanLoveDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// 取得步驟7資料
    /// </summary>
    public async Task<CaseWizard_S7_FAS_ViewModel> GetStep7DataAsync(string caseId)
    {
        var finalAssessment = await _context.FinalAssessmentSummaries
            .FirstOrDefaultAsync(fas => fas.CaseId == caseId);

        // 🎯 使用 AutoMapper 自動轉換，原本需要 5+ 行手動對應，現在只需要 1 行！
        var viewModel = finalAssessment != null 
            ? _mapper.Map<CaseWizard_S7_FAS_ViewModel>(finalAssessment)
            : new CaseWizard_S7_FAS_ViewModel { CaseId = caseId };

        return viewModel;
    }

    /// <summary>
    /// 儲存步驟7資料
    /// </summary>
    public async Task<(bool Success, string Message)> SaveStep7DataAsync(CaseWizard_S7_FAS_ViewModel model)
    {
        try
        {
            var finalAssessment = await _context.FinalAssessmentSummaries
                .FirstOrDefaultAsync(fas => fas.CaseId == model.CaseId);

            if (finalAssessment == null)
            {
                finalAssessment = new FinalAssessmentSummary
                {
                    CaseId = model.CaseId,
                    CreatedAt = DateTimeExtensions.TaiwanTime
                };
                _context.FinalAssessmentSummaries.Add(finalAssessment);
            }

            finalAssessment.FqSummary = model.FqSummary;
            finalAssessment.HqSummary = model.HqSummary;
            finalAssessment.IqSummary = model.IqSummary;
            finalAssessment.EqSummary = model.EqSummary;
            finalAssessment.UpdatedAt = DateTimeExtensions.TaiwanTime;

            await _context.SaveChangesAsync();
            return (true, "步驟7完成，請繼續下一步");
        }
        catch (Exception ex)
        {
            return (false, $"儲存步驟7資料失敗：{ex.Message}");
        }
    }
}
