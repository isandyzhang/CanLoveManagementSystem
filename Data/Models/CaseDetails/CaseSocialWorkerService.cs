using CanLove_Backend.Data.Models.Core;
using CanLove_Backend.Data.Models.CaseDetails;
using CanLove_Backend.Data.Models.Options;
using CanLove_Backend.Data.Models.History;
using CanLove_Backend.Data.Models.Audit;



﻿using System;
using System.Collections.Generic;

namespace CanLove_Backend.Data.Models.CaseDetails;

public partial class CaseSocialWorkerService
{
    public int ServiceId { get; set; }

    public string CaseId { get; set; } = null!;

    public int ServiceTypeValueId { get; set; }

    public string? ServiceDescription { get; set; }

    public DateOnly? ServiceDate { get; set; }

    public string? ServiceProvider { get; set; }

    public bool? Deleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? DeletedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Case Case { get; set; } = null!;

    public virtual OptionSetValue ServiceTypeValue { get; set; } = null!;
}
