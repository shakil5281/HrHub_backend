using System;
using System.Collections.Generic;

namespace ERPBackend.Core.DTOs
{
    public class CashTransactionDto
    {
        public int Id { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? TransactionType { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Description { get; set; }
        public string? Branch { get; set; }
    }

    public class AccountsSummaryDto
    {
        public decimal TotalReceived { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal CurrentBalance { get; set; }
        public List<BranchBalanceDto> BranchBalances { get; set; } = new();
    }

    public class BranchBalanceDto
    {
        public string? BranchName { get; set; }
        public decimal Balance { get; set; }
    }

    public class BalanceSheetDto
    {
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
        public decimal NetWorth { get; set; }
    }
}
