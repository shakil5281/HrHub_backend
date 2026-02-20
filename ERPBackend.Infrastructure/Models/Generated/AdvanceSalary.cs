using System;
using System.Collections.Generic;

namespace ERPBackend.Infrastructure.Models.Generated;

public partial class AdvanceSalary
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public decimal Amount { get; set; }

    public DateTime RequestDate { get; set; }

    public DateTime? ApprovalDate { get; set; }

    public int RepaymentMonth { get; set; }

    public int RepaymentYear { get; set; }

    public string Status { get; set; } = null!;

    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CompanyId { get; set; }

    public virtual Company? Company { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
