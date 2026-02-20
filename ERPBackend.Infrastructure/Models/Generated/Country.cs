using System;
using System.Collections.Generic;

namespace ERPBackend.Infrastructure.Models.Generated;

public partial class Country
{
    public int Id { get; set; }

    public string NameEn { get; set; } = null!;

    public string? NameBn { get; set; }

    public virtual ICollection<Division> Divisions { get; set; } = new List<Division>();
}
