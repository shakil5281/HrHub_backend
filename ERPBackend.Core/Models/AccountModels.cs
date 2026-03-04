using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPBackend.Core.Models
{
    public class Branch
    {
        [Key]
        public int Id { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string? BranchCode { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal InitialBalance { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentBalance { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum AccountTransactionType
    {
        Receive,
        Payment,
        Transfer,
        Expense,
        Income,
        Advance
    }

    public enum FundType
    {
        Cash,
        Bank,
        HandCash
    }

    public class AccountTransaction
    {
        [Key]
        public int Id { get; set; }
        public string TransactionNumber { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public AccountTransactionType Type { get; set; }
        public FundType FundSource { get; set; }
        public int? BranchId { get; set; }
        [ForeignKey("BranchId")]
        public Branch? Branch { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public string? Category { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Description { get; set; }
        public string? PreparedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class AdvancePayment
    {
        [Key]
        public int Id { get; set; }
        public string EmployeeOrContractorName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal DueAmount { get; set; }
        public string? PaymentType { get; set; } // Advance, Contractual, Partial
        public string? Status { get; set; } // Pending, Settled
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class IncomeCategory
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class ExpenseCategory
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
