using ERPBackend.Core.Interfaces;
using ERPBackend.Core.Models;
using ERPBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERPBackend.Services.Services
{
    public class MerchandisingService : IMerchandisingService
    {
        private readonly MerchandisingDbContext _context;

        public MerchandisingService(MerchandisingDbContext context) => _context = context;

        // Buyer Management
        public async Task<IEnumerable<Buyer>> GetAllBuyersAsync(int companyId) => await _context.Buyers.Where(b => b.CompanyId == companyId).ToListAsync();
        public async Task<Buyer?> GetBuyerByIdAsync(int id) => await _context.Buyers.FindAsync(id);
        public async Task<Buyer> CreateBuyerAsync(Buyer buyer) { _context.Buyers.Add(buyer); await _context.SaveChangesAsync(); return buyer; }
        public async Task UpdateBuyerAsync(Buyer buyer) { _context.Entry(buyer).State = EntityState.Modified; await _context.SaveChangesAsync(); }
        public async Task DeleteBuyerAsync(int id) { var b = await _context.Buyers.FindAsync(id); if (b != null) { _context.Buyers.Remove(b); await _context.SaveChangesAsync(); } }

        // Style Management
        public async Task<IEnumerable<Style>> GetStylesByBuyerAsync(int buyerId) => await _context.Styles.Where(s => s.BuyerId == buyerId).ToListAsync();
        public async Task<Style?> GetStyleByIdAsync(int id) => await _context.Styles.FindAsync(id);
        public async Task<Style> CreateStyleAsync(Style style) { _context.Styles.Add(style); await _context.SaveChangesAsync(); return style; }
        public async Task UpdateStyleAsync(Style style) { _context.Entry(style).State = EntityState.Modified; await _context.SaveChangesAsync(); }
        public async Task DeleteStyleAsync(int id) { var s = await _context.Styles.FindAsync(id); if (s != null) { _context.Styles.Remove(s); await _context.SaveChangesAsync(); } }

        // Brand Management
        public async Task<IEnumerable<Brand>> GetAllBrandsAsync(int companyId) => await _context.Brands.Include(b => b.Buyer).Where(b => b.Buyer!.CompanyId == companyId).ToListAsync();
        public async Task<IEnumerable<Brand>> GetBrandsByBuyerAsync(int buyerId) => await _context.Brands.Where(b => b.BuyerId == buyerId).ToListAsync();
        public async Task<Brand> CreateBrandAsync(Brand brand) { _context.Brands.Add(brand); await _context.SaveChangesAsync(); return brand; }
        public async Task UpdateBrandAsync(Brand brand) { _context.Entry(brand).State = EntityState.Modified; await _context.SaveChangesAsync(); }
        public async Task DeleteBrandAsync(int id) { var b = await _context.Brands.FindAsync(id); if (b != null) { _context.Brands.Remove(b); await _context.SaveChangesAsync(); } }

        // Tech Pack Management
        public async Task<IEnumerable<TechPack>> GetAllTechPacksAsync(int companyId) => await _context.TechPacks.Include(t => t.Style).Where(t => t.Style!.CompanyId == companyId).ToListAsync();
        public async Task<TechPack> CreateTechPackAsync(TechPack techPack) { _context.TechPacks.Add(techPack); await _context.SaveChangesAsync(); return techPack; }

        // MODERN RELATIONAL BOOKINGS (Linked to ProgramOrder)
        public async Task<IEnumerable<FabricBooking>> GetFabricBookingsByProgramAsync(int programId) => await _context.FabricBookings.Where(b => b.ProgramOrderId == programId).ToListAsync();
        public async Task<IEnumerable<FabricBooking>> GetAllFabricBookingsAsync(int companyId) => await _context.FabricBookings.Include(b => b.ProgramOrder).Where(b => b.ProgramOrder!.CompanyId == companyId).ToListAsync();
        public async Task<FabricBooking> CreateFabricBookingAsync(FabricBooking booking) { _context.FabricBookings.Add(booking); await _context.SaveChangesAsync(); return booking; }

        public async Task<IEnumerable<ButtonBooking>> GetButtonBookingsByProgramAsync(int programId) => await _context.ButtonBookings.Where(b => b.ProgramOrderId == programId).ToListAsync();
        public async Task<IEnumerable<ButtonBooking>> GetAllButtonBookingsAsync(int companyId) => await _context.ButtonBookings.Include(b => b.ProgramOrder).Where(b => b.ProgramOrder!.CompanyId == companyId).ToListAsync();
        public async Task<ButtonBooking> CreateButtonBookingAsync(ButtonBooking booking) { _context.ButtonBookings.Add(booking); await _context.SaveChangesAsync(); return booking; }
        public async Task UpdateButtonBookingAsync(ButtonBooking booking) { _context.Entry(booking).State = EntityState.Modified; await _context.SaveChangesAsync(); }
        public async Task DeleteButtonBookingAsync(int id) { var b = await _context.ButtonBookings.FindAsync(id); if (b != null) { _context.ButtonBookings.Remove(b); await _context.SaveChangesAsync(); } }

        public async Task<IEnumerable<ZipperBooking>> GetAllZipperBookingsAsync(int companyId) => await _context.ZipperBookings.Include(b => b.ProgramOrder).Where(b => b.ProgramOrder!.CompanyId == companyId).ToListAsync();
        public async Task<ZipperBooking> CreateZipperBookingAsync(ZipperBooking booking) { _context.ZipperBookings.Add(booking); await _context.SaveChangesAsync(); return booking; }

        public async Task<IEnumerable<SnapButtonBooking>> GetAllSnapButtonBookingsAsync(int companyId) => await _context.SnapButtonBookings.Include(b => b.ProgramOrder).Where(b => b.ProgramOrder!.CompanyId == companyId).ToListAsync();
        public async Task<SnapButtonBooking> CreateSnapButtonBookingAsync(SnapButtonBooking booking) { _context.SnapButtonBookings.Add(booking); await _context.SaveChangesAsync(); return booking; }

        public async Task<IEnumerable<MainLabelBooking>> GetAllMainLabelBookingsAsync(int companyId) => await _context.MainLabelBookings.Include(b => b.ProgramOrder).Where(b => b.ProgramOrder!.CompanyId == companyId).ToListAsync();
        public async Task<MainLabelBooking> CreateMainLabelBookingAsync(MainLabelBooking booking) { _context.MainLabelBookings.Add(booking); await _context.SaveChangesAsync(); return booking; }

        public async Task<IEnumerable<CareLabelBooking>> GetAllCareLabelBookingsAsync(int companyId) => await _context.CareLabelBookings.Include(b => b.ProgramOrder).Where(b => b.ProgramOrder!.CompanyId == companyId).ToListAsync();
        public async Task<CareLabelBooking> CreateCareLabelBookingAsync(CareLabelBooking booking) { _context.CareLabelBookings.Add(booking); await _context.SaveChangesAsync(); return booking; }

        public async Task<IEnumerable<PolyBooking>> GetAllPolyBookingsAsync(int companyId) => await _context.PolyBookings.Include(b => b.ProgramOrder).Where(b => b.ProgramOrder!.CompanyId == companyId).ToListAsync();
        public async Task<PolyBooking> CreatePolyBookingAsync(PolyBooking booking) { _context.PolyBookings.Add(booking); await _context.SaveChangesAsync(); return booking; }

        public async Task<IEnumerable<ThreadBooking>> GetAllThreadBookingsAsync(int companyId) => await _context.ThreadBookings.Include(b => b.ProgramOrder).Where(b => b.ProgramOrder!.CompanyId == companyId).ToListAsync();
        public async Task<ThreadBooking> CreateThreadBookingAsync(ThreadBooking booking) { _context.ThreadBookings.Add(booking); await _context.SaveChangesAsync(); return booking; }
    }
}
