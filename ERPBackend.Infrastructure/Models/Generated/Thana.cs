using System;
using System.Collections.Generic;

namespace ERPBackend.Infrastructure.Models.Generated;

public partial class Thana
{
    public int Id { get; set; }

    public string NameEn { get; set; } = null!;

    public int DistrictId { get; set; }

    public string? NameBn { get; set; }

    public virtual District District { get; set; } = null!;
}
