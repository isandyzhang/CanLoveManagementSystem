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
/// 社會工作服務內容服務 - 個案開案流程步驟2 (CaseSocialWorkerContent)
/// </summary>
public class SocialWorkerContentService
{
    private readonly CanLoveDbContext _context;
    private readonly OptionService _optionService;
    private readonly IMapper _mapper;

    public SocialWorkerContentService(CanLoveDbContext context, OptionService optionService, IMapper mapper)
    {
        _context = context;
        _optionService = optionService;
        _mapper = mapper;
    }

    /// <summary>
    /// 取得步驟2資料
    /// </summary>
    public async Task<SocialWorkerContentVM> GetStep2DataAsync(string caseId)
    {
        var socialWorkerContent = await _context.CaseSocialWorkerContents
            .FirstOrDefaultAsync(cswc => cswc.CaseId == caseId);

        // 使用 AutoMapper 自動轉換
        var viewModel = socialWorkerContent != null 
            ? _mapper.Map<SocialWorkerContentVM>(socialWorkerContent)
            : new SocialWorkerContentVM { CaseId = caseId };

        // 載入選項資料
        viewModel.ResidenceTypeOptions = await _optionService.GetResidenceTypeOptionsAsync();

        return viewModel;
    }

    /// <summary>
    /// 儲存步驟2資料
    /// </summary>
    public async Task<(bool Success, string Message)> SaveStep2DataAsync(SocialWorkerContentVM model)
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

            var socialWorkerContent = await _context.CaseSocialWorkerContents
                .FirstOrDefaultAsync(cswc => cswc.CaseId == model.CaseId);

            if (socialWorkerContent == null)
            {
                socialWorkerContent = new CaseSocialWorkerContent
                {
                    CaseId = model.CaseId,
                    OpeningId = opening.OpeningId,
                    CreatedAt = DateTimeExtensions.TaiwanTime
                };
                _context.CaseSocialWorkerContents.Add(socialWorkerContent);
            }

            // 使用 AutoMapper 自動更新
            _mapper.Map(model, socialWorkerContent);
            socialWorkerContent.UpdatedAt = DateTimeExtensions.TaiwanTime;

            await _context.SaveChangesAsync();
            return (true, "步驟2完成，請繼續下一步");
        }
        catch (Exception ex)
        {
            return (false, $"儲存步驟2資料失敗：{ex.Message}");
        }
    }
}

