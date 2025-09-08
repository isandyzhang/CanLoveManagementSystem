using CanLove_Backend.Data.Models.Core;
using CanLove_Backend.Data.Models.CaseDetails;
using CanLove_Backend.Data.Models.Options;
using CanLove_Backend.Data.Models.History;
using CanLove_Backend.Data.Models.Audit;



﻿using System;
using System.Collections.Generic;

namespace CanLove_Backend.Data.Models.Options;

public partial class School
{
    public int SchoolId { get; set; }

    public string SchoolName { get; set; } = null!;

    public string SchoolType { get; set; } = null!;

    public virtual ICollection<Case> Cases { get; set; } = new List<Case>();
}
