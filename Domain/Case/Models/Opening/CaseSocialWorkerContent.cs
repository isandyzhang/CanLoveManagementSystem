using CanLove_Backend.Domain.Case.Models.Basic;
using CanLove_Backend.Domain.Case.Models.Opening;
using CanLove_Backend.Domain.Case.Shared.Models;
using CanLove_Backend.Infrastructure.Options.Models;
using CanLove_Backend.Infrastructure.Data.History;
using CanLove_Backend.Infrastructure.Data.Audit;



﻿using System;
using System.Collections.Generic;

namespace CanLove_Backend.Domain.Case.Models.Opening;

public partial class CaseSocialWorkerContent
{
    public string CaseId { get; set; } = null!;
    public int OpeningId { get; set; }

    public string? FamilyTreeImg { get; set; }

    public int? ResidenceTypeValueId { get; set; }

    public byte? HouseCleanlinessRating { get; set; }

    public string? HouseCleanlinessNote { get; set; }

    public byte? HouseSafetyRating { get; set; }

    public string? HouseSafetyNote { get; set; }

    public byte? CaregiverChildInteractionRating { get; set; }

    public string? CaregiverChildInteractionNote { get; set; }

    public byte? CaregiverFamilyInteractionRating { get; set; }

    public string? CaregiverFamilyInteractionNote { get; set; }

    public byte? FamilyResourceAbilityRating { get; set; }

    public string? FamilyResourceAbilityNote { get; set; }

    public byte? FamilySocialSupportRating { get; set; }

    public string? FamilySocialSupportNote { get; set; }

    public string? SpecialCircumstancesDescription { get; set; }

    public bool? Deleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? DeletedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual CanLove_Backend.Domain.Case.Models.Basic.Case Case { get; set; } = null!;

    public virtual CaseOpening CaseOpening { get; set; } = null!;

    public virtual OptionSetValue? ResidenceTypeValue { get; set; }
}
