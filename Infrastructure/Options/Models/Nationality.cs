using CanLove_Backend.Domain.Case.Models.Basic;
using CanLove_Backend.Domain.Case.Models.Opening;
using CanLove_Backend.Domain.Case.Shared.Models;
using CanLove_Backend.Infrastructure.Options.Models;
using CanLove_Backend.Infrastructure.Data.History;
using CanLove_Backend.Infrastructure.Data.Audit;



﻿using System;
using System.Collections.Generic;

namespace CanLove_Backend.Infrastructure.Options.Models;

public partial class Nationality
{
    public int NationalityId { get; set; }

    public string NationalityName { get; set; } = null!;

    public string NationalityCode { get; set; } = null!;

    public virtual ICollection<CaseDetail> CaseDetailParentNationFathers { get; set; } = new List<CaseDetail>();

    public virtual ICollection<CaseDetail> CaseDetailParentNationMothers { get; set; } = new List<CaseDetail>();
}
