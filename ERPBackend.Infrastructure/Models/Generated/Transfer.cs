using System;
using System.Collections.Generic;

namespace ERPBackend.Infrastructure.Models.Generated;

public partial class Transfer
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int? FromDepartmentId { get; set; }

    public int? FromDesignationId { get; set; }

    public int ToDepartmentId { get; set; }

    public int ToDesignationId { get; set; }

    public DateTime TransferDate { get; set; }

    public string Reason { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? AdminRemark { get; set; }

    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Department? FromDepartment { get; set; }

    public virtual Designation? FromDesignation { get; set; }

    public virtual Department ToDepartment { get; set; } = null!;

    public virtual Designation ToDesignation { get; set; } = null!;
}
