using CanLove_Backend.Domain.Case.ViewModels.Basic;

namespace CanLove_Backend.Domain.Case.ViewModels.Opening;

public interface ICaseWizardVM
{
    string CaseId { get; set; }
    CaseFormMode Mode { get; set; }
    int CurrentStep { get; set; }
    string? SubmitAction { get; set; }
}
