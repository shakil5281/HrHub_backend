using System;
using System.Collections.Generic;

namespace ERPBackend.Infrastructure.Models.Generated;

public partial class Attendance
{
    public int Id { get; set; }

    public string? EmployeeId { get; set; }

    public DateTime Date { get; set; }

    public DateTime? InTime { get; set; }

    public DateTime? OutTime { get; set; }

    public string Status { get; set; } = null!;

    public decimal Othours { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public bool IsManual { get; set; }

    public string? Reason { get; set; }

    public string? Remarks { get; set; }

    public int? CompanyId { get; set; }

    public int EmployeeCard { get; set; }

    public virtual Company? Company { get; set; }

    public virtual Employee EmployeeCardNavigation { get; set; } = null!;
}
