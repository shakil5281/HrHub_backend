using ERPBackend.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace ERPBackend.Infrastructure.Data
{
    public class StoreDbContext : DbContext
    {
        public StoreDbContext(DbContextOptions<StoreDbContext> options) : base(options)
        {
        }

        public DbSet<StoreItem> StoreItems { get; set; } = null!;
        public DbSet<ItemCategory> ItemCategories { get; set; } = null!;
        public DbSet<StoreUnit> StoreUnits { get; set; } = null!;
        public DbSet<Buyer> Buyers { get; set; } = null!;
        public DbSet<StoreOrder> StoreOrders { get; set; } = null!;
        public DbSet<StoreOrderItem> StoreOrderItems { get; set; } = null!;
        public DbSet<StoreBooking> StoreBookings { get; set; } = null!;
        public DbSet<StockTransaction> StockTransactions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
