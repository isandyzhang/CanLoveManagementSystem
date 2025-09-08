using CanLove_Backend.Data.Models.Core;
using CanLove_Backend.Data.Models.CaseDetails;
using CanLove_Backend.Data.Models.Options;
using CanLove_Backend.Data.Models.History;
using CanLove_Backend.Data.Models.Audit;



﻿using System;
using System.Collections.Generic;

namespace CanLove_Backend.Data.Models.Options;

public partial class Nationality
{
    public int NationalityId { get; set; }

    public string NationalityName { get; set; } = null!;

    public string NationalityCode { get; set; } = null!;

    public virtual ICollection<CaseDetail> CaseDetailParentNationFathers { get; set; } = new List<CaseDetail>();

    public virtual ICollection<CaseDetail> CaseDetailParentNationMothers { get; set; } = new List<CaseDetail>();
}
