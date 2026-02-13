using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPBackend.Core.Models
{
    public class Department
    {
        [Key] public int Id { get; set; }

        [Required] [StringLength(100)] public string NameEn { get; set; } = string.Empty;

        [StringLength(100)] public string? NameBn { get; set; }

        public int CompanyId { get; set; }

        [ForeignKey("CompanyId")] public virtual Company? Company { get; set; }

        public virtual ICollection<Section> Sections { get; set; } = new List<Section>();
    }

    public class Section
    {
        [Key] public int Id { get; set; }

        [Required] [StringLength(100)] public string NameEn { get; set; } = string.Empty;

        [StringLength(100)] public string? NameBn { get; set; }

        public int? CompanyId { get; set; }

        [ForeignKey("CompanyId")] public virtual Company? Company { get; set; }

        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")] public virtual Department? Department { get; set; }

        public virtual ICollection<Designation> Designations { get; set; } = new List<Designation>();
        public virtual ICollection<Line> Lines { get; set; } = new List<Line>();
    }

    public class Designation
    {
        [Key] public int Id { get; set; }

        [Required] [StringLength(100)] public string NameEn { get; set; } = string.Empty;

        [StringLength(100)] public string? NameBn { get; set; }

        [Column(TypeName = "decimal(18,2)")] public decimal NightBill { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal HolidayBill { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal AttendanceBonus { get; set; }

        public int? CompanyId { get; set; }

        [ForeignKey("CompanyId")] public virtual Company? Company { get; set; }

        public int? DepartmentId { get; set; }

        [ForeignKey("DepartmentId")] public virtual Department? Department { get; set; }

        public int SectionId { get; set; }

        [ForeignKey("SectionId")] public virtual Section? Section { get; set; }
    }

    public class Line
    {
        [Key] public int Id { get; set; }

        [Required] [StringLength(100)] public string NameEn { get; set; } = string.Empty;

        [StringLength(100)] public string? NameBn { get; set; }

        public int? CompanyId { get; set; }

        [ForeignKey("CompanyId")] public virtual Company? Company { get; set; }

        public int? DepartmentId { get; set; }

        [ForeignKey("DepartmentId")] public virtual Department? Department { get; set; }

        public int SectionId { get; set; }

        [ForeignKey("SectionId")] public virtual Section? Section { get; set; }
    }

    public class Shift
    {
        [Key] public int Id { get; set; }
        [Required] [StringLength(100)] public string NameEn { get; set; } = string.Empty;
        [StringLength(100)] public string? NameBn { get; set; }
        [Required] [StringLength(10)] public string InTime { get; set; } = "09:00";
        [Required] [StringLength(10)] public string OutTime { get; set; } = "17:00";
        [StringLength(10)] public string? ActualInTime { get; set; }
        [StringLength(10)] public string? ActualOutTime { get; set; }
        [StringLength(10)] public string? LateInTime { get; set; }
        [StringLength(10)] public string? LunchTimeStart { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal LunchHour { get; set; } = 1.0m;
        [StringLength(200)] public string? Weekends { get; set; } // Comma separated days
        [StringLength(200)] public string? CompanyName { get; set; }
        public int? CompanyId { get; set; }
        [ForeignKey(nameof(CompanyId))] public virtual Company? Company { get; set; }
        [Required] [StringLength(20)] public string Status { get; set; } = "Active";
    }

    public class Group
    {
        [Key] public int Id { get; set; }
        [Required] [StringLength(100)] public string NameEn { get; set; } = string.Empty;
        [StringLength(100)] public string? NameBn { get; set; }
        [StringLength(200)] public string? CompanyName { get; set; }
        public int? CompanyId { get; set; }
        [ForeignKey(nameof(CompanyId))] public virtual Company? Company { get; set; }
    }

    public class Floor
    {
        [Key] public int Id { get; set; }
        [Required] [StringLength(100)] public string NameEn { get; set; } = string.Empty;
        [StringLength(100)] public string? NameBn { get; set; }
        [StringLength(200)] public string? CompanyName { get; set; }
        public int? CompanyId { get; set; }
        [ForeignKey(nameof(CompanyId))] public virtual Company? Company { get; set; }
    }
}
