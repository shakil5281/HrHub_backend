using System.Collections.Generic;
using System.Threading.Tasks;
using ERPBackend.Core.DTOs;

namespace ERPBackend.Core.Interfaces
{
    public interface IAccountService
    {
        // Branch Management
        Task<List<BranchDto>> GetBranchesAsync();
        Task<BranchDto> AddBranchAsync(BranchDto branch);
        Task<bool> UpdateBranchAsync(BranchDto branch);
        Task<bool> DeleteBranchAsync(int id);

        // Transactions
        Task<List<AccountTransactionDto>> GetTransactionsAsync(string? type = null, string? fundSource = null);
        Task<AccountTransactionDto> CreateTransactionAsync(AccountTransactionDto transaction);
        
        // Advance Payments
        Task<List<AdvancePaymentDto>> GetAdvancesAsync();
        Task<AdvancePaymentDto> CreateAdvanceAsync(AdvancePaymentDto advance);

        // Categories
        Task<List<string>> GetIncomeCategoriesAsync();
        Task<List<string>> GetExpenseCategoriesAsync();

        // Dashboard & Reports
        Task<AccountDashboardSummaryDto> GetDashboardSummaryAsync();
        Task<List<GeneralReportDto>> GetLedgerReportAsync(int? branchId, string? fundSource);
    }
}
