using System.Collections.Generic;
using ERPBackend.Core.DTOs;
using System.Threading.Tasks;

namespace ERPBackend.Core.Interfaces
{
    public interface IZkTecoService
    {
        Task<int> SyncDataFromDeviceAsync(string dbPath, DateTime? startDate = null, DateTime? endDate = null,
            int? companyId = null);

        Task ProcessDailyAttendanceAsync(DateTime date,
            List<string>? employeeCodes = null,
            int? departmentId = null,
            int? sectionId = null,
            int? designationId = null,
            int? lineId = null,
            int? shiftId = null,
            int? groupId = null,
            int? companyId = null);

        Task ProcessBatchAttendanceAsync(DateTime? startDate, DateTime? endDate,
            List<string>? employeeCodes = null,
            int? departmentId = null,
            int? sectionId = null,
            int? designationId = null,
            int? lineId = null,
            int? shiftId = null,
            int? groupId = null,
            int? companyId = null);

        Task<List<AttendanceLogDto>> GetAttendanceLogsAsync(DateTime? startDate, DateTime? endDate,
            string? searchTerm = null, int? companyId = null);
    }
}
