using ERPBackend.Core.DTOs;

namespace ERPBackend.Core.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetDashboardSummaryAsync();
        Task<List<AttendanceStatDto>> GetAttendanceStatsAsync();
        Task<List<DepartmentStatDto>> GetDepartmentStatsAsync();
        Task<List<RecentHireDto>> GetRecentHiresAsync();
        Task<List<UpcomingEventDto>> GetUpcomingEventsAsync();
    }
}
