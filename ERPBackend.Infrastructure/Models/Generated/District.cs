using System;
using System.Collections.Generic;

namespace ERPBackend.Infrastructure.Models.Generated;

public partial class District
{
    public int Id { get; set; }

    public string NameEn { get; set; } = null!;

    public int DivisionId { get; set; }

    public string? NameBn { get; set; }

    public virtual Division Division { get; set; } = null!;

    public virtual ICollection<PostOffice> PostOffices { get; set; } = new List<PostOffice>();

    public virtual ICollection<Thana> Thanas { get; set; } = new List<Thana>();
}
