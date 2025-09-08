using CanLove_Backend.Data.Models.Core;
using CanLove_Backend.Data.Models.CaseDetails;
using CanLove_Backend.Data.Models.Options;
using CanLove_Backend.Data.Models.History;
using CanLove_Backend.Data.Models.Audit;



﻿using System;
using System.Collections.Generic;

namespace CanLove_Backend.Data.Models.Options;

public partial class FamilyStructureType
{
    public int StructureTypeId { get; set; }

    public string StructureCode { get; set; } = null!;

    public string StructureName { get; set; } = null!;

    public bool? NeedsDescription { get; set; }

    public virtual ICollection<CaseDetail> CaseDetails { get; set; } = new List<CaseDetail>();
}
