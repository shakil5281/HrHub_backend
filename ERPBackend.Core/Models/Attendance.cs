using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPBackend.Core.Entities;

namespace ERPBackend.Core.Models
{
    public class Attendance
    {
        [Key] public int Id { get; set; }

        public int EmployeeId { get; set; }
        [ForeignKey(nameof(EmployeeId))] public virtual Employee? Employee { get; set; }

        public DateTime Date { get; set; }

        [StringLength(10)] public string? InTime { get; set; }
        [StringLength(10)] public string? OutTime { get; set; }

        [Required] [StringLength(20)] public string Status { get; set; } = "Absent"; // Present, Late, Absent, On Leave, Off Day, Holiday

        [Column(TypeName = "decimal(18,2)")] public decimal OTHours { get; set; }

        [StringLength(100)] public string? Reason { get; set; } // Reason for manual entry
        [StringLength(500)] public string? Remarks { get; set; } // Additional remarks

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
