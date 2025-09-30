using CanLove_Backend.Data.Models.Core;
using CanLove_Backend.Data.Models.CaseDetails;
using CanLove_Backend.Data.Models.Options;
using CanLove_Backend.Data.Models.History;
using CanLove_Backend.Data.Models.Audit;



﻿using System;
using System.Collections.Generic;

namespace CanLove_Backend.Data.Models.CaseDetails;

public partial class CaseDetail
{
    public string CaseId { get; set; } = null!;

    public string? ContactName { get; set; }

    public int? ContactRelationValueId { get; set; }

    public string? ContactPhone { get; set; }

    public string? HomePhone { get; set; }

    public int? FamilyStructureTypeId { get; set; }

    public string? FamilyStructureOtherDesc { get; set; }

    public int? ParentNationFatherId { get; set; }

    public int? ParentNationMotherId { get; set; }

    public string? MainCaregiverName { get; set; }

    public int? MainCaregiverRelationValueId { get; set; }

    public string? MainCaregiverId { get; set; }

    public DateOnly? MainCaregiverBirth { get; set; }

    public string? MainCaregiverJob { get; set; }

    public int? MainCaregiverMarryStatusValueId { get; set; }

    public int? MainCaregiverEduValueId { get; set; }

    public int? SourceValueId { get; set; }

    public int? HelpExperienceValueId { get; set; }

    public string? Note { get; set; }

    public bool? Deleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? DeletedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Case Case { get; set; } = null!;

    public virtual OptionSetValue? ContactRelationValue { get; set; }

    public virtual OptionSetValue? MainCaregiverRelationValue { get; set; }

    public virtual FamilyStructureType? FamilyStructureType { get; set; }

    public virtual OptionSetValue? HelpExperienceValue { get; set; }

    public virtual OptionSetValue? MainCaregiverEduValue { get; set; }

    public virtual OptionSetValue? MainCaregiverMarryStatusValue { get; set; }

    public virtual Nationality? ParentNationFather { get; set; }

    public virtual Nationality? ParentNationMother { get; set; }

    public virtual OptionSetValue? SourceValue { get; set; }
}
