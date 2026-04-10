using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using ERPBackend.Core.Models;
using ERPBackend.Core.DTOs;

namespace ERPBackend.Core.Interfaces
{
    public interface IOrderSheetService
    {
        Task<IEnumerable<OrderSheetDto>> GetAllAsync(int companyId);
        Task<OrderSheet?> GetByIdAsync(int id);
        Task<OrderSheetDto?> GetDtoByIdAsync(int id);
        Task<OrderSheet> CreateAsync(OrderSheet orderSheet);
        Task UpdateAsync(OrderSheet orderSheet);
        Task DeleteAsync(int id);
        
        // Pack Calculation
        int CalculatePackQuantity(int packA, int packB, bool isCombined);
        
        // Size Matrix / Summary logic can be internal or helper
        OrderSummaryDto GetOrderSummary(int orderSheetId);

        // Import Excel
        Task<MultiSheetOrderImportDto> ParseExcelAsync(Stream fileStream);
        Task<int> ImportOrderSheetsAsync(MultiSheetOrderImportDto importData, int companyId, int branchId);
        Task<byte[]> DownloadTemplateAsync();
        Task<byte[]> ExportOrderSheetAsync(int id);
        Task<GlobalOrderSummaryDto> GetGlobalSummaryAsync(int companyId);
    }
}
