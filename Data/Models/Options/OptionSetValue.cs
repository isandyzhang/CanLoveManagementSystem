using CanLove_Backend.Data.Models.Core;
using CanLove_Backend.Data.Models.CaseDetails;
using CanLove_Backend.Data.Models.Options;
using CanLove_Backend.Data.Models.History;
using CanLove_Backend.Data.Models.Audit;



﻿using System;
using System.Collections.Generic;

namespace CanLove_Backend.Data.Models.Options;

public partial class OptionSetValue
{
    public int OptionValueId { get; set; }

    public int OptionSetId { get; set; }

    public string ValueCode { get; set; } = null!;

    public string ValueName { get; set; } = null!;

    public virtual ICollection<CaseConsultationRecord> CaseConsultationRecordConsultationMethodValues { get; set; } = new List<CaseConsultationRecord>();

    public virtual ICollection<CaseConsultationRecord> CaseConsultationRecordConsultationTargetValues { get; set; } = new List<CaseConsultationRecord>();

    public virtual ICollection<CaseDetail> CaseDetailContactRelationValues { get; set; } = new List<CaseDetail>();

    public virtual ICollection<CaseDetail> CaseDetailHelpExperienceValues { get; set; } = new List<CaseDetail>();

    public virtual ICollection<CaseDetail> CaseDetailMainCaregiverEduValues { get; set; } = new List<CaseDetail>();

    public virtual ICollection<CaseDetail> CaseDetailMainCaregiverMarryStatusValues { get; set; } = new List<CaseDetail>();

    public virtual ICollection<CaseDetail> CaseDetailSourceValues { get; set; } = new List<CaseDetail>();

    public virtual ICollection<CaseFamilyMemberNote> CaseFamilyMemberNotes { get; set; } = new List<CaseFamilyMemberNote>();

    public virtual ICollection<CaseHqhealthStatus> CaseHqhealthStatuses { get; set; } = new List<CaseHqhealthStatus>();

    public virtual ICollection<CaseSocialWorkerContent> CaseSocialWorkerContents { get; set; } = new List<CaseSocialWorkerContent>();

    public virtual ICollection<CaseSocialWorkerService> CaseSocialWorkerServices { get; set; } = new List<CaseSocialWorkerService>();

    public virtual OptionSet OptionSet { get; set; } = null!;
}
