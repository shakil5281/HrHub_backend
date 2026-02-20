using System;
using System.Collections.Generic;

namespace ERPBackend.Infrastructure.Models.Generated;

public partial class Separation
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public DateTime LastWorkingDate { get; set; }

    public string Type { get; set; } = null!;

    public string Reason { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? AdminRemark { get; set; }

    public bool IsSettled { get; set; }

    public DateTime? SettledAt { get; set; }

    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
