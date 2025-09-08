using CanLove_Backend.Data.Models.Core;
using CanLove_Backend.Data.Models.CaseDetails;
using CanLove_Backend.Data.Models.Options;
using CanLove_Backend.Data.Models.History;
using CanLove_Backend.Data.Models.Audit;



﻿using System;
using System.Collections.Generic;

namespace CanLove_Backend.Data.Models.Options;

public partial class OptionSet
{
    public int OptionSetId { get; set; }

    public string OptionKey { get; set; } = null!;

    public string OptionSetName { get; set; } = null!;

    public virtual ICollection<OptionSetValue> OptionSetValues { get; set; } = new List<OptionSetValue>();
}
