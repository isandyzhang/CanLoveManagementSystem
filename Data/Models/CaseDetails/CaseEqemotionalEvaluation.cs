using CanLove_Backend.Data.Models.Core;
using CanLove_Backend.Data.Models.CaseDetails;
using CanLove_Backend.Data.Models.Options;
using CanLove_Backend.Data.Models.History;
using CanLove_Backend.Data.Models.Audit;



﻿using System;
using System.Collections.Generic;

namespace CanLove_Backend.Data.Models.CaseDetails;

public partial class CaseEqemotionalEvaluation
{
    public string CaseId { get; set; } = null!;
    public int OpeningId { get; set; }

    public int? EqQ1 { get; set; }

    public int? EqQ2 { get; set; }

    public int? EqQ3 { get; set; }

    public int? EqQ4 { get; set; }

    public int? EqQ5 { get; set; }

    public int? EqQ6 { get; set; }

    public int? EqQ7 { get; set; }

    public bool? Deleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? DeletedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Case Case { get; set; } = null!;

    public virtual CaseOpening CaseOpening { get; set; } = null!;
}
