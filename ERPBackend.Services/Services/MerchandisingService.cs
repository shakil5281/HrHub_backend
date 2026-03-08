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
                .Where(b => b.Buyer != null && b.Buyer.CompanyId == companyId)
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

        public async Task<FabricBooking> CreateFabricBookingAsync(FabricBooking booking)
        {
            _context.FabricBookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
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
        #endregion
    }
}
