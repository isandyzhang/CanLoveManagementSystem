using CanLove_Backend.Domain.Case.Models.Basic;
using CanLove_Backend.Domain.Case.Models.Opening;
using CanLove_Backend.Domain.Case.Shared.Models;
using CanLove_Backend.Infrastructure.Options.Models;
using CanLove_Backend.Infrastructure.Data.History;
using CanLove_Backend.Infrastructure.Data.Audit;



﻿using System;
using System.Collections.Generic;

namespace CanLove_Backend.Infrastructure.Options.Models;

public partial class OptionSetValue
{
    public int OptionValueId { get; set; }

    public int OptionSetId { get; set; }

    public string ValueCode { get; set; } = null!;

    public string ValueName { get; set; } = null!;

    public virtual ICollection<CaseConsultationRecord> CaseConsultationRecordConsultationMethodValues { get; set; } = new List<CaseConsultationRecord>();

    public virtual ICollection<CaseConsultationRecord> CaseConsultationRecordConsultationTargetValues { get; set; } = new List<CaseConsultationRecord>();

    public virtual ICollection<CaseDetail> CaseDetailContactRelationValues { get; set; } = new List<CaseDetail>();

    public virtual ICollection<CaseDetail> CaseDetailMainCaregiverRelationValues { get; set; } = new List<CaseDetail>();

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
