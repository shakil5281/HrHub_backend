using System;
using System.Collections.Generic;

namespace ERPBackend.Infrastructure.Models.Generated;

public partial class ManpowerRequirement
{
    public int Id { get; set; }

    public int DepartmentId { get; set; }

    public int DesignationId { get; set; }

    public int RequiredCount { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public virtual Department Department { get; set; } = null!;

    public virtual Designation Designation { get; set; } = null!;
}
