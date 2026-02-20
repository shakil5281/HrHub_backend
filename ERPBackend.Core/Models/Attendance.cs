using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPBackend.Core.Entities;

namespace ERPBackend.Core.Models
{
    public class Attendance
    {
        [Key] public int Id { get; set; }

        public int EmployeeCard { get; set; }
        [ForeignKey(nameof(EmployeeCard))] public virtual Employee? Employee { get; set; }

        public int? CompanyId { get; set; }
        [ForeignKey(nameof(CompanyId))] public virtual Company? Company { get; set; }

        [StringLength(50)] public string? EmployeeId { get; set; }

        public DateTime Date { get; set; }

        public DateTime? InTime { get; set; }
        public DateTime? OutTime { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Absent"; // Present, Late, Absent, On Leave, Off Day, Holiday

        [Column(TypeName = "decimal(18,2)")] public decimal OTHours { get; set; }
        
        public int? ShiftId { get; set; }
        [ForeignKey(nameof(ShiftId))] public virtual Shift? Shift { get; set; }
        public bool IsOffDay { get; set; }

        [StringLength(100)] public string? Reason { get; set; } // Reason for manual entry
        [StringLength(500)] public string? Remarks { get; set; } // Additional remarks

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public bool IsManual { get; set; } = false;
    }
}
