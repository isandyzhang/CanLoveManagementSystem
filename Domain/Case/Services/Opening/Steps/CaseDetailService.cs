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
/// 個案詳細資料服務 - 個案開案流程步驟1 (CaseDetail)
/// </summary>
public class CaseDetailService
{
    private readonly CanLoveDbContext _context;
    private readonly OptionService _optionService;
    private readonly IMapper _mapper;

    public CaseDetailService(CanLoveDbContext context, OptionService optionService, IMapper mapper)
    {
        _context = context;
        _optionService = optionService;
        _mapper = mapper;
    }

    /// <summary>
    /// 取得步驟1資料 - 使用 AutoMapper
    /// </summary>
    public async Task<CaseDetailVM> GetStep1DataAsync(string caseId)
    {
        var caseDetail = await _context.CaseDetails
            .FirstOrDefaultAsync(cd => cd.CaseId == caseId);

        // 首次填寫或者編輯個案詳細資料
        var viewModel = caseDetail != null 
            ? _mapper.Map<CaseDetailVM>(caseDetail)
            : new CaseDetailVM { CaseId = caseId };

        // 載入選項資料（這部分還是需要手動處理）
        viewModel.ContactRelationOptions = await _optionService.GetContactRelationOptionsAsync();
        viewModel.MainCaregiverRelationOptions = await _optionService.GetContactRelationOptionsAsync();
        viewModel.FamilyStructureTypeOptions = await _context.FamilyStructureTypes.OrderBy(f => f.StructureTypeId).ToListAsync();
        viewModel.NationalityOptions = await _context.Nationalities.OrderBy(n => n.NationalityId).ToListAsync();
        viewModel.MarryStatusOptions = await _optionService.GetMarryStatusOptionsAsync();
        viewModel.EducationLevelOptions = await _optionService.GetEducationLevelOptionsAsync();
        viewModel.SourceOptions = await _optionService.GetSourceOptionsAsync();
        viewModel.HelpExperienceOptions = await _optionService.GetHelpExperienceOptionsAsync();

        return viewModel;
    }

    /// <summary>
    /// 儲存步驟1資料 - 使用 AutoMapper
    /// </summary>
    public async Task<(bool Success, string Message)> SaveStep1DataAsync(CaseDetailVM model)
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

            var caseDetail = await _context.CaseDetails
                .FirstOrDefaultAsync(cd => cd.CaseId == model.CaseId);

            if (caseDetail == null)
            {
                // 使用 AutoMapper 自動轉換
                caseDetail = _mapper.Map<CaseDetail>(model);
                caseDetail.OpeningId = opening.OpeningId;
                caseDetail.CreatedAt = DateTimeExtensions.TaiwanTime;
                _context.CaseDetails.Add(caseDetail);
            }
            else
            {
                // 使用 AutoMapper 自動更新
                _mapper.Map(model, caseDetail);
                caseDetail.UpdatedAt = DateTimeExtensions.TaiwanTime;
            }

            await _context.SaveChangesAsync();
            return (true, "步驟1完成，請繼續下一步");
        }
        catch (Exception ex)
        {
            return (false, $"儲存步驟1資料失敗：{ex.Message}");
        }
    }
}

