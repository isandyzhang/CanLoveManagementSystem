using CanLove_Backend.Models.Mvc.ViewModels;

namespace CanLove_Backend.Models.Mvc.ViewModels.CaseWizardOpenCase;

public interface ICaseWizardViewModel
{
    string CaseId { get; set; }
    CaseFormMode Mode { get; set; }
    int CurrentStep { get; set; }
    string? SubmitAction { get; set; }
}


