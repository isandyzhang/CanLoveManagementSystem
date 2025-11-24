using CanLove_Backend.Domain.Case.Models.Basic;
using CanLove_Backend.Domain.Case.Models.Opening;
using CanLove_Backend.Domain.Case.Shared.Models;
using CanLove_Backend.Infrastructure.Options.Models;
using CanLove_Backend.Infrastructure.Data.History;
using CanLove_Backend.Infrastructure.Data.Audit;



﻿using System;
using System.Collections.Generic;

namespace CanLove_Backend.Infrastructure.Options.Models;

public partial class FamilyStructureType
{
    public int StructureTypeId { get; set; }

    public string StructureCode { get; set; } = null!;

    public string StructureName { get; set; } = null!;

    public bool? NeedsDescription { get; set; }

    public virtual ICollection<CaseDetail> CaseDetails { get; set; } = new List<CaseDetail>();
}
