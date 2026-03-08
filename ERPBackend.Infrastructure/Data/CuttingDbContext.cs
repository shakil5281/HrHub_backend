using ERPBackend.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace ERPBackend.Infrastructure.Data
{
    public class CuttingDbContext : DbContext
    {
        public CuttingDbContext(DbContextOptions<CuttingDbContext> options) : base(options)
        {
        }

        public DbSet<CuttingPlan> CuttingPlans { get; set; } = null!;
        public DbSet<MarkerLayout> MarkerLayouts { get; set; } = null!;
        public DbSet<CuttingBatch> CuttingBatches { get; set; } = null!;
        public DbSet<CuttingBatchItem> CuttingBatchItems { get; set; } = null!;
        public DbSet<Bundle> Bundles { get; set; } = null!;
        public DbSet<WastageRecord> WastageRecords { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Ignore<Company>();
            builder.Ignore<ApplicationUser>();

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
