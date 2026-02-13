using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPBackend.Core.Models
{
    public class ProductionAssignment
    {
        [Key]
        public int Id { get; set; }

        public int ProductionId { get; set; }

        [ForeignKey("ProductionId")]
        public virtual Production? Production { get; set; }

        public int LineId { get; set; }

        [ForeignKey("LineId")]
        public virtual ProductionLine? Line { get; set; }

        public int TotalTarget { get; set; }

        public DateTime AssignDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Completed, Paused

        public virtual ICollection<DailyProductionRecord> DailyRecords { get; set; } = new List<DailyProductionRecord>();
    }

    public class DailyProductionRecord
    {
        [Key]
        public int Id { get; set; }

        public int AssignmentId { get; set; }

        [ForeignKey("AssignmentId")]
        public virtual ProductionAssignment? Assignment { get; set; }

        public DateTime Date { get; set; }

        public int DailyTarget { get; set; }
        public int HourlyTarget { get; set; }

        // Hourly Outputs
        public int H1 { get; set; }
        public int H2 { get; set; }
        public int H3 { get; set; }
        public int H4 { get; set; }
        public int H5 { get; set; }
        public int H6 { get; set; }
        public int H7 { get; set; }
        public int H8 { get; set; }
        public int H9 { get; set; }
        public int H10 { get; set; }
        public int H11 { get; set; }
        public int H12 { get; set; }

        [NotMapped]
        public int TotalCompleted => H1 + H2 + H3 + H4 + H5 + H6 + H7 + H8 + H9 + H10 + H11 + H12;
    }
}
