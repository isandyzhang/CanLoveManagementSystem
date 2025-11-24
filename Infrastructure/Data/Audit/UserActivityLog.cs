using CanLove_Backend.Domain.Case.Models.Basic;
using CanLove_Backend.Domain.Case.Models.Opening;
using CanLove_Backend.Domain.Case.Shared.Models;
using CanLove_Backend.Infrastructure.Options.Models;
using CanLove_Backend.Infrastructure.Data.History;
using CanLove_Backend.Infrastructure.Data.Audit;

using System.Collections.Generic;

namespace CanLove_Backend.Infrastructure.Data.Audit;

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
