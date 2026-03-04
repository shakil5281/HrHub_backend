using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPBackend.Core.DTOs;
using ERPBackend.Core.Interfaces;
using ERPBackend.Core.Models;
using ERPBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPBackend.Services.Services
{
    public class AccountService : IAccountService
    {
        private readonly CashbookDbContext _context;

        public AccountService(CashbookDbContext context)
        {
            _context = context;
        }

        #region Branch Management
        public async Task<List<BranchDto>> GetBranchesAsync()
        {
            return await _context.Branches
                .Select(b => new BranchDto
                {
                    Id = b.Id,
                    BranchName = b.BranchName,
                    BranchCode = b.BranchCode,
                    Address = b.Address,
                    Phone = b.Phone,
                    InitialBalance = b.InitialBalance,
                    CurrentBalance = b.CurrentBalance,
                    IsActive = b.IsActive
                }).ToListAsync();
        }

        public async Task<BranchDto> AddBranchAsync(BranchDto branch)
        {
            var entity = new Branch
            {
                BranchName = branch.BranchName,
                BranchCode = branch.BranchCode,
                Address = branch.Address,
                Phone = branch.Phone,
                InitialBalance = branch.InitialBalance,
                CurrentBalance = branch.InitialBalance, // Start with initial
                IsActive = true
            };
            _context.Branches.Add(entity);
            await _context.SaveChangesAsync();
            branch.Id = entity.Id;
            return branch;
        }

        public async Task<bool> UpdateBranchAsync(BranchDto branch)
        {
            var entity = await _context.Branches.FindAsync(branch.Id);
            if (entity == null) return false;

            entity.BranchName = branch.BranchName;
            entity.BranchCode = branch.BranchCode;
            entity.Address = branch.Address;
            entity.Phone = branch.Phone;
            entity.IsActive = branch.IsActive;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteBranchAsync(int id)
        {
            var entity = await _context.Branches.FindAsync(id);
            if (entity == null) return false;
            _context.Branches.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Transactions
        public async Task<List<AccountTransactionDto>> GetTransactionsAsync(string? type = null, string? fundSource = null)
        {
            var query = _context.AccountTransactions.Include(t => t.Branch).AsQueryable();

            if (!string.IsNullOrEmpty(type))
            {
                if (Enum.TryParse<AccountTransactionType>(type, true, out var tEnum))
                    query = query.Where(t => t.Type == tEnum);
            }

            if (!string.IsNullOrEmpty(fundSource))
            {
                if (Enum.TryParse<FundType>(fundSource, true, out var fEnum))
                    query = query.Where(t => t.FundSource == fEnum);
            }

            return await query
                .OrderByDescending(t => t.TransactionDate)
                .Select(t => new AccountTransactionDto
                {
                    Id = t.Id,
                    TransactionNumber = t.TransactionNumber,
                    TransactionDate = t.TransactionDate,
                    Type = t.Type.ToString(),
                    FundSource = t.FundSource.ToString(),
                    BranchId = t.BranchId,
                    BranchName = t.Branch != null ? t.Branch.BranchName : "",
                    Amount = t.Amount,
                    Category = t.Category,
                    ReferenceNumber = t.ReferenceNumber,
                    Description = t.Description,
                    PreparedBy = t.PreparedBy,
                    CreatedAt = t.CreatedAt
                }).ToListAsync();
        }

        public async Task<AccountTransactionDto> CreateTransactionAsync(AccountTransactionDto transaction)
        {
            if (!Enum.TryParse<AccountTransactionType>(transaction.Type, true, out var typeEnum))
                throw new Exception("Invalid transaction type");

            if (!Enum.TryParse<FundType>(transaction.FundSource, true, out var fundEnum))
                throw new Exception("Invalid fund source");

            var entity = new AccountTransaction
            {
                TransactionNumber = "TRX-" + DateTime.Now.Ticks.ToString().Substring(10),
                TransactionDate = transaction.TransactionDate,
                Type = typeEnum,
                FundSource = fundEnum,
                BranchId = transaction.BranchId,
                Amount = transaction.Amount,
                Category = transaction.Category,
                ReferenceNumber = transaction.ReferenceNumber,
                Description = transaction.Description,
                PreparedBy = transaction.PreparedBy
            };

            // Update Branch Balance if applicable
            if (transaction.BranchId.HasValue)
            {
                var branch = await _context.Branches.FindAsync(transaction.BranchId.Value);
                if (branch != null)
                {
                    if (typeEnum == AccountTransactionType.Receive || typeEnum == AccountTransactionType.Income)
                        branch.CurrentBalance += transaction.Amount;
                    else if (typeEnum == AccountTransactionType.Payment || typeEnum == AccountTransactionType.Expense)
                        branch.CurrentBalance -= transaction.Amount;
                }
            }

            _context.AccountTransactions.Add(entity);
            await _context.SaveChangesAsync();
            transaction.Id = entity.Id;
            transaction.TransactionNumber = entity.TransactionNumber;
            return transaction;
        }
        #endregion

        #region Advance Payments
        public async Task<List<AdvancePaymentDto>> GetAdvancesAsync()
        {
            return await _context.AdvancePayments
                .Select(a => new AdvancePaymentDto
                {
                    Id = a.Id,
                    EmployeeOrContractorName = a.EmployeeOrContractorName,
                    Date = a.Date,
                    TotalAmount = a.TotalAmount,
                    PaidAmount = a.PaidAmount,
                    DueAmount = a.DueAmount,
                    PaymentType = a.PaymentType,
                    Status = a.Status,
                    CreatedAt = a.CreatedAt
                }).ToListAsync();
        }

        public async Task<AdvancePaymentDto> CreateAdvanceAsync(AdvancePaymentDto advance)
        {
            var entity = new AdvancePayment
            {
                EmployeeOrContractorName = advance.EmployeeOrContractorName,
                Date = advance.Date,
                TotalAmount = advance.TotalAmount,
                PaidAmount = advance.PaidAmount,
                DueAmount = advance.TotalAmount - advance.PaidAmount,
                PaymentType = advance.PaymentType,
                Status = (advance.TotalAmount - advance.PaidAmount) <= 0 ? "Settled" : "Pending"
            };
            _context.AdvancePayments.Add(entity);
            await _context.SaveChangesAsync();
            advance.Id = entity.Id;
            return advance;
        }
        #endregion

        #region Categories
        public async Task<List<string>> GetIncomeCategoriesAsync()
        {
            return await _context.IncomeCategories.Where(c => c.IsActive).Select(c => c.Name).ToListAsync();
        }

        public async Task<List<string>> GetExpenseCategoriesAsync()
        {
            return await _context.ExpenseCategories.Where(c => c.IsActive).Select(c => c.Name).ToListAsync();
        }
        #endregion

        #region Dashboard & Reports
        public async Task<AccountDashboardSummaryDto> GetDashboardSummaryAsync()
        {
            var today = DateTime.Today;
            return new AccountDashboardSummaryDto
            {
                TotalCashBalance = await _context.AccountTransactions.Where(t => t.FundSource == FundType.Cash).SumAsync(t => (t.Type == AccountTransactionType.Receive || t.Type == AccountTransactionType.Income ? 1 : -1) * t.Amount),
                TotalBankBalance = await _context.AccountTransactions.Where(t => t.FundSource == FundType.Bank).SumAsync(t => (t.Type == AccountTransactionType.Receive || t.Type == AccountTransactionType.Income ? 1 : -1) * t.Amount),
                TotalHandCash = await _context.AccountTransactions.Where(t => t.FundSource == FundType.HandCash).SumAsync(t => (t.Type == AccountTransactionType.Receive || t.Type == AccountTransactionType.Income ? 1 : -1) * t.Amount),
                TodaysReceive = await _context.AccountTransactions.Where(t => t.TransactionDate.Date == today && (t.Type == AccountTransactionType.Receive || t.Type == AccountTransactionType.Income)).SumAsync(t => t.Amount),
                TodaysPayment = await _context.AccountTransactions.Where(t => t.TransactionDate.Date == today && (t.Type == AccountTransactionType.Payment || t.Type == AccountTransactionType.Expense)).SumAsync(t => t.Amount),
                ActiveAdvances = await _context.AdvancePayments.Where(a => a.Status == "Pending").SumAsync(a => a.DueAmount)
            };
        }

        public async Task<List<GeneralReportDto>> GetLedgerReportAsync(int? branchId, string? fundSource)
        {
            var query = _context.AccountTransactions.AsQueryable();
            if (branchId.HasValue) query = query.Where(t => t.BranchId == branchId.Value);
            
            if (!string.IsNullOrEmpty(fundSource) && Enum.TryParse<FundType>(fundSource, true, out var fEnum))
                query = query.Where(t => t.FundSource == fEnum);

            return await query.Select(t => new GeneralReportDto
            {
                Title = t.Description ?? t.TransactionNumber,
                Amount = (t.Type == AccountTransactionType.Receive || t.Type == AccountTransactionType.Income ? 1 : -1) * t.Amount,
                Category = t.Category,
                Date = t.TransactionDate
            }).ToListAsync();
        }
        #endregion
    }
}
