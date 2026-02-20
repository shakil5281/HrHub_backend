using ERPBackend.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Versioning;
using ERPBackend.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;

namespace ERPBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize] // Uncomment to enforce auth
    [SupportedOSPlatform("windows")]
    public class AttendanceSyncController : ControllerBase
    {
        private readonly IZkTecoService _zkTecoService;
        private readonly ILogger<AttendanceSyncController> _logger;

        public AttendanceSyncController(IZkTecoService zkTecoService, ILogger<AttendanceSyncController> logger)
        {
            _zkTecoService = zkTecoService;
            _logger = logger;
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

                int count = await _zkTecoService.SyncDataFromDeviceAsync(path, request.StartDate, request.EndDate,
                    request.CompanyId);
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
                var codes = request.EmployeeCodes ?? (string.IsNullOrEmpty(request.EmployeeCode)
                    ? null
                    : new List<string> { request.EmployeeCode });

                if (request.StartDate.HasValue || request.EndDate.HasValue)
                {
                    await _zkTecoService.ProcessBatchAttendanceAsync(
                        request.StartDate,
                        request.EndDate,
                        codes,
                        request.DepartmentId,
                        request.SectionId,
                        request.DesignationId,
                        request.LineId,
                        request.ShiftId,
                        request.GroupId,
                        request.CompanyId); // Added CompanyId

                    return Ok(new
                    {
                        message =
                            $"Processed attendance for {(codes?.Count > 0 ? $"{codes.Count} employees" : "targeted scope")} in range: {(request.StartDate?.ToString("yyyy-MM-dd") ?? "Start")} to {(request.EndDate?.ToString("yyyy-MM-dd") ?? "End")}"
                    });
                }

                // Fallback to single date or today if nothing provided
                DateTime targetDate = request.Date == default ? DateTime.Today : request.Date;
                await _zkTecoService.ProcessDailyAttendanceAsync(
                    targetDate,
                    codes,
                    request.DepartmentId,
                    request.SectionId,
                    request.DesignationId,
                    request.LineId,
                    request.ShiftId,
                    request.GroupId,
                    request.CompanyId); // Added CompanyId

                return Ok(new
                {
                    message =
                        $"Processed attendance for {(codes?.Count > 0 ? $"{codes.Count} employees" : "targeted scope")} on {targetDate:yyyy-MM-dd}"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("cleanup-data")]
        public async Task<IActionResult> CleanupData([FromBody] CleanupRequest request)
        {
            try
            {
                if (request.ConfirmationCode != "DELETE_ALL_ATTENDANCE_DATA")
                {
                    return BadRequest(new
                    {
                        message = "Invalid confirmation code. Please provide correct confirmation code to proceed."
                    });
                }

                var attendanceCount = await _zkTecoService.ClearAllAttendancesAsync();
                var logsCount = await _zkTecoService.ClearAllAttendanceLogsAsync();

                return Ok(new
                {
                    message = "All attendance data cleared successfully.",
                    attendancesDeleted = attendanceCount,
                    logsDeleted = logsCount
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("logs")]
        public async Task<IActionResult> GetLogs([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate,
            [FromQuery] string? searchTerm, [FromQuery] int? companyId)
        {
            try
            {
                var logs = await _zkTecoService.GetAttendanceLogsAsync(startDate, endDate, searchTerm, companyId);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetLogs: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message, details = ex.ToString() });
            }
        }

        [HttpDelete("logs")]
        public async Task<IActionResult> DeleteLogs([FromBody] DeleteLogsRequest request)
        {
            try
            {
                if (request.Ids == null || !request.Ids.Any())
                {
                    return BadRequest(new { message = "No IDs provided for deletion." });
                }

                int count = await _zkTecoService.DeleteAttendanceLogsAsync(request.Ids);
                return Ok(new { message = $"{count} logs deleted permanently.", count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class DeleteLogsRequest
    {
        public List<int>? Ids { get; set; }
    }

    public class SyncRequest
    {
        public string? DbPath { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? CompanyId { get; set; }
    }

    public class ProcessRequest
    {
        public DateTime Date { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? EmployeeCode { get; set; }
        public List<string>? EmployeeCodes { get; set; }
        public int? DepartmentId { get; set; }
        public int? SectionId { get; set; }
        public int? DesignationId { get; set; }
        public int? LineId { get; set; }
        public int? ShiftId { get; set; }
        public int? GroupId { get; set; }
        public int? CompanyId { get; set; }
    }

    public class CleanupRequest
    {
        public string ConfirmationCode { get; set; } = string.Empty;
    }
}
