using System;
using System.Collections.Generic;

namespace ERPBackend.Infrastructure.Models.Generated;

public partial class CounselingRecord
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public DateTime CounselingDate { get; set; }

    public string IssueType { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? ActionTaken { get; set; }

    public string? FollowUpNotes { get; set; }

    public string Status { get; set; } = null!;

    public string Severity { get; set; } = null!;

    public DateTime? FollowUpDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
