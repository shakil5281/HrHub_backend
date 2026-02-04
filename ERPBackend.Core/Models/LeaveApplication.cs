using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPBackend.Core.Entities;

namespace ERPBackend.Core.Models
{
    public class LeaveApplication
    {
        [Key]
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        [ForeignKey(nameof(EmployeeId))]
        public virtual Employee? Employee { get; set; }

        public int LeaveTypeId { get; set; }
        [ForeignKey(nameof(LeaveTypeId))]
        public virtual LeaveType? LeaveType { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        [Column(TypeName = "decimal(18,1)")]
        public decimal TotalDays { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Cancelled

        public DateTime AppliedDate { get; set; } = DateTime.UtcNow;

        public int? ApprovedById { get; set; }
        public DateTime? ActionDate { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        public string? AttachmentUrl { get; set; }
    }
}
