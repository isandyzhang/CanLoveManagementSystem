using System;
using System.ComponentModel.DataAnnotations;
using CanLove_Backend.Data.Models.Options;

namespace CanLove_Backend.Models.Mvc.ViewModels.Staff;

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
