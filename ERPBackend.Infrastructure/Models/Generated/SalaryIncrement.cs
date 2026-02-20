using System;
using System.Collections.Generic;

namespace ERPBackend.Infrastructure.Models.Generated;

public partial class SalaryIncrement
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public decimal PreviousGrossSalary { get; set; }

    public decimal IncrementAmount { get; set; }

    public decimal NewGrossSalary { get; set; }

    public DateTime EffectiveDate { get; set; }

    public string IncrementType { get; set; } = null!;

    public string? Remarks { get; set; }

    public bool IsApplied { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CompanyId { get; set; }

    public virtual Company? Company { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
