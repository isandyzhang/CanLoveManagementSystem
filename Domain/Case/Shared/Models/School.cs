using CanLove_Backend.Domain.Case.Models.Basic;
using CanLove_Backend.Domain.Case.Models.Opening;
using CanLove_Backend.Domain.Case.Shared.Models;
using CanLove_Backend.Infrastructure.Options.Models;
using CanLove_Backend.Infrastructure.Data.History;
using CanLove_Backend.Infrastructure.Data.Audit;



﻿using System;
using System.Collections.Generic;

namespace CanLove_Backend.Domain.Case.Shared.Models;

public partial class School
{
    public int SchoolId { get; set; }

    public string SchoolName { get; set; } = null!;

    public string SchoolType { get; set; } = null!;

    public virtual ICollection<CanLove_Backend.Domain.Case.Models.Basic.Case> Cases { get; set; } = new List<CanLove_Backend.Domain.Case.Models.Basic.Case>();
}
