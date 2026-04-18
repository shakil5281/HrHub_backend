using ERPBackend.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace ERPBackend.Infrastructure.Data
{
    public class MerchandisingDbContext : DbContext
    {
        public MerchandisingDbContext(DbContextOptions<MerchandisingDbContext> options) : base(options)
        {
        }

        public DbSet<Buyer> Buyers { get; set; } = null!;
        public DbSet<Brand> Brands { get; set; } = null!;
        public DbSet<Style> Styles { get; set; } = null!;
        public DbSet<TechPack> TechPacks { get; set; } = null!;
        public DbSet<SampleRequest> SampleRequests { get; set; } = null!;
        public DbSet<Costing> Costings { get; set; } = null!;
        
        public DbSet<ProgramOrder> ProgramOrders { get; set; } = null!;
        public DbSet<ProgramArticle> ProgramArticles { get; set; } = null!;
        public DbSet<ProgramColor> ProgramColors { get; set; } = null!;
        public DbSet<ProgramSizeBreakdown> ProgramSizeBreakdowns { get; set; } = null!;

        // Accessories (Seperated Tables)
        public DbSet<FabricBooking> FabricBookings { get; set; } = null!;
        public DbSet<ButtonBooking> ButtonBookings { get; set; } = null!;
        public DbSet<ZipperBooking> ZipperBookings { get; set; } = null!;
        public DbSet<SnapButtonBooking> SnapButtonBookings { get; set; } = null!;
        public DbSet<MainLabelBooking> MainLabelBookings { get; set; } = null!;
        public DbSet<CareLabelBooking> CareLabelBookings { get; set; } = null!;
        public DbSet<PolyBooking> PolyBookings { get; set; } = null!;
        public DbSet<ThreadBooking> ThreadBookings { get; set; } = null!;
        public DbSet<ProgramAccessoryRequirement> ProgramAccessoryRequirements { get; set; } = null!;

        // Master Setup
        public DbSet<Season> Seasons { get; set; } = null!;
        public DbSet<MerchandisingDepartment> MerchandisingDepartments { get; set; } = null!;
        public DbSet<LocalAgent> LocalAgents { get; set; } = null!;
        public DbSet<FabricColorPantone> FabricColorPantones { get; set; } = null!;
        public DbSet<FabricTypeGsm> FabricTypeGsms { get; set; } = null!;
        public DbSet<SupplierInfo> SupplierInfos { get; set; } = null!;
        public DbSet<CourierInfo> CourierInfos { get; set; } = null!;
        public DbSet<ShipmentModeTerms> ShipmentModeTerms { get; set; } = null!;
        public DbSet<PaymentModeTerms> PaymentModeTerms { get; set; } = null!;
        public DbSet<SizeName> SizeNames { get; set; } = null!;

        // Knit & Dyeing Master
        public DbSet<KnitMachine> KnitMachines { get; set; } = null!;
        public DbSet<DyeingMachine> DyeingMachines { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Ignore<Company>();
            builder.Ignore<ApplicationUser>();

            // Relationships
            builder.Entity<ProgramArticle>()
                .HasOne(i => i.ProgramOrder)
                .WithMany(o => o.Articles)
                .HasForeignKey(i => i.ProgramOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProgramColor>()
                .HasOne(c => c.ProgramArticle)
                .WithMany(i => i.Colors)
                .HasForeignKey(c => c.ProgramArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProgramSizeBreakdown>()
                .HasOne(sb => sb.ProgramColor)
                .WithMany(c => c.SizeBreakdowns)
                .HasForeignKey(sb => sb.ProgramColorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Accessories relationship with ProgramSizeBreakdown and ProgramOrder
            // Using Restrict or NoAction to avoid circular cascade paths in SQL Server
            builder.Entity<ButtonBooking>(entity =>
            {
                entity.HasOne(b => b.ProgramOrder)
                    .WithMany(o => o.Buttons)
                    .HasForeignKey(b => b.ProgramOrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.ProgramSizeBreakdown)
                    .WithMany()
                    .HasForeignKey(b => b.ProgramSizeBreakdownId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ZipperBooking>(entity =>
            {
                entity.HasOne(b => b.ProgramOrder)
                    .WithMany(o => o.Zippers)
                    .HasForeignKey(b => b.ProgramOrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.ProgramSizeBreakdown)
                    .WithMany()
                    .HasForeignKey(b => b.ProgramSizeBreakdownId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<SnapButtonBooking>(entity =>
            {
                entity.HasOne(b => b.ProgramOrder)
                    .WithMany(o => o.SnapButtons)
                    .HasForeignKey(b => b.ProgramOrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.ProgramSizeBreakdown)
                    .WithMany()
                    .HasForeignKey(b => b.ProgramSizeBreakdownId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<MainLabelBooking>(entity =>
            {
                entity.HasOne(b => b.ProgramOrder)
                    .WithMany(o => o.MainLabels)
                    .HasForeignKey(b => b.ProgramOrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.ProgramSizeBreakdown)
                    .WithMany()
                    .HasForeignKey(b => b.ProgramSizeBreakdownId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<CareLabelBooking>(entity =>
            {
                entity.HasOne(b => b.ProgramOrder)
                    .WithMany(o => o.CareLabels)
                    .HasForeignKey(b => b.ProgramOrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.ProgramSizeBreakdown)
                    .WithMany()
                    .HasForeignKey(b => b.ProgramSizeBreakdownId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<PolyBooking>(entity =>
            {
                entity.HasOne(b => b.ProgramOrder)
                    .WithMany(o => o.PolyBookings)
                    .HasForeignKey(b => b.ProgramOrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.ProgramSizeBreakdown)
                    .WithMany()
                    .HasForeignKey(b => b.ProgramSizeBreakdownId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ThreadBooking>(entity =>
            {
                entity.HasOne(b => b.ProgramOrder)
                    .WithMany(o => o.Threads)
                    .HasForeignKey(b => b.ProgramOrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.ProgramSizeBreakdown)
                    .WithMany()
                    .HasForeignKey(b => b.ProgramSizeBreakdownId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ProgramOrder>()
                .HasIndex(o => new { o.CompanyId, o.ProgramNumber })
                .IsUnique();

            // Decimal Precision
            foreach (var property in builder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetPrecision(18);
                property.SetScale(4);
            }
        }
    }
}
