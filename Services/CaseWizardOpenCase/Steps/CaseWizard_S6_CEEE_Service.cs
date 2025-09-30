using Microsoft.EntityFrameworkCore;
using CanLove_Backend.Data.Contexts;
using CanLove_Backend.Data.Models.CaseDetails;
using CanLove_Backend.Models.Mvc.ViewModels.CaseWizardOpenCase;
using AutoMapper;

namespace CanLove_Backend.Services.CaseWizardOpenCase.Steps;

/// <summary>
/// 個案開案流程步驟6服務 - 情緒評估 (CaseEqemotionalEvaluation)
/// </summary>
public class CaseWizard_S6_CEEE_Service
{
    private readonly CanLoveDbContext _context;
    private readonly IMapper _mapper;

    public CaseWizard_S6_CEEE_Service(CanLoveDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// 取得步驟6資料
    /// </summary>
    public async Task<CaseWizard_S6_CEEE_ViewModel> GetStep6DataAsync(string caseId)
    {
        var emotionalEvaluation = await _context.CaseEqemotionalEvaluations
            .FirstOrDefaultAsync(cee => cee.CaseId == caseId);

        // 🎯 使用 AutoMapper 自動轉換，原本需要 8+ 行手動對應，現在只需要 1 行！
        var viewModel = emotionalEvaluation != null 
            ? _mapper.Map<CaseWizard_S6_CEEE_ViewModel>(emotionalEvaluation)
            : new CaseWizard_S6_CEEE_ViewModel { CaseId = caseId };

        return viewModel;
    }

    /// <summary>
    /// 儲存步驟6資料
    /// </summary>
    public async Task<(bool Success, string Message)> SaveStep6DataAsync(CaseWizard_S6_CEEE_ViewModel model)
    {
        try
        {
            var emotionalEvaluation = await _context.CaseEqemotionalEvaluations
                .FirstOrDefaultAsync(cee => cee.CaseId == model.CaseId);

            if (emotionalEvaluation == null)
            {
                emotionalEvaluation = new CaseEqemotionalEvaluation
                {
                    CaseId = model.CaseId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.CaseEqemotionalEvaluations.Add(emotionalEvaluation);
            }

            emotionalEvaluation.EqQ1 = model.EqQ1;
            emotionalEvaluation.EqQ2 = model.EqQ2;
            emotionalEvaluation.EqQ3 = model.EqQ3;
            emotionalEvaluation.EqQ4 = model.EqQ4;
            emotionalEvaluation.EqQ5 = model.EqQ5;
            emotionalEvaluation.EqQ6 = model.EqQ6;
            emotionalEvaluation.EqQ7 = model.EqQ7;
            emotionalEvaluation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return (true, "步驟6完成，請繼續下一步");
        }
        catch (Exception ex)
        {
            return (false, $"儲存步驟6資料失敗：{ex.Message}");
        }
    }
}
