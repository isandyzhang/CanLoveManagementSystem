using Microsoft.EntityFrameworkCore;
using CanLove_Backend.Infrastructure.Data.Contexts;
using CanLove_Backend.Domain.Case.Models.Opening;
using CanLove_Backend.Domain.Case.ViewModels.Opening;
using CanLove_Backend.Core.Extensions;
using AutoMapper;

namespace CanLove_Backend.Domain.Case.Services.Opening.Steps;

/// <summary>
/// 經濟狀況評估服務 - 個案開案流程步驟3 (CaseFQeconomicStatus)
/// </summary>
public class EconomicStatusService
{
    private readonly CanLoveDbContext _context;
    private readonly IMapper _mapper;

    public EconomicStatusService(CanLoveDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// 取得步驟3資料
    /// </summary>
    public async Task<EconomicStatusVM> GetStep3DataAsync(string caseId)
    {
        var economicStatus = await _context.CaseFqeconomicStatuses
            .FirstOrDefaultAsync(cfs => cfs.CaseId == caseId);

        // 使用 AutoMapper 自動轉換
        var viewModel = economicStatus != null 
            ? _mapper.Map<EconomicStatusVM>(economicStatus)
            : new EconomicStatusVM { CaseId = caseId };

        return viewModel;
    }

    /// <summary>
    /// 儲存步驟3資料
    /// </summary>
    public async Task<(bool Success, string Message)> SaveStep3DataAsync(EconomicStatusVM model)
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

            var economicStatus = await _context.CaseFqeconomicStatuses
                .FirstOrDefaultAsync(cfs => cfs.CaseId == model.CaseId);

            if (economicStatus == null)
            {
                economicStatus = new CaseFqeconomicStatus
                {
                    CaseId = model.CaseId,
                    OpeningId = opening.OpeningId,
                    CreatedAt = DateTimeExtensions.TaiwanTime
                };
                _context.CaseFqeconomicStatuses.Add(economicStatus);
            }

            // 使用 AutoMapper 自動更新
            _mapper.Map(model, economicStatus);
            economicStatus.UpdatedAt = DateTimeExtensions.TaiwanTime;

            await _context.SaveChangesAsync();
            return (true, "步驟3完成，請繼續下一步");
        }
        catch (Exception ex)
        {
            return (false, $"儲存步驟3資料失敗：{ex.Message}");
        }
    }
}

