using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPBackend.Core.Entities;

namespace ERPBackend.Core.Models
{
    public class DailySalarySheet
    {
        [Key]
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        [ForeignKey(nameof(EmployeeId))]
        public virtual Employee? Employee { get; set; }

        public DateTime Date { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GrossSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PerDaySalary { get; set; }

        [Required]
        [StringLength(20)]
        public string AttendanceStatus { get; set; } = string.Empty; // Present, Absent, Leave, Holiday

        [Column(TypeName = "decimal(18,2)")]
        public decimal OTHours { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OTAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalEarning { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Deduction { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetPayable { get; set; }

        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}
