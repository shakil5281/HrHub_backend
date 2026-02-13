using ERPBackend.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace ERPBackend.Infrastructure.Data
{
    public class ProductionDbContext : DbContext
    {
        public ProductionDbContext(DbContextOptions<ProductionDbContext> options) : base(options)
        {
        }

        public DbSet<ProductionLine> ProductionLines { get; set; } = null!;
        public DbSet<Production> Productions { get; set; } = null!;
        public DbSet<ProductionColor> ProductionColors { get; set; } = null!;
        public DbSet<ProductionAssignment> ProductionAssignments { get; set; } = null!;
        public DbSet<DailyProductionRecord> DailyProductionRecords { get; set; } = null!;
        public DbSet<ProductionTarget> ProductionTargets { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Set StyleNo as unique as requested
            builder.Entity<Production>()
                .HasIndex(p => p.StyleNo)
                .IsUnique();

            // Configure one-to-many relationship
            builder.Entity<Production>()
                .HasMany(p => p.Colors)
                .WithOne(c => c.Production)
                .HasForeignKey(c => c.ProductionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
