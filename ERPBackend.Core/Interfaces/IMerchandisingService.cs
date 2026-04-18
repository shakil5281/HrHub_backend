using System.Collections.Generic;
using System.Threading.Tasks;
using ERPBackend.Core.Models;
using ERPBackend.Core.Enums;

namespace ERPBackend.Core.Interfaces
{
    public interface IMerchandisingService
    {
        // Buyer & Style Management
        Task<IEnumerable<Buyer>> GetAllBuyersAsync(int companyId);
        Task<Buyer?> GetBuyerByIdAsync(int id);
        Task<Buyer> CreateBuyerAsync(Buyer buyer);
        Task UpdateBuyerAsync(Buyer buyer);
        Task DeleteBuyerAsync(int id);
        
        Task<IEnumerable<Style>> GetStylesByBuyerAsync(int buyerId);
        Task<Style?> GetStyleByIdAsync(int id);
        Task<Style> CreateStyleAsync(Style style);
        Task UpdateStyleAsync(Style style);
        Task DeleteStyleAsync(int id);

        // Brand Management
        Task<IEnumerable<Brand>> GetAllBrandsAsync(int companyId);
        Task<IEnumerable<Brand>> GetBrandsByBuyerAsync(int buyerId);
        Task<Brand> CreateBrandAsync(Brand brand);
        Task UpdateBrandAsync(Brand brand);
        Task DeleteBrandAsync(int id);

        // Tech Packs
        Task<IEnumerable<TechPack>> GetAllTechPacksAsync(int companyId);
        Task<TechPack> CreateTechPackAsync(TechPack techPack);

        // Fabric Booking
        Task<IEnumerable<FabricBooking>> GetFabricBookingsByProgramAsync(int programId);
        Task<IEnumerable<FabricBooking>> GetAllFabricBookingsAsync(int companyId);
        Task<FabricBooking> CreateFabricBookingAsync(FabricBooking booking);

        // Modular Accessory Bookings
        Task<IEnumerable<ButtonBooking>> GetButtonBookingsByProgramAsync(int programId);
        Task<IEnumerable<ButtonBooking>> GetAllButtonBookingsAsync(int companyId);
        Task<ButtonBooking> CreateButtonBookingAsync(ButtonBooking booking);
        Task UpdateButtonBookingAsync(ButtonBooking booking);
        Task DeleteButtonBookingAsync(int id);

        Task<IEnumerable<ZipperBooking>> GetAllZipperBookingsAsync(int companyId);
        Task<ZipperBooking> CreateZipperBookingAsync(ZipperBooking booking);

        Task<IEnumerable<SnapButtonBooking>> GetAllSnapButtonBookingsAsync(int companyId);
        Task<SnapButtonBooking> CreateSnapButtonBookingAsync(SnapButtonBooking booking);

        Task<IEnumerable<MainLabelBooking>> GetAllMainLabelBookingsAsync(int companyId);
        Task<MainLabelBooking> CreateMainLabelBookingAsync(MainLabelBooking booking);

        Task<IEnumerable<CareLabelBooking>> GetAllCareLabelBookingsAsync(int companyId);
        Task<CareLabelBooking> CreateCareLabelBookingAsync(CareLabelBooking booking);

        Task<IEnumerable<PolyBooking>> GetAllPolyBookingsAsync(int companyId);
        Task<PolyBooking> CreatePolyBookingAsync(PolyBooking booking);

        Task<IEnumerable<ThreadBooking>> GetAllThreadBookingsAsync(int companyId);
        Task<ThreadBooking> CreateThreadBookingAsync(ThreadBooking booking);
    }
}
