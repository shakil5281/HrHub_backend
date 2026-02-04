using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPBackend.Core.Models
{
    public class Country
    {
        [Key] public int Id { get; set; }

        [Required] [StringLength(100)] public string NameEn { get; set; } = string.Empty;
        [StringLength(100)] public string? NameBn { get; set; }

        public virtual ICollection<Division> Divisions { get; set; } = new List<Division>();
    }

    public class Division
    {
        [Key] public int Id { get; set; }

        [Required] [StringLength(100)] public string NameEn { get; set; } = string.Empty;
        [StringLength(100)] public string? NameBn { get; set; }

        public int CountryId { get; set; }

        [ForeignKey("CountryId")] public virtual Country? Country { get; set; }

        public virtual ICollection<District> Districts { get; set; } = new List<District>();
    }

    public class District
    {
        [Key] public int Id { get; set; }

        [Required] [StringLength(100)] public string NameEn { get; set; } = string.Empty;
        [StringLength(100)] public string? NameBn { get; set; }

        public int DivisionId { get; set; }

        [ForeignKey("DivisionId")] public virtual Division? Division { get; set; }

        public virtual ICollection<Thana> Thanas { get; set; } = new List<Thana>();

        public virtual ICollection<PostOffice> PostOffices { get; set; } = new List<PostOffice>();
    }

    public class Thana
    {
        [Key] public int Id { get; set; }

        [Required] [StringLength(100)] public string NameEn { get; set; } = string.Empty;
        [StringLength(100)] public string? NameBn { get; set; }

        public int DistrictId { get; set; }

        [ForeignKey("DistrictId")] public virtual District? District { get; set; }
    }

    public class PostOffice
    {
        [Key] public int Id { get; set; }

        [Required] [StringLength(100)] public string NameEn { get; set; } = string.Empty;
        [StringLength(100)] public string? NameBn { get; set; }

        [Required] [StringLength(20)] public string Code { get; set; } = string.Empty;

        public int DistrictId { get; set; }

        [ForeignKey("DistrictId")] public virtual District? District { get; set; }
    }
}
