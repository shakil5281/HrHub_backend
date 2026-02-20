using System;
using System.Collections.Generic;

namespace ERPBackend.Infrastructure.Models.Generated;

public partial class Department
{
    public int Id { get; set; }

    public string NameEn { get; set; } = null!;

    public int CompanyId { get; set; }

    public string? NameBn { get; set; }

    public virtual Company Company { get; set; } = null!;

    public virtual ICollection<Designation> Designations { get; set; } = new List<Designation>();

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<Line> Lines { get; set; } = new List<Line>();

    public virtual ICollection<ManpowerRequirement> ManpowerRequirements { get; set; } = new List<ManpowerRequirement>();

    public virtual ICollection<Section> Sections { get; set; } = new List<Section>();

    public virtual ICollection<Transfer> TransferFromDepartments { get; set; } = new List<Transfer>();

    public virtual ICollection<Transfer> TransferToDepartments { get; set; } = new List<Transfer>();
}
