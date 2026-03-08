using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPBackend.Core.Interfaces;
using ERPBackend.Core.Models;
using ERPBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPBackend.Services.Services
{
    public class MerchandisingMasterService : IMerchandisingMasterService
    {
        private readonly MerchandisingDbContext _context;

        public MerchandisingMasterService(MerchandisingDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Season>> GetAllSeasonsAsync(int companyId) 
            => await _context.Seasons.Where(x => x.CompanyId == companyId).ToListAsync();

        public async Task<Season> CreateSeasonAsync(Season season)
        {
            _context.Seasons.Add(season);
            await _context.SaveChangesAsync();
            return season;
        }

        public async Task<IEnumerable<MerchandisingDepartment>> GetAllDepartmentsAsync(int companyId)
            => await _context.MerchandisingDepartments.Where(x => x.CompanyId == companyId).ToListAsync();

        public async Task<MerchandisingDepartment> CreateDepartmentAsync(MerchandisingDepartment dept)
        {
            _context.MerchandisingDepartments.Add(dept);
            await _context.SaveChangesAsync();
            return dept;
        }

        public async Task<IEnumerable<FabricTypeGsm>> GetAllFabricGsmsAsync(int companyId)
            => await _context.FabricTypeGsms.Where(x => x.CompanyId == companyId).ToListAsync();

        public async Task<FabricTypeGsm> CreateFabricGsmAsync(FabricTypeGsm model)
        {
            _context.FabricTypeGsms.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<IEnumerable<SupplierInfo>> GetAllSuppliersAsync(int companyId)
            => await _context.SupplierInfos.Where(x => x.CompanyId == companyId).ToListAsync();

        public async Task<SupplierInfo> CreateSupplierAsync(SupplierInfo supplier)
        {
            _context.SupplierInfos.Add(supplier);
            await _context.SaveChangesAsync();
            return supplier;
        }

        public async Task<IEnumerable<KnitMachine>> GetAllKnitMachinesAsync(int companyId)
            => await _context.KnitMachines.Where(x => x.CompanyId == companyId).ToListAsync();

        public async Task<IEnumerable<DyeingMachine>> GetAllDyeingMachinesAsync(int companyId)
            => await _context.DyeingMachines.Where(x => x.CompanyId == companyId).ToListAsync();

        public async Task<IEnumerable<CourierInfo>> GetAllCouriersAsync(int companyId)
            => await _context.CourierInfos.Where(x => x.CompanyId == companyId).ToListAsync();

        public async Task<IEnumerable<ShipmentModeTerms>> GetAllShipmentModesAsync(int companyId)
            => await _context.ShipmentModeTerms.Where(x => x.CompanyId == companyId).ToListAsync();
    }
}
