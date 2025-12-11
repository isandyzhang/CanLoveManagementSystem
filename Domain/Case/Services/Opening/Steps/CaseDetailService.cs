using Microsoft.EntityFrameworkCore;
using CanLove_Backend.Infrastructure.Data.Contexts;
using CanLove_Backend.Domain.Case.Models.Opening;
using CanLove_Backend.Domain.Case.ViewModels.Opening;
using CanLove_Backend.Infrastructure.Options.Services;
using CanLove_Backend.Infrastructure.Storage.Blob;
using CanLove_Backend.Infrastructure.Storage.Encryption;
using CanLove_Backend.Domain.Case.Shared.Services;
using CanLove_Backend.Domain.Case.Exceptions;
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
        // 使用 AsNoTracking() 避免不必要的實體追蹤，提升讀取效能
        var caseDetail = await _context.CaseDetails
            .AsNoTracking()
            .FirstOrDefaultAsync(cd => cd.CaseId == caseId);

        // 首次填寫或者編輯個案詳細資料
        var viewModel = caseDetail != null 
            ? _mapper.Map<CaseDetailVM>(caseDetail)
            : new CaseDetailVM { CaseId = caseId };

        // 載入選項資料（使用並行查詢提升效能，並使用 OptionService 的快取機制）
        var contactRelationTask = _optionService.GetContactRelationOptionsAsync();
        var familyStructureTask = _optionService.GetFamilyStructureTypesAsync();
        var nationalityTask = _optionService.GetNationalitiesAsync();
        var marryStatusTask = _optionService.GetMarryStatusOptionsAsync();
        var educationLevelTask = _optionService.GetEducationLevelOptionsAsync();
        var sourceTask = _optionService.GetSourceOptionsAsync();
        var helpExperienceTask = _optionService.GetHelpExperienceOptionsAsync();

        // 並行執行所有查詢
        await Task.WhenAll(
            contactRelationTask,
            familyStructureTask,
            nationalityTask,
            marryStatusTask,
            educationLevelTask,
            sourceTask,
            helpExperienceTask
        );

        // 設定結果（ContactRelation 和 MainCaregiverRelation 使用相同選項）
        viewModel.ContactRelationOptions = await contactRelationTask;
        viewModel.MainCaregiverRelationOptions = viewModel.ContactRelationOptions;
        viewModel.FamilyStructureTypeOptions = await familyStructureTask;
        viewModel.NationalityOptions = await nationalityTask;
        viewModel.MarryStatusOptions = await marryStatusTask;
        viewModel.EducationLevelOptions = await educationLevelTask;
        viewModel.SourceOptions = await sourceTask;
        viewModel.HelpExperienceOptions = await helpExperienceTask;

        return viewModel;
    }

    /// <summary>
    /// 儲存步驟1資料 - 使用 AutoMapper
    /// </summary>
    public async Task<(bool Success, string Message)> SaveStep1DataAsync(CaseDetailVM model)
    {
        // 使用資料庫交易確保資料一致性
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 合併查詢：同時取得 CaseOpening 和 CaseDetail，減少資料庫往返次數
            var opening = await _context.CaseOpenings
                .FirstOrDefaultAsync(o => o.CaseId == model.CaseId);
            
            if (opening == null)
            {
                throw new CaseOpeningNotFoundException(model.CaseId);
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
            await transaction.CommitAsync();
            
            return (true, "步驟1完成，請繼續下一步");
        }
        catch (CaseOpeningException)
        {
            await transaction.RollbackAsync();
            // 重新拋出自訂例外，讓上層處理
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            // 包裝為自訂例外，提供使用者友善的錯誤訊息
            throw new CaseOpeningSaveException("步驟1", ex);
        }
    }
}

