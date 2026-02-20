using System;
using System.Collections.Generic;

namespace ERPBackend.Infrastructure.Models.Generated;

public partial class Section
{
    public int Id { get; set; }

    public string NameEn { get; set; } = null!;

    public int DepartmentId { get; set; }

    public string? NameBn { get; set; }

    public int? CompanyId { get; set; }

    public virtual Company? Company { get; set; }

    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<Designation> Designations { get; set; } = new List<Designation>();

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<Line> Lines { get; set; } = new List<Line>();
}
