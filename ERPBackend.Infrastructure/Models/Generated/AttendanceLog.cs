using System;
using System.Collections.Generic;

namespace ERPBackend.Infrastructure.Models.Generated;

public partial class AttendanceLog
{
    public int Id { get; set; }

    public int EmployeeCard { get; set; }

    public int? CompanyId { get; set; }

    public string? EmployeeId { get; set; }

    public DateTime LogTime { get; set; }

    public string? DeviceId { get; set; }

    public string? VerificationMode { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Company? Company { get; set; }

    public virtual Employee EmployeeCardNavigation { get; set; } = null!;
}
