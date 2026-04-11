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

        // Button Booking
        Task<IEnumerable<ButtonBooking>> GetButtonBookingsByOrderAsync(int orderId);
        Task<IEnumerable<ButtonBooking>> GetAllButtonBookingsAsync(int companyId);
        Task<ButtonBooking?> GetButtonBookingByIdAsync(int id);
        Task<ButtonBooking> CreateButtonBookingAsync(ButtonBooking booking);
        Task UpdateButtonBookingAsync(ButtonBooking booking);
        Task DeleteButtonBookingAsync(int id);
        // Snap Button Bookings
        Task<IEnumerable<SnapButtonBooking>> GetAllSnapButtonBookingsAsync(int companyId);
        Task<SnapButtonBooking> GetSnapButtonBookingByIdAsync(int id);
        Task<SnapButtonBooking> CreateSnapButtonBookingAsync(SnapButtonBooking booking);
        Task UpdateSnapButtonBookingAsync(SnapButtonBooking booking);
        Task DeleteSnapButtonBookingAsync(int id);

        // Zipper Bookings
        Task<IEnumerable<ZipperBooking>> GetAllZipperBookingsAsync(int companyId);
        Task<ZipperBooking> GetZipperBookingByIdAsync(int id);
        Task<ZipperBooking> CreateZipperBookingAsync(ZipperBooking booking);
        Task UpdateZipperBookingAsync(ZipperBooking booking);
        Task DeleteZipperBookingAsync(int id);

        // Label Bookings
        Task<IEnumerable<LabelBooking>> GetAllLabelBookingsAsync(int companyId);
        Task<LabelBooking> GetLabelBookingByIdAsync(int id);
        Task<LabelBooking> CreateLabelBookingAsync(LabelBooking booking);
        Task UpdateLabelBookingAsync(LabelBooking booking);
        Task DeleteLabelBookingAsync(int id);

        // Trim Bookings
        Task<IEnumerable<TrimBooking>> GetAllTrimBookingsAsync(int companyId);
        Task<TrimBooking> GetTrimBookingByIdAsync(int id);
        Task<TrimBooking> CreateTrimBookingAsync(TrimBooking booking);
        Task UpdateTrimBookingAsync(TrimBooking booking);
        Task DeleteTrimBookingAsync(int id);

        // Thread Bookings
        Task<IEnumerable<ThreadBooking>> GetAllThreadBookingsAsync(int companyId);
        Task<ThreadBooking> GetThreadBookingByIdAsync(int id);
        Task<ThreadBooking> CreateThreadBookingAsync(ThreadBooking booking);
        Task UpdateThreadBookingAsync(ThreadBooking booking);
        Task DeleteThreadBookingAsync(int id);

        // Packing Bookings
        Task<IEnumerable<PackingBooking>> GetAllPackingBookingsAsync(int companyId);
        Task<PackingBooking> GetPackingBookingByIdAsync(int id);
        Task<PackingBooking> CreatePackingBookingAsync(PackingBooking booking);
        Task UpdatePackingBookingAsync(PackingBooking booking);
        Task DeletePackingBookingAsync(int id);
    }
}
