using System;
using System.ComponentModel.DataAnnotations;

namespace ERPBackend.Core.Models
{
    public class ProductionLine
    {
        [Key]
        public int Id { get; set; }

        public int SL { get; set; }

        [Required]
        [StringLength(100)]
        public string LineName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Inactive

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
