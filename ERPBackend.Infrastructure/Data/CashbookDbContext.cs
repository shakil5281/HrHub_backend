using Microsoft.EntityFrameworkCore;
using ERPBackend.Core.Models;

namespace ERPBackend.Infrastructure.Data
{
    public class CashbookDbContext : DbContext
    {
        public CashbookDbContext(DbContextOptions<CashbookDbContext> options) : base(options)
        {
        }

        public DbSet<CashTransaction> CashTransactions { get; set; }
        public DbSet<FundTransfer> FundTransfers { get; set; }
        public DbSet<OpeningBalance> OpeningBalances { get; set; }
        public DbSet<Expense> Expenses { get; set; }

        // New Account Module Tables
        public DbSet<Branch> Branches { get; set; }
        public DbSet<AccountTransaction> AccountTransactions { get; set; }
        public DbSet<AdvancePayment> AdvancePayments { get; set; }
        public DbSet<IncomeCategory> IncomeCategories { get; set; }
        public DbSet<ExpenseCategory> ExpenseCategories { get; set; }

        // Cutting Module Tables
        public DbSet<CuttingPlan> CuttingPlans { get; set; }
        public DbSet<FabricBooking> FabricBookings { get; set; }
        public DbSet<MarkerLayout> MarkerLayouts { get; set; }
        public DbSet<CuttingBatch> CuttingBatches { get; set; }
        public DbSet<CuttingBatchItem> CuttingBatchItems { get; set; }
        public DbSet<Bundle> Bundles { get; set; }
        public DbSet<WastageRecord> WastageRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure decimal precision
            foreach (var property in builder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetPrecision(18);
                property.SetScale(2);
            }
        }
    }
}
