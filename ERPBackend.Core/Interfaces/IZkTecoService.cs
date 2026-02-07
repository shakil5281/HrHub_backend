using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERPBackend.Core.Interfaces
{
    public interface IZkTecoService
    {
        Task<int> SyncDataFromDeviceAsync(string dbPath, DateTime? startDate = null, DateTime? endDate = null);
        Task ProcessDailyAttendanceAsync(DateTime date);
        Task ProcessBatchAttendanceAsync(DateTime? startDate, DateTime? endDate);
    }
}
