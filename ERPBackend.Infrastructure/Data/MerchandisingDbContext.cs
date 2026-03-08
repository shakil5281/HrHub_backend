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
        public DbSet<StyleOrder> StyleOrders { get; set; } = null!;
        public DbSet<OrderColor> OrderColors { get; set; } = null!;
        public DbSet<OrderSizeBreakdown> OrderSizeBreakdowns { get; set; } = null!;
        public DbSet<BOM> BOMs { get; set; } = null!;
        public DbSet<BOMItem> BOMItems { get; set; } = null!;
        public DbSet<FabricBooking> FabricBookings { get; set; } = null!;
        public DbSet<AccessoriesBooking> AccessoriesBookings { get; set; } = null!;
        public DbSet<MerchProductionPlan> MerchProductionPlans { get; set; } = null!;
        public DbSet<Shipment> Shipments { get; set; } = null!;

        // Order Sheet System
        public DbSet<OrderSheet> OrderSheets { get; set; } = null!;
        public DbSet<OrderSheetItem> OrderSheetItems { get; set; } = null!;
        public DbSet<OrderSheetColor> OrderSheetColors { get; set; } = null!;
        public DbSet<OrderSheetSizeBreakdown> OrderSheetSizeBreakdowns { get; set; } = null!;

        // Master Setup
        public DbSet<Season> Seasons { get; set; } = null!;
        public DbSet<MerchandisingDepartment> MerchandisingDepartments { get; set; } = null!;
        public DbSet<LocalAgent> LocalAgents { get; set; } = null!;
        public DbSet<FabricColorPantone> FabricColorPantones { get; set; } = null!;
        public DbSet<FabricTypeGsm> FabricTypeGsms { get; set; } = null!;
        public DbSet<ShipmentModeTerms> ShipmentModeTerms { get; set; } = null!;
        public DbSet<PaymentModeTerms> PaymentModeTerms { get; set; } = null!;
        public DbSet<SupplierInfo> SupplierInfos { get; set; } = null!;
        public DbSet<CourierInfo> CourierInfos { get; set; } = null!;
        public DbSet<SizeName> SizeNames { get; set; } = null!;
        public DbSet<ExportItem> ExportItems { get; set; } = null!;

        // Knit Module
        public DbSet<KnitMachine> KnitMachines { get; set; } = null!;
        public DbSet<DyeingMachine> DyeingMachines { get; set; } = null!;
        public DbSet<YarnInventory> YarnInventories { get; set; } = null!;
        public DbSet<SubContractOrder> SubContractOrders { get; set; } = null!;

        // Consumption
        public DbSet<FabricConsumption> FabricConsumptions { get; set; } = null!;
        public DbSet<AccessoriesConsumption> AccessoriesConsumptions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Ignore tables that belong to other contexts
            builder.Ignore<Company>();
            builder.Ignore<ApplicationUser>();

            // Merchandising Configurations
            builder.Entity<Brand>()
                .HasOne(b => b.Buyer)
                .WithMany(b => b.Brands)
                .HasForeignKey(b => b.BuyerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Style>()
                .HasOne(s => s.Buyer)
                .WithMany()
                .HasForeignKey(s => s.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Style>()
                .HasOne(s => s.Brand)
                .WithMany()
                .HasForeignKey(s => s.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TechPack>()
                .HasOne(t => t.Style)
                .WithMany(s => s.TechPacks)
                .HasForeignKey(t => t.StyleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SampleRequest>()
                .HasOne(sr => sr.Style)
                .WithMany(s => s.SampleRequests)
                .HasForeignKey(sr => sr.StyleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Costing>()
                .HasOne(c => c.Style)
                .WithMany(s => s.Costings)
                .HasForeignKey(c => c.StyleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StyleOrder>()
                .HasOne(o => o.Buyer)
                .WithMany()
                .HasForeignKey(o => o.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StyleOrder>()
                .HasOne(o => o.Style)
                .WithMany()
                .HasForeignKey(o => o.StyleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderColor>()
                .HasOne(oc => oc.StyleOrder)
                .WithMany(o => o.OrderColors)
                .HasForeignKey(oc => oc.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderSizeBreakdown>()
                .HasOne(osb => osb.OrderColor)
                .WithMany(oc => oc.SizeBreakdowns)
                .HasForeignKey(osb => osb.OrderColorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<BOM>()
                .HasOne(b => b.StyleOrder)
                .WithMany(o => o.BOMs)
                .HasForeignKey(b => b.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<BOMItem>()
                .HasOne(bi => bi.BOM)
                .WithMany(b => b.BOMItems)
                .HasForeignKey(bi => bi.BOMId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<FabricBooking>()
                .HasOne(fb => fb.StyleOrder)
                .WithMany()
                .HasForeignKey(fb => fb.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AccessoriesBooking>()
                .HasOne(ab => ab.StyleOrder)
                .WithMany()
                .HasForeignKey(ab => ab.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MerchProductionPlan>()
                .HasOne(pp => pp.StyleOrder)
                .WithMany()
                .HasForeignKey(pp => pp.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Shipment>()
                .HasOne(s => s.StyleOrder)
                .WithMany()
                .HasForeignKey(s => s.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Order Sheet Configurations
            builder.Entity<OrderSheetItem>()
                .HasOne(i => i.OrderSheet)
                .WithMany(o => o.Items)
                .HasForeignKey(i => i.OrderSheetId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderSheetColor>()
                .HasOne(c => c.OrderSheetItem)
                .WithMany(i => i.Colors)
                .HasForeignKey(c => c.OrderSheetItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderSheetSizeBreakdown>()
                .HasOne(sb => sb.OrderSheetColor)
                .WithMany(c => c.SizeBreakdowns)
                .HasForeignKey(sb => sb.OrderSheetColorId)
                .OnDelete(DeleteBehavior.Cascade);

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
