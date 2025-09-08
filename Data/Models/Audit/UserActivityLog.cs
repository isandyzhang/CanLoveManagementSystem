using CanLove_Backend.Data.Models.Core;
using CanLove_Backend.Data.Models.CaseDetails;
using CanLove_Backend.Data.Models.Options;
using CanLove_Backend.Data.Models.History;
using CanLove_Backend.Data.Models.Audit;

using System.Collections.Generic;

namespace CanLove_Backend.Data.Models.Audit;

public partial class UserActivityLog
{
    public long ActivityId { get; set; }

    public string UserId { get; set; } = null!;

    public string ActivityType { get; set; } = null!;

    public string? ActivityDescription { get; set; }

    public string? TargetTable { get; set; }

    public string? TargetRecordId { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime? CreatedAt { get; set; }
}
