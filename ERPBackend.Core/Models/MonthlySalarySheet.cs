using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPBackend.Core.Entities;

namespace ERPBackend.Core.Models
{
    public class MonthlySalarySheet
    {
        [Key]
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        [ForeignKey(nameof(EmployeeId))]
        public virtual Employee? Employee { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GrossSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BasicSalary { get; set; }

        public int TotalDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LeaveDays { get; set; }
        public int Holidays { get; set; }
        public int WeekendDays { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OTHours { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OTRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OTAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AttendanceBonus { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OtherAllowances { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalEarning { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AbsentDeduction { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AdvanceDeduction { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OTDeduction { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDeduction { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetPayable { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Draft"; // Draft, Processed, Approved, Paid

        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
        public string? ProcessedBy { get; set; }
    }
}
