using Microsoft.EntityFrameworkCore;
using CanLove_Backend.Infrastructure.Data.Contexts;
using CanLove_Backend.Domain.Case.Models.Basic;
using CanLove_Backend.Domain.Case.ViewModels.Opening;
using CanLove_Backend.Domain.Case.ViewModels.Basic;
using CanLove_Backend.Domain.Case.Services.Opening.Steps;
using CanLove_Backend.Infrastructure.Options.Services;
using CanLove_Backend.Infrastructure.Storage.Blob;
using CanLove_Backend.Infrastructure.Storage.Encryption;
using CanLove_Backend.Domain.Case.Shared.Services;
using CanLove_Backend.Domain.Staff.Services;
using CanLove_Backend.Core.Extensions;
using CaseEntity = CanLove_Backend.Domain.Case.Models.Basic.Case;

namespace CanLove_Backend.Domain.Case.Services.Opening;

/// <summary>
/// 個案開案流程主要協調服務
/// </summary>
public class CaseWizardOpenCaseService
{
    private readonly CanLoveDbContext _context;
    private readonly CaseDetailService _caseDetailService;
    private readonly SocialWorkerContentService _socialWorkerContentService;
    private readonly EconomicStatusService _economicStatusService;
    private readonly HealthStatusService _healthStatusService;
    private readonly AcademicPerformanceService _academicPerformanceService;
    private readonly EmotionalEvaluationService _emotionalEvaluationService;
    private readonly FinalAssessmentService _finalAssessmentService;
    private readonly DataEncryptionService _encryptionService;

    public CaseWizardOpenCaseService(
        CanLoveDbContext context,
        CaseDetailService caseDetailService,
        SocialWorkerContentService socialWorkerContentService,
        EconomicStatusService economicStatusService,
        HealthStatusService healthStatusService,
        AcademicPerformanceService academicPerformanceService,
        EmotionalEvaluationService emotionalEvaluationService,
        FinalAssessmentService finalAssessmentService,
        DataEncryptionService encryptionService)
    {
        _context = context;
        _caseDetailService = caseDetailService;
        _socialWorkerContentService = socialWorkerContentService;
        _economicStatusService = economicStatusService;
        _healthStatusService = healthStatusService;
        _academicPerformanceService = academicPerformanceService;
        _emotionalEvaluationService = emotionalEvaluationService;
        _finalAssessmentService = finalAssessmentService;
        _encryptionService = encryptionService;
    }

    /// <summary>
    /// 建立新個案
    /// </summary>
    public async Task<(bool Success, string Message)> CreateCaseAsync(CaseCreateViewModel model)
    {
        try
        {
            // 檢查個案編號是否已存在
            var existingCase = await _context.Cases.FindAsync(model.Case.CaseId);
            if (existingCase != null)
            {
                return (false, "此個案編號已存在，請使用其他編號");
            }

            // 建立新個案
            var newCase = new CaseEntity
            {
                CaseId = model.Case.CaseId,
                Name = model.Case.Name,
                AssessmentDate = model.Case.AssessmentDate,
                Gender = model.Case.Gender,
                SchoolId = model.Case.SchoolId,
                BirthDate = model.Case.BirthDate,
                // 加密身分證字號
                IdNumber = !string.IsNullOrWhiteSpace(model.Case.IdNumber) 
                    ? _encryptionService.Encrypt(model.Case.IdNumber) 
                    : string.Empty,
                Address = model.Case.Address,
                CityId = model.Case.CityId,
                DistrictId = model.Case.DistrictId,
                Phone = model.Case.Phone,
                Email = model.Case.Email,
                Status = "Draft", // 預設為草稿
                Deleted = false,
                CreatedAt = DateTimeExtensions.TaiwanTime,
                UpdatedAt = DateTimeExtensions.TaiwanTime
            };

            _context.Cases.Add(newCase);
            await _context.SaveChangesAsync();

            return (true, "個案建立成功！現在開始填寫詳細資料");
        }
        catch (Exception ex)
        {
            return (false, $"建立個案失敗：{ex.Message}");
        }
    }

    /// <summary>
    /// 取得步驟1資料
    /// </summary>
    public async Task<CaseDetailVM> GetStep1DataAsync(string caseId)
    {
        return await _caseDetailService.GetStep1DataAsync(caseId);
    }

    /// <summary>
    /// 儲存步驟1資料
    /// </summary>
    public async Task<(bool Success, string Message)> SaveStep1DataAsync(CaseDetailVM model)
    {
        return await _caseDetailService.SaveStep1DataAsync(model);
    }

    /// <summary>
    /// 取得步驟2資料
    /// </summary>
    public async Task<SocialWorkerContentVM> GetStep2DataAsync(string caseId)
    {
        return await _socialWorkerContentService.GetStep2DataAsync(caseId);
    }

    /// <summary>
    /// 儲存步驟2資料
    /// </summary>
    public async Task<(bool Success, string Message)> SaveStep2DataAsync(SocialWorkerContentVM model)
    {
        return await _socialWorkerContentService.SaveStep2DataAsync(model);
    }

    /// <summary>
    /// 取得步驟3資料
    /// </summary>
    public async Task<EconomicStatusVM> GetStep3DataAsync(string caseId)
    {
        return await _economicStatusService.GetStep3DataAsync(caseId);
    }

    /// <summary>
    /// 儲存步驟3資料
    /// </summary>
    public async Task<(bool Success, string Message)> SaveStep3DataAsync(EconomicStatusVM model)
    {
        return await _economicStatusService.SaveStep3DataAsync(model);
    }

    /// <summary>
    /// 取得步驟4資料
    /// </summary>
    public async Task<HealthStatusVM> GetStep4DataAsync(string caseId)
    {
        return await _healthStatusService.GetStep4DataAsync(caseId);
    }

    /// <summary>
    /// 儲存步驟4資料
    /// </summary>
    public async Task<(bool Success, string Message)> SaveStep4DataAsync(HealthStatusVM model)
    {
        return await _healthStatusService.SaveStep4DataAsync(model);
    }

    /// <summary>
    /// 取得步驟5資料
    /// </summary>
    public async Task<AcademicPerformanceVM> GetStep5DataAsync(string caseId)
    {
        return await _academicPerformanceService.GetStep5DataAsync(caseId);
    }

    /// <summary>
    /// 儲存步驟5資料
    /// </summary>
    public async Task<(bool Success, string Message)> SaveStep5DataAsync(AcademicPerformanceVM model)
    {
        return await _academicPerformanceService.SaveStep5DataAsync(model);
    }

    /// <summary>
    /// 取得步驟6資料
    /// </summary>
    public async Task<EmotionalEvaluationVM> GetStep6DataAsync(string caseId)
    {
        return await _emotionalEvaluationService.GetStep6DataAsync(caseId);
    }

    /// <summary>
    /// 儲存步驟6資料
    /// </summary>
    public async Task<(bool Success, string Message)> SaveStep6DataAsync(EmotionalEvaluationVM model)
    {
        return await _emotionalEvaluationService.SaveStep6DataAsync(model);
    }

    /// <summary>
    /// 取得步驟7資料
    /// </summary>
    public async Task<FinalAssessmentVM> GetStep7DataAsync(string caseId)
    {
        return await _finalAssessmentService.GetStep7DataAsync(caseId);
    }

    /// <summary>
    /// 儲存步驟7資料
    /// </summary>
    public async Task<(bool Success, string Message)> SaveStep7DataAsync(FinalAssessmentVM model)
    {
        return await _finalAssessmentService.SaveStep7DataAsync(model);
    }

}
