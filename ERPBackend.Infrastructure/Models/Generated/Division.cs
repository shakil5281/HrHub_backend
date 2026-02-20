using System;
using System.Collections.Generic;

namespace ERPBackend.Infrastructure.Models.Generated;

public partial class Division
{
    public int Id { get; set; }

    public string NameEn { get; set; } = null!;

    public int CountryId { get; set; }

    public string? NameBn { get; set; }

    public virtual Country Country { get; set; } = null!;

    public virtual ICollection<District> Districts { get; set; } = new List<District>();
}
