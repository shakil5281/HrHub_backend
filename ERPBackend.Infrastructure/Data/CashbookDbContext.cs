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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Cashbook specific configurations will go here
        }
    }
}
