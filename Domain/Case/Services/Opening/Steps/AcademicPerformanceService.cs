using Microsoft.EntityFrameworkCore;
using CanLove_Backend.Infrastructure.Data.Contexts;
using CanLove_Backend.Domain.Case.Models.Opening;
using CanLove_Backend.Domain.Case.ViewModels.Opening;
using CanLove_Backend.Core.Extensions;
using AutoMapper;

namespace CanLove_Backend.Domain.Case.Services.Opening.Steps;

/// <summary>
/// 學業表現評估服務 - 個案開案流程步驟5 (CaseIqacademicPerformance)
/// </summary>
public class AcademicPerformanceService
{
    private readonly CanLoveDbContext _context;
    private readonly IMapper _mapper;

    public AcademicPerformanceService(CanLoveDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// 取得步驟5資料
    /// </summary>
    public async Task<AcademicPerformanceVM> GetStep5DataAsync(string caseId)
    {
        var academicPerformance = await _context.CaseIqacademicPerformances
            .FirstOrDefaultAsync(cap => cap.CaseId == caseId);

        // 使用 AutoMapper 自動轉換
        var viewModel = academicPerformance != null 
            ? _mapper.Map<AcademicPerformanceVM>(academicPerformance)
            : new AcademicPerformanceVM { CaseId = caseId };

        return viewModel;
    }

    /// <summary>
    /// 儲存步驟5資料
    /// </summary>
    public async Task<(bool Success, string Message)> SaveStep5DataAsync(AcademicPerformanceVM model)
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

            var academicPerformance = await _context.CaseIqacademicPerformances
                .FirstOrDefaultAsync(cap => cap.CaseId == model.CaseId);

            if (academicPerformance == null)
            {
                academicPerformance = new CaseIqacademicPerformance
                {
                    CaseId = model.CaseId,
                    OpeningId = opening.OpeningId,
                    CreatedAt = DateTimeExtensions.TaiwanTime
                };
                _context.CaseIqacademicPerformances.Add(academicPerformance);
            }

            // 使用 AutoMapper 自動更新
            _mapper.Map(model, academicPerformance);
            academicPerformance.UpdatedAt = DateTimeExtensions.TaiwanTime;

            await _context.SaveChangesAsync();
            return (true, "步驟5完成，請繼續下一步");
        }
        catch (Exception ex)
        {
            return (false, $"儲存步驟5資料失敗：{ex.Message}");
        }
    }
}

