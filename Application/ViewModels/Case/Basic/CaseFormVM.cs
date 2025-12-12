using CanLove_Backend.Domain.Case.Models.Basic;
using CanLove_Backend.Domain.Case.Shared.Models;
using CanLove_Backend.Infrastructure.Options.Models;
using Microsoft.AspNetCore.Http;

namespace CanLove_Backend.Application.ViewModels.Case.Basic;

public class CaseFormVM
{
    public CaseFormMode Mode { get; set; } = CaseFormMode.Create;

    public CanLove_Backend.Domain.Case.Models.Basic.Case Case { get; set; } = new CanLove_Backend.Domain.Case.Models.Basic.Case();

    public IFormFile? PhotoFile { get; set; }

    public List<City> Cities { get; set; } = new List<City>();

    public List<District> Districts { get; set; } = new List<District>();

    public List<School> Schools { get; set; } = new List<School>();

    public List<OptionSetValue> GenderOptions { get; set; } = new List<OptionSetValue>();

    public string? SubmitAction { get; set; }
}

