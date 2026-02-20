using System;
using System.Collections.Generic;

namespace ERPBackend.Infrastructure.Models.Generated;

public partial class Bonuse
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public string BonusType { get; set; } = null!;

    public decimal Amount { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }

    public DateTime PaymentDate { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public int? CompanyId { get; set; }

    public virtual Company? Company { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
