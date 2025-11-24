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
    private readonly CaseWizard_S1_CD_Service _step1Service;
    private readonly CaseWizard_S2_CSWC_Service _step2Service;
    private readonly CaseWizard_S3_CFQES_Service _step3Service;
    private readonly CaseWizard_S4_CHQHS_Service _step4Service;
    private readonly CaseWizard_S5_CIQAP_Service _step5Service;
    private readonly CaseWizard_S6_CEEE_Service _step6Service;
    private readonly CaseWizard_S7_FAS_Service _step7Service;
    private readonly DataEncryptionService _encryptionService;

    public CaseWizardOpenCaseService(
        CanLoveDbContext context,
        CaseWizard_S1_CD_Service step1Service,
        CaseWizard_S2_CSWC_Service step2Service,
        CaseWizard_S3_CFQES_Service step3Service,
        CaseWizard_S4_CHQHS_Service step4Service,
        CaseWizard_S5_CIQAP_Service step5Service,
        CaseWizard_S6_CEEE_Service step6Service,
        CaseWizard_S7_FAS_Service step7Service,
        DataEncryptionService encryptionService)
    {
        _context = context;
        _step1Service = step1Service;
        _step2Service = step2Service;
        _step3Service = step3Service;
        _step4Service = step4Service;
        _step5Service = step5Service;
        _step6Service = step6Service;
        _step7Service = step7Service;
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
    public async Task<CaseWizard_S1_CD_ViewModel> GetStep1DataAsync(string caseId)
    {
        return await _step1Service.GetStep1DataAsync(caseId);
    }

    /// <summary>
    /// 儲存步驟1資料
    /// </summary>
    public async Task<(bool Success, string Message)> SaveStep1DataAsync(CaseWizard_S1_CD_ViewModel model)
    {
        return await _step1Service.SaveStep1DataAsync(model);
    }

    /// <summary>
    /// 取得步驟2資料
    /// </summary>
    public async Task<CaseWizard_S2_CSWC_ViewModel> GetStep2DataAsync(string caseId)
    {
        return await _step2Service.GetStep2DataAsync(caseId);
    }

    /// <summary>
    /// 儲存步驟2資料
    /// </summary>
    public async Task<(bool Success, string Message)> SaveStep2DataAsync(CaseWizard_S2_CSWC_ViewModel model)
    {
        return await _step2Service.SaveStep2DataAsync(model);
    }

    /// <summary>
    /// 取得步驟4資料
    /// </summary>
    public async Task<CaseWizard_S4_CHQHS_ViewModel> GetStep4DataAsync(string caseId)
    {
        return await _step4Service.GetStep4DataAsync(caseId);
    }

    /// <summary>
    /// 儲存步驟4資料
    /// </summary>
    public async Task<(bool Success, string Message)> SaveStep4DataAsync(CaseWizard_S4_CHQHS_ViewModel model)
    {
        return await _step4Service.SaveStep4DataAsync(model);
    }

    /// <summary>
    /// 取得步驟3資料
    /// </summary>
    public async Task<CaseWizard_S3_CFQES_ViewModel> GetStep3DataAsync(string caseId)
    {
        return await _step3Service.GetStep3DataAsync(caseId);
    }

    /// <summary>
    /// 儲存步驟3資料
    /// </summary>
    public async Task<(bool Success, string Message)> SaveStep3DataAsync(CaseWizard_S3_CFQES_ViewModel model)
    {
        return await _step3Service.SaveStep3DataAsync(model);
    }

    /// <summary>
    /// 取得步驟5資料
    /// </summary>
    public async Task<CaseWizard_S5_CIQAP_ViewModel> GetStep5DataAsync(string caseId)
    {
        return await _step5Service.GetStep5DataAsync(caseId);
    }

    /// <summary>
    /// 儲存步驟5資料
    /// </summary>
    public async Task<(bool Success, string Message)> SaveStep5DataAsync(CaseWizard_S5_CIQAP_ViewModel model)
    {
        return await _step5Service.SaveStep5DataAsync(model);
    }

    /// <summary>
    /// 取得步驟6資料
    /// </summary>
    public async Task<CaseWizard_S6_CEEE_ViewModel> GetStep6DataAsync(string caseId)
    {
        return await _step6Service.GetStep6DataAsync(caseId);
    }

    /// <summary>
    /// 儲存步驟6資料
    /// </summary>
    public async Task<(bool Success, string Message)> SaveStep6DataAsync(CaseWizard_S6_CEEE_ViewModel model)
    {
        return await _step6Service.SaveStep6DataAsync(model);
    }

    /// <summary>
    /// 取得步驟7資料
    /// </summary>
    public async Task<CaseWizard_S7_FAS_ViewModel> GetStep7DataAsync(string caseId)
    {
        return await _step7Service.GetStep7DataAsync(caseId);
    }

    /// <summary>
    /// 儲存步驟7資料
    /// </summary>
    public async Task<(bool Success, string Message)> SaveStep7DataAsync(CaseWizard_S7_FAS_ViewModel model)
    {
        return await _step7Service.SaveStep7DataAsync(model);
    }

}
