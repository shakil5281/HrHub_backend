using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPBackend.Core.Entities;

namespace ERPBackend.Core.Models
{
    public class EmployeePunishment
    {
        [Key] public int Id { get; set; }

        public int EmployeeId { get; set; }
        [ForeignKey(nameof(EmployeeId))] public virtual Employee? Employee { get; set; }

        [Required]
        [StringLength(100)]
        public string PunishmentType { get; set; } = string.Empty; // Warning, Fine, Suspension, Termination, etc.

        [Required]
        [StringLength(1000)]
        public string Reason { get; set; } = string.Empty;

        public decimal FineAmount { get; set; } = 0;
        public int SuspensionDays { get; set; } = 0;

        public DateTime PunishmentDate { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Active"; // Active, Revoked, Completed

        [StringLength(1000)]
        public string? Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
