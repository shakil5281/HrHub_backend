using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPBackend.Core.Models
{
    public class FundTransfer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public required string FromBranch { get; set; }

        [Required]
        [StringLength(100)]
        public required string ToBranch { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal RequestedAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ApprovedAmount { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Completed, Rejected

        public DateTime RequestDate { get; set; } = DateTime.Now;
        public DateTime? ApprovedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
    }
}
