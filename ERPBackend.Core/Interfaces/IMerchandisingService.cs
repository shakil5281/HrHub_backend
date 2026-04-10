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
        Task<IEnumerable<Brand>> GetBrandsByCompanyAsync(int companyId);
        Task<Brand?> GetBrandByIdAsync(int id);
        Task<Brand> CreateBrandAsync(Brand brand);
        Task UpdateBrandAsync(Brand brand);
        Task DeleteBrandAsync(int id);

        // Style Management
        Task<IEnumerable<Style>> GetStylesByBuyerAsync(int buyerId);
        Task<Style?> GetStyleByIdAsync(int id);
        Task<Style> CreateStyleAsync(Style style);
        Task UpdateStyleAsync(Style style);
        Task DeleteStyleAsync(int id);

        // Order Management
        Task<IEnumerable<StyleOrder>> GetOrdersByCompanyAsync(int companyId);
        Task<StyleOrder?> GetOrderByIdAsync(int id);
        Task<StyleOrder> CreateOrderAsync(StyleOrder order);
        Task UpdateOrderAsync(StyleOrder order);

        // BOM & Booking
        Task<BOM?> GetBOMByOrderIdAsync(int orderId);
        Task<BOM> CreateBOMAsync(BOM bom);
        Task<IEnumerable<FabricBooking>> GetFabricBookingsByOrderAsync(int orderId);
        Task<IEnumerable<FabricBooking>> GetAllFabricBookingsAsync(int companyId);
        Task<FabricBooking?> GetFabricBookingByIdAsync(int id);
        Task<FabricBooking> CreateFabricBookingAsync(FabricBooking booking);
        Task UpdateFabricBookingAsync(FabricBooking booking);
        Task DeleteFabricBookingAsync(int id);

        // Production & Shipment
        Task<IEnumerable<MerchProductionPlan>> GetProductionPlansByOrderAsync(int orderId);
        Task<Shipment?> GetShipmentByOrderIdAsync(int orderId);
        Task<Shipment> CreateShipmentAsync(Shipment shipment);

        // Accessories Booking
        Task<IEnumerable<AccessoriesBooking>> GetAccessoriesBookingsByOrderAsync(int orderId);
        Task<IEnumerable<AccessoriesBooking>> GetAllAccessoriesBookingsAsync(int companyId);
        Task<AccessoriesBooking?> GetAccessoriesBookingByIdAsync(int id);
        Task<AccessoriesBooking> CreateAccessoriesBookingAsync(AccessoriesBooking booking);
        Task UpdateAccessoriesBookingAsync(AccessoriesBooking booking);
        Task DeleteAccessoriesBookingAsync(int id);

        // Tech Pack Management
        Task<IEnumerable<TechPack>> GetAllTechPacksAsync(int companyId);
        Task<IEnumerable<TechPack>> GetTechPacksByStyleAsync(int styleId);
        Task<TechPack> CreateTechPackAsync(TechPack techPack);
        Task DeleteTechPackAsync(int id);
    }
}
