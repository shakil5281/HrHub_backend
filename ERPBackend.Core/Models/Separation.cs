using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPBackend.Core.Entities;

namespace ERPBackend.Core.Models
{
    public class Separation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; } = null!;

        [Required]
        public DateTime LastWorkingDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = "Resignation"; // Resignation, Termination, Retirement, etc.

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = null!;

        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        [StringLength(500)]
        public string? AdminRemark { get; set; }

        public bool IsSettled { get; set; } = false;
        public DateTime? SettledAt { get; set; }

        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
    }
}
