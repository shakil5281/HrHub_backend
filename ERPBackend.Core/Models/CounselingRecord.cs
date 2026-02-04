using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPBackend.Core.Entities;

namespace ERPBackend.Core.Models
{
    public class CounselingRecord
    {
        [Key] public int Id { get; set; }

        public int EmployeeId { get; set; }
        [ForeignKey(nameof(EmployeeId))] public virtual Employee? Employee { get; set; }

        public DateTime CounselingDate { get; set; }

        [Required] [StringLength(100)] public string IssueType { get; set; } = string.Empty; // Absenteeism, Late Coming, Performance, etc.

        [Required] [StringLength(1000)] public string Description { get; set; } = string.Empty;

        [StringLength(1000)] public string? ActionTaken { get; set; }

        [StringLength(500)] public string? FollowUpNotes { get; set; }

        [StringLength(50)] public string Status { get; set; } = "Open"; // Open, Closed, Follow-up Required

        [StringLength(50)] public string Severity { get; set; } = "Low"; // Low, Medium, High

        public DateTime? FollowUpDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
