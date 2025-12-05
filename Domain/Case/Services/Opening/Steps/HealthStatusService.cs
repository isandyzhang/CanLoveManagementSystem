using Microsoft.EntityFrameworkCore;
using CanLove_Backend.Infrastructure.Data.Contexts;
using CanLove_Backend.Domain.Case.Models.Opening;
using CanLove_Backend.Domain.Case.ViewModels.Opening;
using CanLove_Backend.Infrastructure.Options.Services;
using CanLove_Backend.Infrastructure.Storage.Blob;
using CanLove_Backend.Infrastructure.Storage.Encryption;
using CanLove_Backend.Domain.Case.Shared.Services;
using CanLove_Backend.Domain.Staff.Services;
using CanLove_Backend.Core.Extensions;
using AutoMapper;

namespace CanLove_Backend.Domain.Case.Services.Opening.Steps;

/// <summary>
/// 健康狀況評估服務 - 個案開案流程步驟4 (CaseHqhealthStatus)
/// </summary>
public class HealthStatusService
{
    private readonly CanLoveDbContext _context;
    private readonly OptionService _optionService;
    private readonly IMapper _mapper;

    public HealthStatusService(CanLoveDbContext context, OptionService optionService, IMapper mapper)
    {
        _context = context;
        _optionService = optionService;
        _mapper = mapper;
    }

    /// <summary>
    /// 取得步驟4資料
    /// </summary>
    public async Task<HealthStatusVM> GetStep4DataAsync(string caseId)
    {
        var healthStatus = await _context.CaseHqhealthStatuses
            .FirstOrDefaultAsync(chs => chs.CaseId == caseId);

        // 使用 AutoMapper 自動轉換（如果映射配置存在）
        var viewModel = healthStatus != null 
            ? _mapper.Map<HealthStatusVM>(healthStatus)
            : new HealthStatusVM { CaseId = caseId };

        // 載入選項資料
        viewModel.CaregiverRoleOptions = await _optionService.GetCaregiverRoleOptionsAsync();

        return viewModel;
    }

    /// <summary>
    /// 儲存步驟4資料
    /// </summary>
    public async Task<(bool Success, string Message)> SaveStep4DataAsync(HealthStatusVM model)
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

            var healthStatus = await _context.CaseHqhealthStatuses
                .FirstOrDefaultAsync(chs => chs.CaseId == model.CaseId);

            if (healthStatus == null)
            {
                healthStatus = new CaseHqhealthStatus
                {
                    CaseId = model.CaseId,
                    OpeningId = opening.OpeningId,
                    CaregiverId = model.CaregiverId,
                    CaregiverRoleValueId = model.CaregiverRoleValueId,
                    CreatedAt = DateTimeExtensions.TaiwanTime
                };
                _context.CaseHqhealthStatuses.Add(healthStatus);
            }

            // 使用 AutoMapper 自動更新（如果映射配置存在）
            _mapper.Map(model, healthStatus);
            healthStatus.UpdatedAt = DateTimeExtensions.TaiwanTime;

            await _context.SaveChangesAsync();
            return (true, "步驟4完成，請繼續下一步");
        }
        catch (Exception ex)
        {
            return (false, $"儲存步驟4資料失敗：{ex.Message}");
        }
    }
}

