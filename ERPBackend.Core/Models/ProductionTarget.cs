using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPBackend.Core.Models
{
    public class ProductionTarget
    {
        [Key]
        public int Id { get; set; }

        public int AssignmentId { get; set; }

        [ForeignKey("AssignmentId")]
        public virtual ProductionAssignment? Assignment { get; set; }

        public DateTime TargetDate { get; set; }

        public int DailyTarget { get; set; }
        public int HourlyTarget { get; set; }

        [StringLength(200)]
        public string Remarks { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
