using System;
using System.Collections.Generic;

namespace ERPBackend.Infrastructure.Models.Generated;

public partial class DailySalarySheet
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public DateTime Date { get; set; }

    public decimal GrossSalary { get; set; }

    public decimal PerDaySalary { get; set; }

    public string AttendanceStatus { get; set; } = null!;

    public decimal Othours { get; set; }

    public decimal Otamount { get; set; }

    public decimal TotalEarning { get; set; }

    public decimal Deduction { get; set; }

    public decimal NetPayable { get; set; }

    public DateTime ProcessedAt { get; set; }

    public int? CompanyId { get; set; }

    public virtual Company? Company { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
