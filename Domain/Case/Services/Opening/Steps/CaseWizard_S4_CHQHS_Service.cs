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
/// 個案開案流程步驟4服務 - 健康狀況評估 (CaseHqhealthStatus)
/// </summary>
public class CaseWizard_S4_CHQHS_Service
{
    private readonly CanLoveDbContext _context;
    private readonly OptionService _optionService;
    private readonly IMapper _mapper;

    public CaseWizard_S4_CHQHS_Service(CanLoveDbContext context, OptionService optionService, IMapper mapper)
    {
        _context = context;
        _optionService = optionService;
        _mapper = mapper;
    }

    /// <summary>
    /// 取得步驟4資料
    /// </summary>
    public async Task<CaseWizard_S4_CHQHS_ViewModel> GetStep4DataAsync(string caseId)
    {
        var healthStatus = await _context.CaseHqhealthStatuses
            .FirstOrDefaultAsync(chs => chs.CaseId == caseId);

        return new CaseWizard_S4_CHQHS_ViewModel
        {
            CaseId = caseId,
            CaregiverId = healthStatus?.CaregiverId ?? 0,
            CaregiverRoleValueId = healthStatus?.CaregiverRoleValueId ?? 0,
            CaregiverName = healthStatus?.CaregiverName,
            IsPrimary = healthStatus?.IsPrimary,
            EmotionalExpressionRating = healthStatus?.EmotionalExpressionRating,
            EmotionalExpressionNote = healthStatus?.EmotionalExpressionNote,
            HealthStatusRating = healthStatus?.HealthStatusRating,
            HealthStatusNote = healthStatus?.HealthStatusNote,
            ChildHealthStatusRating = healthStatus?.ChildHealthStatusRating,
            ChildHealthStatusNote = healthStatus?.ChildHealthStatusNote,
            ChildCareStatusRating = healthStatus?.ChildCareStatusRating,
            ChildCareStatusNote = healthStatus?.ChildCareStatusNote,
            // 載入選項資料
            CaregiverRoleOptions = await _optionService.GetCaregiverRoleOptionsAsync()
        };
    }

    /// <summary>
    /// 儲存步驟4資料
    /// </summary>
    public async Task<(bool Success, string Message)> SaveStep4DataAsync(CaseWizard_S4_CHQHS_ViewModel model)
    {
        try
        {
            var healthStatus = await _context.CaseHqhealthStatuses
                .FirstOrDefaultAsync(chs => chs.CaseId == model.CaseId);

            if (healthStatus == null)
            {
                healthStatus = new CaseHqhealthStatus
                {
                    CaseId = model.CaseId,
                    CaregiverId = model.CaregiverId,
                    CaregiverRoleValueId = model.CaregiverRoleValueId,
                    CreatedAt = DateTimeExtensions.TaiwanTime
                };
                _context.CaseHqhealthStatuses.Add(healthStatus);
            }

            healthStatus.CaregiverId = model.CaregiverId;
            healthStatus.CaregiverRoleValueId = model.CaregiverRoleValueId;
            healthStatus.CaregiverName = model.CaregiverName;
            healthStatus.IsPrimary = model.IsPrimary;
            healthStatus.EmotionalExpressionRating = model.EmotionalExpressionRating;
            healthStatus.EmotionalExpressionNote = model.EmotionalExpressionNote;
            healthStatus.HealthStatusRating = model.HealthStatusRating;
            healthStatus.HealthStatusNote = model.HealthStatusNote;
            healthStatus.ChildHealthStatusRating = model.ChildHealthStatusRating;
            healthStatus.ChildHealthStatusNote = model.ChildHealthStatusNote;
            healthStatus.ChildCareStatusRating = model.ChildCareStatusRating;
            healthStatus.ChildCareStatusNote = model.ChildCareStatusNote;
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
