using CanLove_Backend.Domain.Case.Models.Basic;
using CanLove_Backend.Domain.Case.Models.Opening;
using CanLove_Backend.Domain.Case.Shared.Models;
using CanLove_Backend.Infrastructure.Options.Models;
using CanLove_Backend.Infrastructure.Data.History;
using CanLove_Backend.Infrastructure.Data.Audit;



﻿using System;
using System.Collections.Generic;

namespace CanLove_Backend.Infrastructure.Data.History;

public partial class CaseDetailHistory
{
    public long HistoryId { get; set; }

    public string CaseId { get; set; } = null!;

    public int VersionNumber { get; set; }

    public string ChangeType { get; set; } = null!;

    public string? FieldName { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? ChangeReason { get; set; }

    public string? ChangedBy { get; set; }

    public DateTime? ChangedAt { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public virtual Case Case { get; set; } = null!;
}
