using System;
using System.Threading.Tasks;
using ERPBackend.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Versioning;

namespace ERPBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize] // Uncomment to enforce auth
    [SupportedOSPlatform("windows")]
    public class AttendanceSyncController : ControllerBase
    {
        private readonly IZkTecoService _zkTecoService;

        public AttendanceSyncController(IZkTecoService zkTecoService)
        {
            _zkTecoService = zkTecoService;
        }

        [HttpPost("sync")]
        public async Task<IActionResult> SyncData([FromBody] SyncRequest request)
        {
            try
            {
                // Default path if not provided
                string path = string.IsNullOrWhiteSpace(request.DbPath) 
                    ? @"C:\Program Files (x86)\ZKTeco\att2000.mdb" 
                    : request.DbPath;

                int count = await _zkTecoService.SyncDataFromDeviceAsync(path, request.StartDate, request.EndDate);
                return Ok(new { message = $"Synced {count} new records.", count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, details = ex.ToString() });
            }
        }

        [HttpPost("process-daily")]
        public async Task<IActionResult> ProcessDaily([FromBody] ProcessRequest request)
        {
            try
            {
                if (request.StartDate.HasValue || request.EndDate.HasValue)
                {
                    await _zkTecoService.ProcessBatchAttendanceAsync(request.StartDate, request.EndDate);
                    return Ok(new { message = $"Processed attendance for range: {(request.StartDate?.ToString("yyyy-MM-dd") ?? "Start")} to {(request.EndDate?.ToString("yyyy-MM-dd") ?? "End")}" });
                }
                
                // Fallback to single date or today if nothing provided
                DateTime targetDate = request.Date == default ? DateTime.Today : request.Date;
                await _zkTecoService.ProcessDailyAttendanceAsync(targetDate);
                return Ok(new { message = $"Processed attendance for {targetDate:yyyy-MM-dd}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class SyncRequest
    {
        public string? DbPath { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class ProcessRequest
    {
        public DateTime Date { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
