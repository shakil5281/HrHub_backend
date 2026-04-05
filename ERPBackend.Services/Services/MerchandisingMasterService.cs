using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using ERPBackend.Core.Interfaces;
using ERPBackend.Core.Models;
using ERPBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

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

        public async Task<IEnumerable<FabricColorPantone>> GetAllColorsAsync(int companyId)
            => await _context.FabricColorPantones.Where(x => x.CompanyId == companyId).ToListAsync();

        public async Task<FabricColorPantone> CreateColorAsync(FabricColorPantone color)
        {
            _context.FabricColorPantones.Add(color);
            await _context.SaveChangesAsync();
            return color;
        }

        public async Task<FabricColorPantone> UpdateColorAsync(FabricColorPantone color)
        {
            _context.FabricColorPantones.Update(color);
            await _context.SaveChangesAsync();
            return color;
        }

        public async Task<bool> DeleteColorAsync(int id)
        {
            var color = await _context.FabricColorPantones.FindAsync(id);
            if (color == null) return false;
            
            _context.FabricColorPantones.Remove(color);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> ImportColorsAsync(Stream fileStream, int companyId, int branchId)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets[0];
            int rowCount = worksheet.Dimension.Rows;
            int importCount = 0;

            for (int row = 2; row <= rowCount; row++)
            {
                var colorName = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                if (string.IsNullOrEmpty(colorName)) continue;

                var pantoneCode = worksheet.Cells[row, 2].Value?.ToString()?.Trim() ?? "";

                // Check if already exists to avoid duplicates
                var exists = await _context.FabricColorPantones
                    .AnyAsync(x => x.CompanyId == companyId && x.ColorName.ToLower() == colorName.ToLower());
                
                if (exists) continue;

                var color = new FabricColorPantone
                {
                    ColorName = colorName,
                    PantoneCode = pantoneCode,
                    CompanyId = companyId,
                    BranchId = branchId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.FabricColorPantones.Add(color);
                importCount++;
            }

            if (importCount > 0)
            {
                await _context.SaveChangesAsync();
            }

            return importCount;
        }
    }
}
