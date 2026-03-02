using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPBackend.Core.Models
{
    public class Expense
    {
        [Key]
        public int Id { get; set; }

        public DateTime ExpenseDate { get; set; }

        [Required]
        [StringLength(100)]
        public required string Category { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [StringLength(100)]
        public string? PaymentMethod { get; set; }

        [StringLength(100)]
        public string? ReferenceNumber { get; set; }

        public string? Description { get; set; }

        [StringLength(100)]
        public string? Branch { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
