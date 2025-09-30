using Microsoft.EntityFrameworkCore;
using CanLove_Backend.Data.Contexts;
using CanLove_Backend.Data.Models.CaseDetails;
using CanLove_Backend.Models.Mvc.ViewModels.CaseWizardOpenCase;
using CanLove_Backend.Services.Shared;
using AutoMapper;

namespace CanLove_Backend.Services.CaseWizardOpenCase.Steps;

/// <summary>
/// 個案開案流程步驟1服務 - 個案詳細資料 (CaseDetail) - 使用 AutoMapper 改善版
/// </summary>
public class CaseWizard_S1_CD_Service
{
    private readonly CanLoveDbContext _context;
    private readonly OptionService _optionService;
    private readonly IMapper _mapper;

    public CaseWizard_S1_CD_Service(CanLoveDbContext context, OptionService optionService, IMapper mapper)
    {
        _context = context;
        _optionService = optionService;
        _mapper = mapper;
    }

    /// <summary>
    /// 取得步驟1資料 - 使用 AutoMapper 改善
    /// </summary>
    public async Task<CaseWizard_S1_CD_ViewModel> GetStep1DataAsync(string caseId)
    {
        var caseDetail = await _context.CaseDetails
            .FirstOrDefaultAsync(cd => cd.CaseId == caseId);

        // 首次填寫或者編輯個案詳細資料
        var viewModel = caseDetail != null 
            ? _mapper.Map<CaseWizard_S1_CD_ViewModel>(caseDetail)
            : new CaseWizard_S1_CD_ViewModel { CaseId = caseId };

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
    /// 儲存步驟1資料 - 使用 AutoMapper 改善
    /// </summary>
    public async Task<(bool Success, string Message)> SaveStep1DataAsync(CaseWizard_S1_CD_ViewModel model)
    {
        try
        {
            var caseDetail = await _context.CaseDetails
                .FirstOrDefaultAsync(cd => cd.CaseId == model.CaseId);

            if (caseDetail == null)
            {
                // 🎯 原本需要手動設定 20+ 個屬性，現在只需要 1 行！
                caseDetail = _mapper.Map<CaseDetail>(model);
                caseDetail.CreatedAt = DateTime.UtcNow;
                _context.CaseDetails.Add(caseDetail);
            }
            else
            {
                // 🎯 原本需要手動更新 20+ 個屬性，現在只需要 1 行！
                _mapper.Map(model, caseDetail);
                caseDetail.UpdatedAt = DateTime.UtcNow;
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
