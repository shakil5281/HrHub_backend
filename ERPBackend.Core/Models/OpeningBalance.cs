using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPBackend.Core.Models
{
    public class OpeningBalance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public required string AccountName { get; set; }

        [Required]
        [StringLength(50)]
        public required string Category { get; set; } // Cash, Bank, Mobile Banking, Other

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime Date { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
