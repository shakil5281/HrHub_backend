using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ERPBackend.Core.Models;

namespace ERPBackend.Core.Interfaces
{
    public interface IMerchandisingMasterService
    {
        // Seasons
        Task<IEnumerable<Season>> GetAllSeasonsAsync(int companyId);
        Task<Season> CreateSeasonAsync(Season season);
        
        // Departments
        Task<IEnumerable<MerchandisingDepartment>> GetAllDepartmentsAsync(int companyId);
        Task<MerchandisingDepartment> CreateDepartmentAsync(MerchandisingDepartment dept);

        // Fabric & GSM
        Task<IEnumerable<FabricTypeGsm>> GetAllFabricGsmsAsync(int companyId);
        Task<FabricTypeGsm> CreateFabricGsmAsync(FabricTypeGsm model);

        // Suppliers
        Task<IEnumerable<SupplierInfo>> GetAllSuppliersAsync(int companyId);
        Task<SupplierInfo> CreateSupplierAsync(SupplierInfo supplier);

        // Machines
        Task<IEnumerable<KnitMachine>> GetAllKnitMachinesAsync(int companyId);
        Task<IEnumerable<DyeingMachine>> GetAllDyeingMachinesAsync(int companyId);

        // Logistics
        Task<IEnumerable<CourierInfo>> GetAllCouriersAsync(int companyId);
        Task<IEnumerable<ShipmentModeTerms>> GetAllShipmentModesAsync(int companyId);

        // Colors
        Task<IEnumerable<FabricColorPantone>> GetAllColorsAsync(int companyId);
        Task<FabricColorPantone> CreateColorAsync(FabricColorPantone color);
        Task<FabricColorPantone> UpdateColorAsync(FabricColorPantone color);
        Task<bool> DeleteColorAsync(int id);
        Task<int> ImportColorsAsync(Stream fileStream, int companyId, int branchId);
    }
}
