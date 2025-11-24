using CanLove_Backend.Domain.Case.Models.Basic;
using CanLove_Backend.Domain.Case.Models.Opening;
using CanLove_Backend.Domain.Case.Shared.Models;
using CanLove_Backend.Infrastructure.Options.Models;
using CanLove_Backend.Infrastructure.Data.History;
using CanLove_Backend.Infrastructure.Data.Audit;



﻿using System;
using System.Collections.Generic;

namespace CanLove_Backend.Domain.Case.Models.Opening;

public partial class CaseHqhealthStatus
{
    public int CaregiverId { get; set; }

    public string CaseId { get; set; } = null!;
    public int OpeningId { get; set; }

    public int CaregiverRoleValueId { get; set; }

    public string? CaregiverName { get; set; }

    public bool? IsPrimary { get; set; }

    public byte? EmotionalExpressionRating { get; set; }

    public string? EmotionalExpressionNote { get; set; }

    public byte? HealthStatusRating { get; set; }

    public string? HealthStatusNote { get; set; }

    public byte? ChildHealthStatusRating { get; set; }

    public string? ChildHealthStatusNote { get; set; }

    public byte? ChildCareStatusRating { get; set; }

    public string? ChildCareStatusNote { get; set; }

    public bool? Deleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? DeletedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual OptionSetValue CaregiverRoleValue { get; set; } = null!;

    public virtual CanLove_Backend.Domain.Case.Models.Basic.Case Case { get; set; } = null!;

    public virtual CaseOpening CaseOpening { get; set; } = null!;
}
