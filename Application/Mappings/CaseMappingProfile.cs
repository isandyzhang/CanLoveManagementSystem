using AutoMapper;
using CanLove_Backend.Domain.Case.Models.Basic;
using CanLove_Backend.Domain.Case.Models.Opening;
using CanLove_Backend.Domain.Case.ViewModels.Opening;
using CanLove_Backend.Core.Extensions;

namespace CanLove_Backend.Application.Mappings;

/// <summary>
/// AutoMapper 對應設定檔
/// </summary>
public class CaseMappingProfile : Profile
{
    public CaseMappingProfile()
    {
        // 1. Case 到 CaseResponse 的對應（已移除 API，此映射暫時保留以備未來使用）
        // CreateMap<Case, CaseResponse>()
        //     .ForMember(dest => dest.SchoolName, 
        //               opt => opt.MapFrom(src => src.School != null ? src.School.SchoolName : null))
        //     .ForMember(dest => dest.CityName, 
        //               opt => opt.MapFrom(src => src.City != null ? src.City.CityName : null))
        //     .ForMember(dest => dest.CreatedAt, 
        //               opt => opt.MapFrom(src => src.CreatedAt ?? DateTimeExtensions.TaiwanTime));

        // 2. CaseDetail 到 CaseDetailVM 的對應
        CreateMap<CaseDetail, CaseDetailVM>()
            // 選項資料不包含在對應中，需要手動載入
            .ForMember(dest => dest.ContactRelationOptions, opt => opt.Ignore())
            .ForMember(dest => dest.MainCaregiverRelationOptions, opt => opt.Ignore())
            .ForMember(dest => dest.FamilyStructureTypeOptions, opt => opt.Ignore())
            .ForMember(dest => dest.NationalityOptions, opt => opt.Ignore())
            .ForMember(dest => dest.MarryStatusOptions, opt => opt.Ignore())
            .ForMember(dest => dest.EducationLevelOptions, opt =>opt.Ignore())
            .ForMember(dest => dest.SourceOptions, opt => opt.Ignore())
            .ForMember(dest => dest.HelpExperienceOptions, opt => opt.Ignore());

        // 3. CaseDetailVM 到 CaseDetail 的對應（反向）
        CreateMap<CaseDetailVM, CaseDetail>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // 由服務層設定
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()) // 由服務層設定
            .ForMember(dest => dest.Deleted, opt => opt.Ignore()) // 系統欄位
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore()) // 系統欄位
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore()) // 系統欄位
            .ForMember(dest => dest.Case, opt => opt.Ignore()) // 導航屬性
            .ForMember(dest => dest.MainCaregiverRelationValue, opt => opt.Ignore()) // 導航屬性
            .ForMember(dest => dest.ContactRelationValue, opt => opt.Ignore()) // 導航屬性
            .ForMember(dest => dest.FamilyStructureType, opt => opt.Ignore()) // 導航屬性
            .ForMember(dest => dest.ParentNationFather, opt => opt.Ignore()) // 導航屬性
            .ForMember(dest => dest.ParentNationMother, opt => opt.Ignore()) // 導航屬性
            .ForMember(dest => dest.MainCaregiverMarryStatusValue, opt => opt.Ignore()) // 導航屬性
            .ForMember(dest => dest.MainCaregiverEduValue, opt => opt.Ignore()) // 導航屬性
            .ForMember(dest => dest.SourceValue, opt => opt.Ignore()) // 導航屬性
            .ForMember(dest => dest.HelpExperienceValue, opt => opt.Ignore()); // 導航屬性

        // 4. CaseSocialWorkerContent 到 SocialWorkerContentVM 的對應
        CreateMap<CaseSocialWorkerContent, SocialWorkerContentVM>()
            .ForMember(dest => dest.ResidenceTypeOptions, opt => opt.Ignore()); // 選項資料需要手動載入

        // 5. SocialWorkerContentVM 到 CaseSocialWorkerContent 的對應
        CreateMap<SocialWorkerContentVM, CaseSocialWorkerContent>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Deleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Case, opt => opt.Ignore())
            .ForMember(dest => dest.ResidenceTypeValue, opt => opt.Ignore());

        // 6. CaseFqeconomicStatus 到 EconomicStatusVM 的對應
        CreateMap<CaseFqeconomicStatus, EconomicStatusVM>();

        // 7. EconomicStatusVM 到 CaseFqeconomicStatus 的對應
        CreateMap<EconomicStatusVM, CaseFqeconomicStatus>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Deleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Case, opt => opt.Ignore());

        // 8. CaseHqhealthStatus 到 HealthStatusVM 的對應
        CreateMap<CaseHqhealthStatus, HealthStatusVM>()
            .ForMember(dest => dest.CaregiverRoleOptions, opt => opt.Ignore());

        // 9. HealthStatusVM 到 CaseHqhealthStatus 的對應
        CreateMap<HealthStatusVM, CaseHqhealthStatus>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Deleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Case, opt => opt.Ignore())
            .ForMember(dest => dest.CaregiverRoleValue, opt => opt.Ignore());

        // 10. CaseIqacademicPerformance 到 AcademicPerformanceVM 的對應
        CreateMap<CaseIqacademicPerformance, AcademicPerformanceVM>();

        // 11. AcademicPerformanceVM 到 CaseIqacademicPerformance 的對應
        CreateMap<AcademicPerformanceVM, CaseIqacademicPerformance>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Deleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Case, opt => opt.Ignore());

        // 12. CaseEqemotionalEvaluation 到 EmotionalEvaluationVM 的對應
        CreateMap<CaseEqemotionalEvaluation, EmotionalEvaluationVM>();

        // 13. EmotionalEvaluationVM 到 CaseEqemotionalEvaluation 的對應
        CreateMap<EmotionalEvaluationVM, CaseEqemotionalEvaluation>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Deleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Case, opt => opt.Ignore());

        // 14. FinalAssessmentSummary 到 FinalAssessmentVM 的對應
        CreateMap<FinalAssessmentSummary, FinalAssessmentVM>();

        // 15. FinalAssessmentVM 到 FinalAssessmentSummary 的對應
        CreateMap<FinalAssessmentVM, FinalAssessmentSummary>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Deleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Case, opt => opt.Ignore());
    }
}
