using System;
using System.Collections.Generic;

namespace ERPBackend.Infrastructure.Models.Generated;

public partial class Group
{
    public int Id { get; set; }

    public string NameEn { get; set; } = null!;

    public string? NameBn { get; set; }

    public string? CompanyName { get; set; }

    public int? CompanyId { get; set; }

    public virtual Company? Company { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
