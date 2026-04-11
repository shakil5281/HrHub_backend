using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPBackend.Core.Interfaces;
using ERPBackend.Core.Models;
using ERPBackend.Core.Enums;
using ERPBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPBackend.Services.Services
{
    public class MerchandisingService : IMerchandisingService
    {
        private readonly MerchandisingDbContext _context;

        public MerchandisingService(MerchandisingDbContext context)
        {
            _context = context;
        }

        #region Buyer
        public async Task<IEnumerable<Buyer>> GetAllBuyersAsync(int companyId)
        {
            return await _context.Buyers
                .Where(b => b.CompanyId == companyId)
                .Include(b => b.Brands)
                .ToListAsync();
        }

        public async Task<Buyer?> GetBuyerByIdAsync(int id)
        {
            return await _context.Buyers
                .Include(b => b.Brands)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Buyer> CreateBuyerAsync(Buyer buyer)
        {
            _context.Buyers.Add(buyer);
            await _context.SaveChangesAsync();
            return buyer;
        }

        public async Task UpdateBuyerAsync(Buyer buyer)
        {
            _context.Entry(buyer).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteBuyerAsync(int id)
        {
            var buyer = await _context.Buyers.FindAsync(id);
            if (buyer != null)
            {
                _context.Buyers.Remove(buyer);
                await _context.SaveChangesAsync();
            }
        }
        #endregion

        #region Brand
        public async Task<IEnumerable<Brand>> GetBrandsByBuyerAsync(int buyerId)
        {
            return await _context.Brands
                .Where(b => b.BuyerId == buyerId)
                .Include(b => b.Buyer)
                .ToListAsync();
        }

        public async Task<IEnumerable<Brand>> GetBrandsByCompanyAsync(int companyId)
        {
            return await _context.Brands
                .Include(b => b.Buyer)
                .Where(b => b.Buyer != null && b.Buyer!.CompanyId == companyId)
                .ToListAsync();
        }

        public async Task<Brand?> GetBrandByIdAsync(int id)
        {
            return await _context.Brands.FindAsync(id);
        }

        public async Task<Brand> CreateBrandAsync(Brand brand)
        {
            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();
            return brand;
        }

        public async Task UpdateBrandAsync(Brand brand)
        {
            _context.Entry(brand).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteBrandAsync(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand != null)
            {
                _context.Brands.Remove(brand);
                await _context.SaveChangesAsync();
            }
        }
        #endregion

        #region Style
        public async Task<IEnumerable<Style>> GetStylesByBuyerAsync(int buyerId)
        {
            return await _context.Styles
                .Where(s => s.BuyerId == buyerId)
                .Include(s => s.Brand)
                .ToListAsync();
        }

        public async Task<Style?> GetStyleByIdAsync(int id)
        {
            return await _context.Styles
                .Include(s => s.Buyer)
                .Include(s => s.Brand)
                .Include(s => s.TechPacks)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Style> CreateStyleAsync(Style style)
        {
            _context.Styles.Add(style);
            await _context.SaveChangesAsync();
            return style;
        }

        public async Task UpdateStyleAsync(Style style)
        {
            _context.Entry(style).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteStyleAsync(int id)
        {
            var style = await _context.Styles.FindAsync(id);
            if (style != null)
            {
                _context.Styles.Remove(style);
                await _context.SaveChangesAsync();
            }
        }
        #endregion

        #region Order
        public async Task<IEnumerable<StyleOrder>> GetOrdersByCompanyAsync(int companyId)
        {
            return await _context.StyleOrders
                .Where(o => o.CompanyId == companyId)
                .Include(o => o.Buyer)
                .Include(o => o.Style)
                .ToListAsync();
        }

        public async Task<StyleOrder?> GetOrderByIdAsync(int id)
        {
            return await _context.StyleOrders
                .Include(o => o.Buyer)
                .Include(o => o.Style)
                .Include(o => o.OrderColors)
                    .ThenInclude(oc => oc.SizeBreakdowns)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<StyleOrder> CreateOrderAsync(StyleOrder order)
        {
            _context.StyleOrders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task UpdateOrderAsync(StyleOrder order)
        {
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        #endregion

        #region BOM & Booking
        public async Task<BOM?> GetBOMByOrderIdAsync(int orderId)
        {
            return await _context.BOMs
                .Include(b => b.BOMItems)
                .FirstOrDefaultAsync(b => b.OrderId == orderId);
        }

        public async Task<BOM> CreateBOMAsync(BOM bom)
        {
            _context.BOMs.Add(bom);
            await _context.SaveChangesAsync();
            return bom;
        }

        public async Task<IEnumerable<FabricBooking>> GetFabricBookingsByOrderAsync(int orderId)
        {
            return await _context.FabricBookings
                .Where(fb => fb.OrderId == orderId)
                .ToListAsync();
        }

        public async Task<IEnumerable<FabricBooking>> GetAllFabricBookingsAsync(int companyId)
        {
            return await _context.FabricBookings
                .Include(fb => fb.StyleOrder!)
                    .ThenInclude(o => o.Style)
                .Where(fb => fb.StyleOrder != null && fb.StyleOrder.CompanyId == companyId)
                .ToListAsync();
        }

        public async Task<FabricBooking?> GetFabricBookingByIdAsync(int id)
        {
            return await _context.FabricBookings
                .Include(fb => fb.StyleOrder)
                .FirstOrDefaultAsync(fb => fb.Id == id);
        }

        public async Task<FabricBooking> CreateFabricBookingAsync(FabricBooking booking)
        {
            _context.FabricBookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task UpdateFabricBookingAsync(FabricBooking booking)
        {
            _context.Entry(booking).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteFabricBookingAsync(int id)
        {
            var booking = await _context.FabricBookings.FindAsync(id);
            if (booking != null)
            {
                _context.FabricBookings.Remove(booking);
                await _context.SaveChangesAsync();
            }
        }
        #endregion

        #region Production & Shipment
        public async Task<IEnumerable<MerchProductionPlan>> GetProductionPlansByOrderAsync(int orderId)
        {
            return await _context.MerchProductionPlans
                .Where(pp => pp.OrderId == orderId)
                .ToListAsync();
        }

        public async Task<Shipment?> GetShipmentByOrderIdAsync(int orderId)
        {
            return await _context.Shipments
                .FirstOrDefaultAsync(s => s.OrderId == orderId);
        }

        public async Task<Shipment> CreateShipmentAsync(Shipment shipment)
        {
            _context.Shipments.Add(shipment);
            await _context.SaveChangesAsync();
            return shipment;
        }

        public async Task<IEnumerable<AccessoriesBooking>> GetAccessoriesBookingsByOrderAsync(int orderId)
        {
            return await _context.AccessoriesBookings
                .Where(ab => ab.OrderId == orderId)
                .ToListAsync();
        }

        public async Task<IEnumerable<AccessoriesBooking>> GetAllAccessoriesBookingsAsync(int companyId)
        {
            return await _context.AccessoriesBookings
                .Include(ab => ab.StyleOrder!)
                    .ThenInclude(o => o.Style)
                .Where(ab => ab.StyleOrder != null && ab.StyleOrder.CompanyId == companyId)
                .ToListAsync();
        }

        public async Task<AccessoriesBooking?> GetAccessoriesBookingByIdAsync(int id)
        {
            return await _context.AccessoriesBookings
                .Include(ab => ab.StyleOrder)
                .FirstOrDefaultAsync(ab => ab.Id == id);
        }

        public async Task<AccessoriesBooking> CreateAccessoriesBookingAsync(AccessoriesBooking booking)
        {
            _context.AccessoriesBookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task UpdateAccessoriesBookingAsync(AccessoriesBooking booking)
        {
            _context.Entry(booking).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAccessoriesBookingAsync(int id)
        {
            var booking = await _context.AccessoriesBookings.FindAsync(id);
            if (booking != null)
            {
                _context.AccessoriesBookings.Remove(booking);
                await _context.SaveChangesAsync();
            }
        }
        #endregion
        #region Tech Pack
        public async Task<IEnumerable<TechPack>> GetAllTechPacksAsync(int companyId)
        {
            return await _context.TechPacks
                .Include(t => t.Style)
                .Where(t => t.Style != null && t.Style.CompanyId == companyId)
                .ToListAsync();
        }

        public async Task<IEnumerable<TechPack>> GetTechPacksByStyleAsync(int styleId)
        {
            return await _context.TechPacks
                .Where(t => t.StyleId == styleId)
                .ToListAsync();
        }

        public async Task<TechPack> CreateTechPackAsync(TechPack techPack)
        {
            _context.TechPacks.Add(techPack);
            await _context.SaveChangesAsync();
            return techPack;
        }

        public async Task DeleteTechPackAsync(int id)
        {
            var techPack = await _context.TechPacks.FindAsync(id);
            if (techPack != null)
            {
                _context.TechPacks.Remove(techPack);
                await _context.SaveChangesAsync();
            }
        }
        #endregion

        #region Button Booking
        public async Task<IEnumerable<ButtonBooking>> GetButtonBookingsByOrderAsync(int orderId)
        {
            return await _context.ButtonBookings
                .Where(bb => bb.OrderId == orderId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ButtonBooking>> GetAllButtonBookingsAsync(int companyId)
        {
            return await _context.ButtonBookings
                .Include(bb => bb.StyleOrder!)
                    .ThenInclude(o => o.Style)
                .Where(bb => bb.StyleOrder != null && bb.StyleOrder.CompanyId == companyId)
                .ToListAsync();
        }

        public async Task<ButtonBooking?> GetButtonBookingByIdAsync(int id)
        {
            return await _context.ButtonBookings
                .Include(bb => bb.StyleOrder)
                .FirstOrDefaultAsync(bb => bb.Id == id);
        }

        public async Task<ButtonBooking> CreateButtonBookingAsync(ButtonBooking booking)
        {
            _context.ButtonBookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task UpdateButtonBookingAsync(ButtonBooking booking)
        {
            _context.Entry(booking).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteButtonBookingAsync(int id)
        {
            var booking = await _context.ButtonBookings.FindAsync(id);
            if (booking != null)
            {
                _context.ButtonBookings.Remove(booking);
                await _context.SaveChangesAsync();
            }
        }

        // Snap Button Bookings
        public async Task<IEnumerable<SnapButtonBooking>> GetAllSnapButtonBookingsAsync(int companyId)
        {
            return await _context.SnapButtonBookings
                .Include(b => b.StyleOrder!)
                    .ThenInclude(o => o.Style)
                .Where(b => b.StyleOrder != null && b.StyleOrder.CompanyId == companyId)
                .ToListAsync();
        }

        public async Task<SnapButtonBooking?> GetSnapButtonBookingByIdAsync(int id) => await _context.SnapButtonBookings.Include(b => b.StyleOrder!).FirstOrDefaultAsync(b => b.Id == id);
        public async Task<SnapButtonBooking> CreateSnapButtonBookingAsync(SnapButtonBooking booking)
        {
            _context.SnapButtonBookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }
        public async Task UpdateSnapButtonBookingAsync(SnapButtonBooking booking)
        {
            _context.SnapButtonBookings.Update(booking);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteSnapButtonBookingAsync(int id)
        {
            var booking = await _context.SnapButtonBookings.FindAsync(id);
            if (booking != null) { _context.SnapButtonBookings.Remove(booking); await _context.SaveChangesAsync(); }
        }

        // Zipper Bookings
        public async Task<IEnumerable<ZipperBooking>> GetAllZipperBookingsAsync(int companyId)
        {
            return await _context.ZipperBookings
                .Include(b => b.StyleOrder!)
                    .ThenInclude(o => o.Style)
                .Where(b => b.StyleOrder != null && b.StyleOrder.CompanyId == companyId)
                .ToListAsync();
        }
        public async Task<ZipperBooking?> GetZipperBookingByIdAsync(int id) => await _context.ZipperBookings.Include(b => b.StyleOrder!).FirstOrDefaultAsync(b => b.Id == id);
        public async Task<ZipperBooking> CreateZipperBookingAsync(ZipperBooking booking)
        {
            _context.ZipperBookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }
        public async Task UpdateZipperBookingAsync(ZipperBooking booking)
        {
            _context.ZipperBookings.Update(booking);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteZipperBookingAsync(int id)
        {
            var booking = await _context.ZipperBookings.FindAsync(id);
            if (booking != null) { _context.ZipperBookings.Remove(booking); await _context.SaveChangesAsync(); }
        }

        // Label Bookings
        public async Task<IEnumerable<LabelBooking>> GetAllLabelBookingsAsync(int companyId)
        {
            return await _context.LabelBookings
                .Include(b => b.StyleOrder!)
                    .ThenInclude(o => o.Style)
                .Where(b => b.StyleOrder != null && b.StyleOrder.CompanyId == companyId)
                .ToListAsync();
        }
        public async Task<LabelBooking?> GetLabelBookingByIdAsync(int id) => await _context.LabelBookings.Include(b => b.StyleOrder!).FirstOrDefaultAsync(b => b.Id == id);
        public async Task<LabelBooking> CreateLabelBookingAsync(LabelBooking booking)
        {
            _context.LabelBookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }
        public async Task UpdateLabelBookingAsync(LabelBooking booking)
        {
            _context.LabelBookings.Update(booking);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteLabelBookingAsync(int id)
        {
            var booking = await _context.LabelBookings.FindAsync(id);
            if (booking != null) { _context.LabelBookings.Remove(booking); await _context.SaveChangesAsync(); }
        }

        // Trim Bookings
        public async Task<IEnumerable<TrimBooking>> GetAllTrimBookingsAsync(int companyId)
        {
            return await _context.TrimBookings
                .Include(b => b.StyleOrder!)
                    .ThenInclude(o => o.Style)
                .Where(b => b.StyleOrder != null && b.StyleOrder.CompanyId == companyId)
                .ToListAsync();
        }
        public async Task<TrimBooking?> GetTrimBookingByIdAsync(int id) => await _context.TrimBookings.Include(b => b.StyleOrder!).FirstOrDefaultAsync(b => b.Id == id);
        public async Task<TrimBooking> CreateTrimBookingAsync(TrimBooking booking)
        {
            _context.TrimBookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }
        public async Task UpdateTrimBookingAsync(TrimBooking booking)
        {
            _context.TrimBookings.Update(booking);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteTrimBookingAsync(int id)
        {
            var booking = await _context.TrimBookings.FindAsync(id);
            if (booking != null) { _context.TrimBookings.Remove(booking); await _context.SaveChangesAsync(); }
        }

        // Thread Bookings
        public async Task<IEnumerable<ThreadBooking>> GetAllThreadBookingsAsync(int companyId)
        {
            return await _context.ThreadBookings
                .Include(b => b.StyleOrder!)
                    .ThenInclude(o => o.Style)
                .Where(b => b.StyleOrder != null && b.StyleOrder.CompanyId == companyId)
                .ToListAsync();
        }
        public async Task<ThreadBooking?> GetThreadBookingByIdAsync(int id) => await _context.ThreadBookings.Include(b => b.StyleOrder!).FirstOrDefaultAsync(b => b.Id == id);
        public async Task<ThreadBooking> CreateThreadBookingAsync(ThreadBooking booking)
        {
            _context.ThreadBookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }
        public async Task UpdateThreadBookingAsync(ThreadBooking booking)
        {
            _context.ThreadBookings.Update(booking);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteThreadBookingAsync(int id)
        {
            var booking = await _context.ThreadBookings.FindAsync(id);
            if (booking != null) { _context.ThreadBookings.Remove(booking); await _context.SaveChangesAsync(); }
        }

        // Packing Bookings
        public async Task<IEnumerable<PackingBooking>> GetAllPackingBookingsAsync(int companyId)
        {
            return await _context.PackingBookings
                .Include(b => b.StyleOrder!)
                    .ThenInclude(o => o.Style)
                .Where(b => b.StyleOrder != null && b.StyleOrder.CompanyId == companyId)
                .ToListAsync();
        }
        public async Task<PackingBooking?> GetPackingBookingByIdAsync(int id) => await _context.PackingBookings.Include(b => b.StyleOrder!).FirstOrDefaultAsync(b => b.Id == id);
        public async Task<PackingBooking> CreatePackingBookingAsync(PackingBooking booking)
        {
            _context.PackingBookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }
        public async Task UpdatePackingBookingAsync(PackingBooking booking)
        {
            _context.PackingBookings.Update(booking);
            await _context.SaveChangesAsync();
        }
        public async Task DeletePackingBookingAsync(int id)
        {
            var booking = await _context.PackingBookings.FindAsync(id);
            if (booking != null) { _context.PackingBookings.Remove(booking); await _context.SaveChangesAsync(); }
        }
        #endregion
    }
}
