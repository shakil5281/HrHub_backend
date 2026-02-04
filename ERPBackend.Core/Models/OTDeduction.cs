using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPBackend.Core.Entities;

namespace ERPBackend.Core.Models
{
    public class OTDeduction
    {
        [Key]
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        [ForeignKey(nameof(EmployeeId))]
        public virtual Employee? Employee { get; set; }

        public DateTime Date { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DeductionHours { get; set; }

        [Required]
        [StringLength(200)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Remarks { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Approved"; // Approved, Pending, Rejected

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
