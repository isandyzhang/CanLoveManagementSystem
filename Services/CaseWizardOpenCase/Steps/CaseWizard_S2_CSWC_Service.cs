using Microsoft.EntityFrameworkCore;
using CanLove_Backend.Data.Contexts;
using CanLove_Backend.Data.Models.CaseDetails;
using CanLove_Backend.Models.Mvc.ViewModels.CaseWizardOpenCase;
using CanLove_Backend.Services.Shared;
using AutoMapper;


namespace CanLove_Backend.Services.CaseWizardOpenCase.Steps;

/// <summary>
/// 個案開案流程步驟2服務 - 社會工作服務內容 (CaseSocialWorkerContent)
/// </summary>
public class CaseWizard_S2_CSWC_Service
{
    private readonly CanLoveDbContext _context;
    private readonly OptionService _optionService;
    private readonly IMapper _mapper;

    public CaseWizard_S2_CSWC_Service(CanLoveDbContext context, OptionService optionService, IMapper mapper)
    {
        _context = context;
        _optionService = optionService;
        _mapper = mapper;
    }

    /// <summary>
    /// 取得步驟2資料
    /// </summary>
    public async Task<CaseWizard_S2_CSWC_ViewModel> GetStep2DataAsync(string caseId)
    {
        var socialWorkerContent = await _context.CaseSocialWorkerContents
            .FirstOrDefaultAsync(cswc => cswc.CaseId == caseId);

        // 🎯 使用 AutoMapper 自動轉換，原本需要 15+ 行手動對應，現在只需要 1 行！
        var viewModel = socialWorkerContent != null 
            ? _mapper.Map<CaseWizard_S2_CSWC_ViewModel>(socialWorkerContent)
            : new CaseWizard_S2_CSWC_ViewModel { CaseId = caseId };

        // 載入選項資料
        viewModel.ResidenceTypeOptions = await _optionService.GetResidenceTypeOptionsAsync();

        return viewModel;
    }

    /// <summary>
    /// 儲存步驟2資料
    /// </summary>
    public async Task<(bool Success, string Message)> SaveStep2DataAsync(CaseWizard_S2_CSWC_ViewModel model)
    {
        try
        {
            var socialWorkerContent = await _context.CaseSocialWorkerContents
                .FirstOrDefaultAsync(cswc => cswc.CaseId == model.CaseId);

            if (socialWorkerContent == null)
            {
                socialWorkerContent = new CaseSocialWorkerContent
                {
                    CaseId = model.CaseId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.CaseSocialWorkerContents.Add(socialWorkerContent);
            }

            socialWorkerContent.FamilyTreeImg = model.FamilyTreeImg;
            socialWorkerContent.ResidenceTypeValueId = model.ResidenceTypeValueId;
            socialWorkerContent.HouseCleanlinessRating = model.HouseCleanlinessRating;
            socialWorkerContent.HouseCleanlinessNote = model.HouseCleanlinessNote;
            socialWorkerContent.HouseSafetyRating = model.HouseSafetyRating;
            socialWorkerContent.HouseSafetyNote = model.HouseSafetyNote;
            socialWorkerContent.CaregiverChildInteractionRating = model.CaregiverChildInteractionRating;
            socialWorkerContent.CaregiverChildInteractionNote = model.CaregiverChildInteractionNote;
            socialWorkerContent.CaregiverFamilyInteractionRating = model.CaregiverFamilyInteractionRating;
            socialWorkerContent.CaregiverFamilyInteractionNote = model.CaregiverFamilyInteractionNote;
            socialWorkerContent.FamilyResourceAbilityRating = model.FamilyResourceAbilityRating;
            socialWorkerContent.FamilyResourceAbilityNote = model.FamilyResourceAbilityNote;
            socialWorkerContent.FamilySocialSupportRating = model.FamilySocialSupportRating;
            socialWorkerContent.FamilySocialSupportNote = model.FamilySocialSupportNote;
            socialWorkerContent.SpecialCircumstancesDescription = model.SpecialCircumstancesDescription;
            socialWorkerContent.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return (true, "步驟2完成，請繼續下一步");
        }
        catch (Exception ex)
        {
            return (false, $"儲存步驟2資料失敗：{ex.Message}");
        }
    }
}
