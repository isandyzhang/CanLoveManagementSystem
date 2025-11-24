using CanLove_Backend.Domain.Case.Models.Basic;
using CanLove_Backend.Domain.Case.Models.Opening;
using CanLove_Backend.Domain.Case.Shared.Models;
using CanLove_Backend.Infrastructure.Options.Models;
using CanLove_Backend.Infrastructure.Data.History;
using CanLove_Backend.Infrastructure.Data.Audit;



﻿using System;
using System.Collections.Generic;

namespace CanLove_Backend.Domain.Case.Models.Opening;

public partial class CaseFamilySpecialStatus
{
    public string CaseId { get; set; } = null!;

    public int StatusTypeValueId { get; set; }

    public string? LowIncomeCardNumber { get; set; }

    public string? DisabilityIcfCode { get; set; }

    public string? OtherDescription { get; set; }

    public bool? Deleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? DeletedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual CanLove_Backend.Domain.Case.Models.Basic.Case Case { get; set; } = null!;

    public virtual OptionSetValue StatusTypeValue { get; set; } = null!;
}
