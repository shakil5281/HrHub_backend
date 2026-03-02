using System;
using System.Collections.Generic;

namespace ERPBackend.Infrastructure.Models.Generated;

public partial class Shift
{
    public int Id { get; set; }

    public string NameEn { get; set; } = null!;

    public string? NameBn { get; set; }

    public string InTime { get; set; } = null!;

    public string OutTime { get; set; } = null!;

    public string? LateInTime { get; set; }

    public decimal LunchHour { get; set; }

    public string? LunchTimeStart { get; set; }

    public string Status { get; set; } = null!;

    public string? Weekends { get; set; }

    public string? CompanyName { get; set; }

    public int? CompanyId { get; set; }

    public string? ActualInTime { get; set; }

    public string? ActualOutTime { get; set; }
    public bool HasSpecialBreak { get; set; }
    public string? SpecialBreakStart { get; set; }
    public string? SpecialBreakEnd { get; set; }
    public string? SpecialBreakDates { get; set; }

    public virtual Company? Company { get; set; }

    public virtual ICollection<EmployeeShiftRoster> EmployeeShiftRosters { get; set; } = new List<EmployeeShiftRoster>();

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
