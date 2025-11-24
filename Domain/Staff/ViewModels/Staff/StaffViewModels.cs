using System;
using System.ComponentModel.DataAnnotations;
using CanLove_Backend.Domain.Case.Shared.Models;
using CanLove_Backend.Infrastructure.Options.Models;

namespace CanLove_Backend.Domain.Staff.ViewModels;

public class StaffListItemViewModel
{
    public int StaffId { get; set; }
    public string? DisplayName { get; set; }
    public string? DepartmentName { get; set; }
    public string? JobTitleName { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class StaffEditPostModel
{
    [Required]
    public int StaffId { get; set; }
    public string? DisplayName { get; set; }
    public string? DepartmentCode { get; set; }
    public string? JobTitleCode { get; set; }
}

public class StaffEditViewModel
{
    public int StaffId { get; set; }
    public string? DisplayName { get; set; }
    public string? DepartmentCode { get; set; }
    public string? JobTitleCode { get; set; }
    public string? DepartmentName { get; set; }
    public string? JobTitleName { get; set; }

    public List<OptionSetValue> DepartmentOptions { get; set; } = new();
    public List<OptionSetValue> JobTitleOptions { get; set; } = new();
}
