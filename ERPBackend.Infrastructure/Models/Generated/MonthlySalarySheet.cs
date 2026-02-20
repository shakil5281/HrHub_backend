using System;
using System.Collections.Generic;

namespace ERPBackend.Infrastructure.Models.Generated;

public partial class MonthlySalarySheet
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }

    public decimal GrossSalary { get; set; }

    public decimal BasicSalary { get; set; }

    public int TotalDays { get; set; }

    public int PresentDays { get; set; }

    public int AbsentDays { get; set; }

    public int LeaveDays { get; set; }

    public int Holidays { get; set; }

    public int WeekendDays { get; set; }

    public decimal Othours { get; set; }

    public decimal Otrate { get; set; }

    public decimal Otamount { get; set; }

    public decimal AttendanceBonus { get; set; }

    public decimal OtherAllowances { get; set; }

    public decimal TotalEarning { get; set; }

    public decimal AbsentDeduction { get; set; }

    public decimal AdvanceDeduction { get; set; }

    public decimal Otdeduction { get; set; }

    public decimal TotalDeduction { get; set; }

    public decimal NetPayable { get; set; }

    public string Status { get; set; } = null!;

    public DateTime ProcessedAt { get; set; }

    public string? ProcessedBy { get; set; }

    public int? CompanyId { get; set; }

    public virtual Company? Company { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
