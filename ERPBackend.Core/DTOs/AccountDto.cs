using System;
using System.Collections.Generic;

namespace ERPBackend.Core.DTOs
{
    public class BranchDto
    {
        public int Id { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string? BranchCode { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public decimal InitialBalance { get; set; }
        public decimal CurrentBalance { get; set; }
        public bool IsActive { get; set; }
    }

    public class AccountTransactionDto
    {
        public int Id { get; set; }
        public string TransactionNumber { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string Type { get; set; } = string.Empty; // Receive, Payment, Transfer, etc.
        public string FundSource { get; set; } = string.Empty; // Cash, Bank, HandCash
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        public decimal Amount { get; set; }
        public string? Category { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Description { get; set; }
        public string? PreparedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdvancePaymentDto
    {
        public int Id { get; set; }
        public string EmployeeOrContractorName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
        public string? PaymentType { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AccountDashboardSummaryDto
    {
        public decimal TotalCashBalance { get; set; }
        public decimal TotalBankBalance { get; set; }
        public decimal TotalHandCash { get; set; }
        public decimal TodaysReceive { get; set; }
        public decimal TodaysPayment { get; set; }
        public decimal ActiveAdvances { get; set; }
    }

    public class GeneralReportDto
    {
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Category { get; set; }
        public DateTime Date { get; set; }
    }
}
