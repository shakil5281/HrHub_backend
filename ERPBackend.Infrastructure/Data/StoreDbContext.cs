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
        public DbSet<StoreOrder> StoreOrders { get; set; } = null!;
        public DbSet<StoreOrderItem> StoreOrderItems { get; set; } = null!;
        public DbSet<StoreBooking> StoreBookings { get; set; } = null!;
        public DbSet<StockTransaction> StockTransactions { get; set; } = null!;

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
