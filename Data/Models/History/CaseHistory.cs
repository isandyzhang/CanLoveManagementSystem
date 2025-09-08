using CanLove_Backend.Data.Models.Core;
using CanLove_Backend.Data.Models.CaseDetails;
using CanLove_Backend.Data.Models.Options;
using CanLove_Backend.Data.Models.History;
using CanLove_Backend.Data.Models.Audit;



﻿using System;
using System.Collections.Generic;

namespace CanLove_Backend.Data.Models.History;

public partial class CaseHistory
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
