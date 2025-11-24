using CanLove_Backend.Domain.Case.Models.Basic;
using CanLove_Backend.Domain.Case.Models.Opening;
using CanLove_Backend.Domain.Case.Shared.Models;
using CanLove_Backend.Infrastructure.Options.Models;
using CanLove_Backend.Infrastructure.Data.History;
using CanLove_Backend.Infrastructure.Data.Audit;



﻿using System;
using System.Collections.Generic;

namespace CanLove_Backend.Infrastructure.Options.Models;

public partial class OptionSet
{
    public int OptionSetId { get; set; }

    public string OptionKey { get; set; } = null!;

    public string OptionSetName { get; set; } = null!;

    public virtual ICollection<OptionSetValue> OptionSetValues { get; set; } = new List<OptionSetValue>();
}
