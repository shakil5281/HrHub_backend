using ERPBackend.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPBackend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // Secure it, but for testing user can enable/disable
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var result = await _dashboardService.GetDashboardSummaryAsync();
            return Ok(result);
        }

        [HttpGet("attendance-stats")]
        public async Task<IActionResult> GetAttendanceStats()
        {
            var result = await _dashboardService.GetAttendanceStatsAsync();
            return Ok(result);
        }

        [HttpGet("department-stats")]
        public async Task<IActionResult> GetDepartmentStats()
        {
            var result = await _dashboardService.GetDepartmentStatsAsync();
            return Ok(result);
        }

        [HttpGet("recent-hires")]
        public async Task<IActionResult> GetRecentHires()
        {
            var result = await _dashboardService.GetRecentHiresAsync();
            return Ok(result);
        }

        [HttpGet("upcoming-events")]
        public async Task<IActionResult> GetUpcomingEvents()
        {
            var result = await _dashboardService.GetUpcomingEventsAsync();
            return Ok(result);
        }
    }
}
