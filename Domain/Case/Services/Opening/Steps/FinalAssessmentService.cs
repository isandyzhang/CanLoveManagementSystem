using Microsoft.EntityFrameworkCore;
using CanLove_Backend.Infrastructure.Data.Contexts;
using CanLove_Backend.Domain.Case.Models.Opening;
using CanLove_Backend.Domain.Case.ViewModels.Opening;
using CanLove_Backend.Core.Extensions;
using AutoMapper;

namespace CanLove_Backend.Domain.Case.Services.Opening.Steps;

/// <summary>
/// 最後評估表服務 - 個案開案流程步驟7 (FinalAssessmentSummary)
/// </summary>
public class FinalAssessmentService
{
    private readonly CanLoveDbContext _context;
    private readonly IMapper _mapper;

    public FinalAssessmentService(CanLoveDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// 取得步驟7資料
    /// </summary>
    public async Task<FinalAssessmentVM> GetStep7DataAsync(string caseId)
    {
        var finalAssessment = await _context.FinalAssessmentSummaries
            .FirstOrDefaultAsync(fas => fas.CaseId == caseId);

        // 使用 AutoMapper 自動轉換
        var viewModel = finalAssessment != null 
            ? _mapper.Map<FinalAssessmentVM>(finalAssessment)
            : new FinalAssessmentVM { CaseId = caseId };

        return viewModel;
    }

    /// <summary>
    /// 儲存步驟7資料
    /// </summary>
    public async Task<(bool Success, string Message)> SaveStep7DataAsync(FinalAssessmentVM model)
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

            var finalAssessment = await _context.FinalAssessmentSummaries
                .FirstOrDefaultAsync(fas => fas.CaseId == model.CaseId);

            if (finalAssessment == null)
            {
                finalAssessment = new FinalAssessmentSummary
                {
                    CaseId = model.CaseId,
                    OpeningId = opening.OpeningId,
                    CreatedAt = DateTimeExtensions.TaiwanTime
                };
                _context.FinalAssessmentSummaries.Add(finalAssessment);
            }

            // 使用 AutoMapper 自動更新
            _mapper.Map(model, finalAssessment);
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

