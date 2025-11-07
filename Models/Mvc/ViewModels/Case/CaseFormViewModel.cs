using CanLove_Backend.Data.Models.Core;
using CanLove_Backend.Data.Models.Options;
using Microsoft.AspNetCore.Http;

namespace CanLove_Backend.Models.Mvc.ViewModels;

public class CaseFormViewModel
{
    public CaseFormMode Mode { get; set; } = CaseFormMode.Create;

    public Case Case { get; set; } = new Case();

    public IFormFile? PhotoFile { get; set; }

    public List<City> Cities { get; set; } = new List<City>();

    public List<District> Districts { get; set; } = new List<District>();

    public List<School> Schools { get; set; } = new List<School>();

    public List<OptionSetValue> GenderOptions { get; set; } = new List<OptionSetValue>();

    public string? SubmitAction { get; set; }
}


