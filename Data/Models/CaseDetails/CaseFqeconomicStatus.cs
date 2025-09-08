using CanLove_Backend.Data.Models.Core;
using CanLove_Backend.Data.Models.CaseDetails;
using CanLove_Backend.Data.Models.Options;
using CanLove_Backend.Data.Models.History;
using CanLove_Backend.Data.Models.Audit;



﻿using System;
using System.Collections.Generic;

namespace CanLove_Backend.Data.Models.CaseDetails;

public partial class CaseFqeconomicStatus
{
    public string CaseId { get; set; } = null!;

    public string? EconomicOverview { get; set; }

    public string? WorkSituation { get; set; }

    public string? CivilWelfareResources { get; set; }

    public decimal? MonthlyIncome { get; set; }

    public decimal? MonthlyExpense { get; set; }

    public string? MonthlyExpenseNote { get; set; }

    public string? Description { get; set; }

    public bool? Deleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? DeletedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Case Case { get; set; } = null!;
}
