using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPBackend.Core.Entities;

namespace ERPBackend.Core.Models
{
    public class SalaryIncrement
    {
        [Key]
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        [ForeignKey(nameof(EmployeeId))]
        public virtual Employee? Employee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PreviousGrossSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal IncrementAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NewGrossSalary { get; set; }

        public DateTime EffectiveDate { get; set; }

        [StringLength(100)]
        public string IncrementType { get; set; } = "Yearly"; // Yearly, Promotion, Adjustment

        [StringLength(500)]
        public string? Remarks { get; set; }

        public bool IsApplied { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
