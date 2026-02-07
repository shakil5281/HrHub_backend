using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPBackend.Core.Models
{
    public class CashTransaction
    {
        [Key]
        public int Id { get; set; }

        public DateTime TransactionDate { get; set; }

        [StringLength(50)]
        public required string TransactionType { get; set; } // "Received" or "Expense"

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [StringLength(100)]
        public string? PaymentMethod { get; set; } // "Cash", "Bank", "Cheque"

        [StringLength(100)]
        public string? ReferenceNumber { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [StringLength(100)]
        public string? Branch { get; set; } // To support branch-specific balance
    }
}
