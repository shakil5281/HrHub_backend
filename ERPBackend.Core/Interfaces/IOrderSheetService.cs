using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using ERPBackend.Core.Models;
using ERPBackend.Core.DTOs;

namespace ERPBackend.Core.Interfaces
{
    public interface IOrderSheetService
    {
        Task<IEnumerable<ProgramOrderDto>> GetAllAsync(int companyId);
        Task<ProgramOrder?> GetByIdAsync(int id);
        Task<ProgramOrderDto?> GetDtoByIdAsync(int id);
        Task<ProgramOrder> CreateAsync(ProgramOrder programOrder);
        Task UpdateAsync(ProgramOrder programOrder);
        Task DeleteAsync(int id);
        
        ProgramSummaryDto GetOrderSummary(int programId);

        Task<MultiSheetOrderImportDto> ParseExcelAsync(Stream fileStream);
        Task<int> ImportOrderSheetsAsync(MultiSheetOrderImportDto importData, int companyId, int branchId);
        Task<byte[]> DownloadTemplateAsync();
        Task<byte[]> ExportOrderSheetAsync(int id);
        Task<GlobalProgramSummaryDto> GetGlobalSummaryAsync(int companyId);
    }
}
