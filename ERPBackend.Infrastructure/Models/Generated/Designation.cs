using System;
using System.Collections.Generic;

namespace ERPBackend.Infrastructure.Models.Generated;

public partial class Designation
{
    public int Id { get; set; }

    public string NameEn { get; set; } = null!;

    public int SectionId { get; set; }

    public string? NameBn { get; set; }

    public decimal AttendanceBonus { get; set; }

    public decimal HolidayBill { get; set; }

    public decimal NightBill { get; set; }

    public int? CompanyId { get; set; }

    public int? DepartmentId { get; set; }

    public virtual Company? Company { get; set; }

    public virtual Department? Department { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<ManpowerRequirement> ManpowerRequirements { get; set; } = new List<ManpowerRequirement>();

    public virtual Section Section { get; set; } = null!;

    public virtual ICollection<Transfer> TransferFromDesignations { get; set; } = new List<Transfer>();

    public virtual ICollection<Transfer> TransferToDesignations { get; set; } = new List<Transfer>();
}
