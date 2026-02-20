using System;
using System.Collections.Generic;

namespace ERPBackend.Infrastructure.Models.Generated;

public partial class Line
{
    public int Id { get; set; }

    public string NameEn { get; set; } = null!;

    public int SectionId { get; set; }

    public string? NameBn { get; set; }

    public int? CompanyId { get; set; }

    public int? DepartmentId { get; set; }

    public virtual Company? Company { get; set; }

    public virtual Department? Department { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual Section Section { get; set; } = null!;
}
