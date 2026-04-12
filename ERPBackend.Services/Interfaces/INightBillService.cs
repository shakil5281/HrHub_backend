using ERPBackend.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERPBackend.Services.Interfaces
{
    public interface INightBillService
    {
        Task<NightBillResponseDto> GetNightBillsAsync(DateTime? fromDate, DateTime? toDate, int? employeeId, int? departmentId, string? status, string? searchTerm);
        Task<int> ProcessNightBillsAsync(BillProcessRequestDto request, string userName);
        Task<bool> DeleteNightBillAsync(int id);
        Task<int> DeleteMultipleAsync(List<int> ids);
    }
}
