using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPBackend.Core.Entities;

namespace ERPBackend.Core.Models
{
    public class Bonus
    {
        [Key]
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        [ForeignKey(nameof(EmployeeId))]
        public virtual Employee? Employee { get; set; }

        [StringLength(100)]
        public string BonusType { get; set; } = string.Empty; // Eid-ul-Fitr, Eid-ul-Adha, Performance

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public int Year { get; set; }
        public int Month { get; set; } // The month it will be paid

        public DateTime PaymentDate { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Draft"; // Draft, Approved, Paid

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
