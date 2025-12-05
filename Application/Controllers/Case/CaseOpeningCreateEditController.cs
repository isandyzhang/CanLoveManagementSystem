using Microsoft.AspNetCore.Mvc;
using CanLove_Backend.Infrastructure.Data.Contexts;
using CanLove_Backend.Domain.Case.Models.Opening;
using CanLove_Backend.Domain.Case.ViewModels.Basic;
using CanLove_Backend.Domain.Case.ViewModels.Opening;
using CanLove_Backend.Domain.Case.Services.Opening.Steps;
using CanLove_Backend.Infrastructure.Storage.Blob;
using CanLove_Backend.Infrastructure.Storage.Encryption;
using CanLove_Backend.Infrastructure.Options.Services;
using CanLove_Backend.Domain.Case.Shared.Services;
using CanLove_Backend.Domain.Staff.Services;
using Microsoft.EntityFrameworkCore;
using CanLove_Backend.Core.Extensions;

namespace CanLove_Backend.Application.Controllers.Case;

/// <summary>
/// 個案開案步驟表單控制器（Create/Edit 用 Wizard）
/// </summary>
public class CaseOpeningCreateEditController : Controller
{
    private readonly CanLoveDbContext _context;
    private readonly OptionService _optionService;
    private readonly CaseDetailService _caseDetailService;
    private readonly SocialWorkerContentService _socialWorkerContentService;
    private readonly EconomicStatusService _economicStatusService;
    private readonly HealthStatusService _healthStatusService;
    private readonly AcademicPerformanceService _academicPerformanceService;
    private readonly EmotionalEvaluationService _emotionalEvaluationService;
    private readonly FinalAssessmentService _finalAssessmentService;

    public CaseOpeningCreateEditController(
        CanLoveDbContext context,
        OptionService optionService,
        CaseDetailService caseDetailService,
        SocialWorkerContentService socialWorkerContentService,
        EconomicStatusService economicStatusService,
        HealthStatusService healthStatusService,
        AcademicPerformanceService academicPerformanceService,
        EmotionalEvaluationService emotionalEvaluationService,
        FinalAssessmentService finalAssessmentService)
    {
        _context = context;
        _optionService = optionService;
        _caseDetailService = caseDetailService;
        _socialWorkerContentService = socialWorkerContentService;
        _economicStatusService = economicStatusService;
        _healthStatusService = healthStatusService;
        _academicPerformanceService = academicPerformanceService;
        _emotionalEvaluationService = emotionalEvaluationService;
        _finalAssessmentService = finalAssessmentService;
    }

    // 以下內容與目前 CaseOpeningController 中的 Wizard 區塊相同：
    // LoadCaseInfoAsync / ValidateCaseExistsAsync / GetCaseOpeningAsync / GetActionNameByMode / ValidateMode /
    // ValidateCaseId / SetCaseInfoAsync / GetModeText / Create / GetStepActionResult /
    // Create(step) / Edit(step) / Step0~Step7 GET/POST / PreviousStep
    //
    // 為了避免重複貼整份大檔，後續實作時可以逐步將 Wizard 相關方法從 CaseOpeningController 搬移到此處，
    // 並讓 View 的 asp-controller 指向 CaseOpeningCreateEditController。
}


