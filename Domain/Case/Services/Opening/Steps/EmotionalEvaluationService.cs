using Microsoft.EntityFrameworkCore;
using CanLove_Backend.Infrastructure.Data.Contexts;
using CanLove_Backend.Domain.Case.Models.Opening;
using CanLove_Backend.Domain.Case.ViewModels.Opening;
using CanLove_Backend.Core.Extensions;
using AutoMapper;

namespace CanLove_Backend.Domain.Case.Services.Opening.Steps;

/// <summary>
/// 情緒評估服務 - 個案開案流程步驟6 (CaseEqemotionalEvaluation)
/// </summary>
public class EmotionalEvaluationService
{
    private readonly CanLoveDbContext _context;
    private readonly IMapper _mapper;

    public EmotionalEvaluationService(CanLoveDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// 取得步驟6資料
    /// </summary>
    public async Task<EmotionalEvaluationVM> GetStep6DataAsync(string caseId)
    {
        var emotionalEvaluation = await _context.CaseEqemotionalEvaluations
            .FirstOrDefaultAsync(cee => cee.CaseId == caseId);

        // 使用 AutoMapper 自動轉換
        var viewModel = emotionalEvaluation != null 
            ? _mapper.Map<EmotionalEvaluationVM>(emotionalEvaluation)
            : new EmotionalEvaluationVM { CaseId = caseId };

        return viewModel;
    }

    /// <summary>
    /// 儲存步驟6資料
    /// </summary>
    public async Task<(bool Success, string Message)> SaveStep6DataAsync(EmotionalEvaluationVM model)
    {
        try
        {
            // 先獲取 CaseOpening 記錄以取得 OpeningId
            var opening = await _context.CaseOpenings
                .FirstOrDefaultAsync(o => o.CaseId == model.CaseId);
            
            if (opening == null)
            {
                return (false, "找不到對應的開案記錄，請先完成步驟0");
            }

            var emotionalEvaluation = await _context.CaseEqemotionalEvaluations
                .FirstOrDefaultAsync(cee => cee.CaseId == model.CaseId);

            if (emotionalEvaluation == null)
            {
                emotionalEvaluation = new CaseEqemotionalEvaluation
                {
                    CaseId = model.CaseId,
                    OpeningId = opening.OpeningId,
                    CreatedAt = DateTimeExtensions.TaiwanTime
                };
                _context.CaseEqemotionalEvaluations.Add(emotionalEvaluation);
            }

            // 使用 AutoMapper 自動更新
            _mapper.Map(model, emotionalEvaluation);
            emotionalEvaluation.UpdatedAt = DateTimeExtensions.TaiwanTime;

            await _context.SaveChangesAsync();
            return (true, "步驟6完成，請繼續下一步");
        }
        catch (Exception ex)
        {
            return (false, $"儲存步驟6資料失敗：{ex.Message}");
        }
    }
}

