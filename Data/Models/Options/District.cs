using CanLove_Backend.Data.Models.Core;
using CanLove_Backend.Data.Models.CaseDetails;
using CanLove_Backend.Data.Models.Options;
using CanLove_Backend.Data.Models.History;
using CanLove_Backend.Data.Models.Audit;



﻿using System;
using System.Collections.Generic;

namespace CanLove_Backend.Data.Models.Options;

public partial class District
{
    public int DistrictId { get; set; }

    public string DistrictName { get; set; } = null!;

    public int CityId { get; set; }

    public virtual ICollection<Case> Cases { get; set; } = new List<Case>();

    public virtual City City { get; set; } = null!;
}
