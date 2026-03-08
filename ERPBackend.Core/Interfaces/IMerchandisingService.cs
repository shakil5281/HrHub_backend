using System.Collections.Generic;
using System.Threading.Tasks;
using ERPBackend.Core.Models;
using ERPBackend.Core.Enums;

namespace ERPBackend.Core.Interfaces
{
    public interface IMerchandisingService
    {
        // Buyer Management
        Task<IEnumerable<Buyer>> GetAllBuyersAsync(int companyId);
        Task<Buyer?> GetBuyerByIdAsync(int id);
        Task<Buyer> CreateBuyerAsync(Buyer buyer);
        Task UpdateBuyerAsync(Buyer buyer);
        Task DeleteBuyerAsync(int id);

        // Brand Management
        Task<IEnumerable<Brand>> GetBrandsByBuyerAsync(int buyerId);
        Task<Brand?> GetBrandByIdAsync(int id);
        Task<Brand> CreateBrandAsync(Brand brand);
        Task UpdateBrandAsync(Brand brand);
        Task DeleteBrandAsync(int id);

        // Style Management
        Task<IEnumerable<Style>> GetStylesByBuyerAsync(int buyerId);
        Task<Style?> GetStyleByIdAsync(int id);
        Task<Style> CreateStyleAsync(Style style);
        Task UpdateStyleAsync(Style style);

        // Order Management
        Task<IEnumerable<StyleOrder>> GetOrdersByCompanyAsync(int companyId);
        Task<StyleOrder?> GetOrderByIdAsync(int id);
        Task<StyleOrder> CreateOrderAsync(StyleOrder order);
        Task UpdateOrderAsync(StyleOrder order);

        // BOM & Booking
        Task<BOM?> GetBOMByOrderIdAsync(int orderId);
        Task<BOM> CreateBOMAsync(BOM bom);
        Task<IEnumerable<FabricBooking>> GetFabricBookingsByOrderAsync(int orderId);
        Task<FabricBooking> CreateFabricBookingAsync(FabricBooking booking);

        // Production & Shipment
        Task<IEnumerable<MerchProductionPlan>> GetProductionPlansByOrderAsync(int orderId);
        Task<Shipment?> GetShipmentByOrderIdAsync(int orderId);
        Task<Shipment> CreateShipmentAsync(Shipment shipment);
    }
}
