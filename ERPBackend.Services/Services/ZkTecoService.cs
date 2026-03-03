using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ERPBackend.Core.Interfaces;
using ERPBackend.Core.Models;
using ERPBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using ERPBackend.Core.DTOs;

namespace ERPBackend.Services.Services
{
    [SupportedOSPlatform("windows")]
    public class ZkTecoService : IZkTecoService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ZkTecoService> _logger;

        public ZkTecoService(ApplicationDbContext context, ILogger<ZkTecoService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> SyncDataFromDeviceAsync(string dbPath, DateTime? startDate = null,
            DateTime? endDate = null, int? companyId = null)
        {
            if (!File.Exists(dbPath))
            {
                throw new FileNotFoundException("ZKTeco database file not found.", dbPath);
            }

            string tempDbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + Path.GetExtension(dbPath));

            var logsToInsert = new List<AttendanceLog>();
            int newRecordsCount = 0;

            try
            {
                // Copy to temp to avoid "file in use" locks
                File.Copy(dbPath, tempDbPath, true);

                // Connection string for Microsoft Access
                // Quoting Data Source and removing invalid standalone 'Share Deny None'
                string connectionString =
                    $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\"{tempDbPath}\";Persist Security Info=False;";

                using (var connection = new OleDbConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // 1. Get User Mapping (UserId -> BadgeNumber)
                    var userMap = new Dictionary<int, string>();
                    using (var command = new OleDbCommand("SELECT USERID, Badgenumber FROM USERINFO", connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int userId = Convert.ToInt32(reader["USERID"]);
                                string badgeNumber = reader["Badgenumber"].ToString() ?? string.Empty;
                                if (!userMap.ContainsKey(userId))
                                {
                                    userMap.Add(userId, badgeNumber);
                                }
                            }
                        }
                    }

                    // 2. Get existing employees to validate map
                    var employeeQuery = _context.Employees.Include(e => e.Department).AsQueryable();
                    if (companyId.HasValue)
                    {
                        employeeQuery = employeeQuery.Where(e => e.Department!.CompanyId == companyId.Value);
                    }

                    var validEmployees = await employeeQuery
                        .Select(e => new
                        {
                            e.Id, e.EmployeeId, CompanyId = e.Department != null ? (int?)e.Department.CompanyId : null
                        })
                        .ToDictionaryAsync(e => e.EmployeeId, e => new { e.Id, e.CompanyId });

                    // 3. Get Logs
                    string sql = "SELECT USERID, CHECKTIME, SENSORID, CHECKTYPE FROM CHECKINOUT";
                    var parameters = new List<OleDbParameter>();

                    if (startDate.HasValue || endDate.HasValue)
                    {
                        sql += " WHERE";
                        if (startDate.HasValue)
                        {
                            sql += " CHECKTIME >= ?";
                            parameters.Add(new OleDbParameter("@start", startDate.Value.Date));
                        }

                        if (endDate.HasValue)
                        {
                            if (startDate.HasValue) sql += " AND";
                            sql += " CHECKTIME < ?";
                            parameters.Add(new OleDbParameter("@end", endDate.Value.Date.AddDays(1)));
                        }
                    }

                    var tempLogs = new List<(int UserId, DateTime CheckTime, string DeviceId)>();
                    using (var command = new OleDbCommand(sql, connection))
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.Add(param);
                        }

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int userId = Convert.ToInt32(reader["USERID"]);
                                DateTime checkTime = Convert.ToDateTime(reader["CHECKTIME"]);
                                string deviceId = reader["SENSORID"].ToString() ?? string.Empty;
                                tempLogs.Add((userId, checkTime, deviceId));
                            }
                        }
                    }

                    // 4. Batch check for existing logs to avoid N+1 queries
                    if (tempLogs.Any())
                    {
                        // Get date range to avoid CTE issues with SQL Server 2008
                        var minDate = tempLogs.Min(l => l.CheckTime);
                        var maxDate = tempLogs.Max(l => l.CheckTime);

                        var existingLogsLookup = await _context.AttendanceLogs
                            .Where(l => l.LogTime >= minDate && l.LogTime <= maxDate)
                            .Select(l => new { l.EmployeeCard, l.LogTime })
                            .ToListAsync();

                        var existingLogsSet = new HashSet<string>(
                            existingLogsLookup.Select(l => $"{l.EmployeeCard}|{l.LogTime:O}")
                        );

                        foreach (var log in tempLogs)
                        {
                            if (userMap.TryGetValue(log.UserId, out string? badgeNumber))
                            {
                                if (validEmployees.TryGetValue(badgeNumber, out var empInfo))
                                {
                                    string key = $"{empInfo.Id}|{log.CheckTime:O}";
                                    if (!existingLogsSet.Contains(key))
                                    {
                                        logsToInsert.Add(new AttendanceLog
                                        {
                                            EmployeeCard = empInfo.Id,
                                            EmployeeId = badgeNumber,
                                            CompanyId = empInfo.CompanyId,
                                            LogTime = log.CheckTime,
                                            DeviceId = log.DeviceId,
                                            VerificationMode = "Device"
                                        });
                                        newRecordsCount++;
                                        // Add to set to prevent duplicates in same batch
                                        existingLogsSet.Add(key);
                                    }
                                }
                            }
                        }
                    }
                }

                if (logsToInsert.Any())
                {
                    // Batch insert could be better, but EF Core handles AddRange well
                    await _context.AttendanceLogs.AddRangeAsync(logsToInsert);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing ZKTeco data");
                throw;
            }
            finally
            {
                // Clean up temp file
                if (File.Exists(tempDbPath))
                {
                    try
                    {
                        File.Delete(tempDbPath);
                    }
                    catch
                    {
                        /* Ignore delete errors */
                    }
                }
            }

            return newRecordsCount;
        }

        public async Task ProcessDailyAttendanceAsync(DateTime date,
            List<string>? employeeCodes = null,
            int? departmentId = null,
            int? sectionId = null,
            int? designationId = null,
            int? lineId = null,
            int? shiftId = null,
            int? groupId = null,
            int? companyId = null)
        {
            // 1. Fetch relevant logs extending into next day to cover overnight shifts
            var endTime = date.Date.AddDays(2);

            var allLogs = await _context.AttendanceLogs
                .Where(l => l.LogTime >= date.Date && l.LogTime < endTime)
                .ToListAsync();

            // 2. Fetch all active employees (include Group for Worker OT check)
            var queryEmployees = _context.Employees
                .Where(e => e.IsActive)
                .Include(e => e.Shift)
                .Include(e => e.Department)
                .Include(e => e.Group)
                .AsQueryable();

            if (employeeCodes != null && employeeCodes.Any())
                queryEmployees = queryEmployees.Where(e => employeeCodes.Contains(e.EmployeeId));

            if (departmentId.HasValue)
                queryEmployees = queryEmployees.Where(e => e.DepartmentId == departmentId.Value);

            if (sectionId.HasValue) queryEmployees = queryEmployees.Where(e => e.SectionId == sectionId.Value);
            if (designationId.HasValue)
                queryEmployees = queryEmployees.Where(e => e.DesignationId == designationId.Value);
            if (lineId.HasValue) queryEmployees = queryEmployees.Where(e => e.LineId == lineId.Value);
            if (shiftId.HasValue) queryEmployees = queryEmployees.Where(e => e.ShiftId == shiftId.Value);
            if (groupId.HasValue) queryEmployees = queryEmployees.Where(e => e.GroupId == groupId.Value);
            if (companyId.HasValue)
                queryEmployees = queryEmployees.Where(e => e.Department!.CompanyId == companyId.Value);

            var activeEmployees = await queryEmployees.ToListAsync();

            // 3. Fetch approved leaves for this date
            var leaves = await _context.LeaveApplications
                .Where(l => l.Status == "Approved" && l.StartDate.Date <= date.Date && l.EndDate.Date >= date.Date)
                .ToListAsync();

            // 4. Fetch roster/off-days
            var empSecondaryIds = activeEmployees.Select(e => e.Id).ToList();
            var allRosters = await _context.EmployeeShiftRosters
                .Include(r => r.Shift)
                .Where(r => empSecondaryIds.Contains(r.EmployeeId) && r.Date.Date <= date.Date)
                .OrderByDescending(r => r.Date)
                .ToListAsync();

            // 5. Existing attendance records for this date
            var existingAttendances = await _context.Attendances
                .Where(a => a.Date == date.Date)
                .ToDictionaryAsync(a => a.EmployeeCard);

            foreach (var emp in activeEmployees)
            {
                // 1. Determine the correct shift (Roster Priority > Default Shift)
                var employeeRosters = allRosters.Where(r => r.EmployeeId == emp.Id).ToList();
                var roster = employeeRosters.FirstOrDefault(r => r.Date.Date == date.Date);
                if (roster == null) roster = employeeRosters.FirstOrDefault();

                var shift = (roster != null && roster.Shift != null) ? roster.Shift : emp.Shift;
                if (shift == null) continue;

                // 2. Parse shift configurations
                TimeSpan actualInTs = TimeSpan.TryParse(shift.ActualInTime, out var sAi) ? sAi : TimeSpan.FromHours(5);
                TimeSpan inTs       = TimeSpan.TryParse(shift.InTime,       out var sIn) ? sIn : sAi;
                TimeSpan outTs      = TimeSpan.TryParse(shift.OutTime,      out var sOut) ? sOut : TimeSpan.FromHours(17);
                TimeSpan actualOutTs = TimeSpan.TryParse(shift.ActualOutTime, out var sAo) ? sAo : TimeSpan.FromHours(4);

                DateTime actualInStart = date.Date.Add(actualInTs);
                DateTime actualOutEnd  = date.Date.AddDays(1).Add(actualOutTs);

                // 3. Filter employee punches within the valid shift window
                var empLogs = allLogs
                    .Where(l => l.EmployeeCard == emp.Id && l.LogTime >= actualInStart && l.LogTime <= actualOutEnd)
                    .OrderBy(l => l.LogTime)
                    .ToList();

                var leave = leaves.FirstOrDefault(l => l.EmployeeId == emp.Id);
                existingAttendances.TryGetValue(emp.Id, out var attendance);

                if (attendance == null)
                {
                    attendance = new Attendance
                    {
                        EmployeeCard = emp.Id,
                        EmployeeId   = emp.EmployeeId,
                        CompanyId    = emp.Department?.CompanyId,
                        Date         = date.Date
                    };
                }

                // Reset record for re-calculation
                attendance.InTime    = null;
                attendance.OutTime   = null;
                attendance.OTHours   = 0;
                attendance.ShiftId   = shift.Id;
                string status        = "Absent";

                // 4. In/Out Time Logic (First/Last Punch)
                if (empLogs.Any())
                {
                    var firstLog = empLogs.First();
                    var lastLog  = empLogs.Count > 1 ? empLogs.Last() : null;

                    attendance.InTime  = firstLog.LogTime;
                    attendance.OutTime = lastLog?.LogTime;

                    // Normalization for calculations: If punch is before Official In, use Official In.
                    DateTime officialInDateTime = date.Date.Add(inTs);
                    DateTime effectiveInForMath = firstLog.LogTime < officialInDateTime ? officialInDateTime : firstLog.LogTime;

                    status = "Present";
                    if (!string.IsNullOrEmpty(shift.LateInTime) && TimeSpan.TryParse(shift.LateInTime, out var lateTs))
                    {
                        if (firstLog.LogTime > date.Date.Add(lateTs)) status = "Late";
                    }

                    // 5. Overtime (Worker Group Only, Rounded 45min+)
                    if (lastLog != null)
                    {
                        double lunchHrs = (double)shift.LunchHour;

                        // Overtime: Worker Group Only, Rounded 45min+
                        if (emp.Group?.NameEn?.ToLower() == "worker" && emp.IsOtEnabled)
                        {
                            DateTime officialOutDateTime = date.Date.Add(outTs);
                            if (officialOutDateTime < officialInDateTime) officialOutDateTime = officialOutDateTime.AddDays(1);

                            if (lastLog.LogTime > officialOutDateTime)
                            {
                                double otMins = (lastLog.LogTime - officialOutDateTime).TotalMinutes;
                                if (lunchHrs > 0) otMins -= (lunchHrs * 60);

                                int finalOt = (int)(otMins / 60);
                                if (otMins % 60 >= 45) finalOt += 1;
                                attendance.OTHours = Math.Max(0, finalOt);
                            }
                        }
                    }
                }
                else
                {
                    // Handle Absent/Leave/OffDay
                    status = leave != null 
                        ? "On Leave" 
                        : (roster?.IsOffDay == true || IsWeekend(date, shift.Weekends) ? "Off Day" : "Absent");
                }

                attendance.Status   = status;
                attendance.IsOffDay = status == "Off Day";

                if (attendance.Id == 0)
                {
                    attendance.Remarks = "Auto-processed";
                    _context.Attendances.Add(attendance);
                }
                else
                {
                    attendance.UpdatedAt = DateTime.UtcNow;
                    _context.Attendances.Update(attendance);
                }
            }

            await _context.SaveChangesAsync();
        }

        private bool IsWeekend(DateTime date, string? weekends)
        {
            if (string.IsNullOrEmpty(weekends)) return date.DayOfWeek == DayOfWeek.Friday;
            var days = weekends.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(d => d.Trim().ToLower());
            return days.Contains(date.DayOfWeek.ToString().ToLower());
        }

        public async Task ProcessBatchAttendanceAsync(DateTime? startDate, DateTime? endDate,
            List<string>? employeeCodes = null,
            int? departmentId = null,
            int? sectionId = null,
            int? designationId = null,
            int? lineId = null,
            int? shiftId = null,
            int? groupId = null,
            int? companyId = null)
        {
            if (!startDate.HasValue) startDate = DateTime.Today;
            if (!endDate.HasValue) endDate = startDate;

            for (var date = startDate.Value.Date; date <= endDate.Value.Date; date = date.AddDays(1))
            {
                await ProcessDailyAttendanceAsync(date, employeeCodes, departmentId, sectionId, designationId, lineId,
                    shiftId, groupId, companyId);
            }
        }

        public async Task<List<AttendanceLogDto>> GetAttendanceLogsAsync(DateTime? startDate, DateTime? endDate,
            string? searchTerm = null, int? companyId = null)
        {
            var query = _context.AttendanceLogs
                .Include(l => l.Employee)
                .ThenInclude(e => e!.Department)
                .AsQueryable();

            if (companyId.HasValue)
            {
                query = query.Where(l => l.Employee!.Department!.CompanyId == companyId.Value);
            }

            if (startDate.HasValue)
                query = query.Where(l => l.LogTime.Date >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(l => l.LogTime.Date <= endDate.Value.Date);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(l =>
                    l.Employee!.FullNameEn.Contains(searchTerm) || l.Employee.EmployeeId.Contains(searchTerm));
            }

            return await query
                .OrderByDescending(l => l.LogTime)
                .Select(l => new AttendanceLogDto
                {
                    Id = l.Id,
                    EmployeeCard = l.EmployeeCard,
                    EmployeeId = l.Employee != null ? l.Employee.EmployeeId : l.EmployeeId ?? "N/A",
                    EmployeeName = l.Employee != null ? l.Employee.FullNameEn : "Unknown",
                    DepartmentName = (l.Employee != null && l.Employee.Department != null) ? l.Employee.Department.NameEn : "N/A",
                    LogTime = l.LogTime,
                    DeviceId = l.DeviceId,
                    VerificationMode = l.VerificationMode,
                    CreatedAt = l.CreatedAt
                })
                .Take(500) // Limit for performance
                .ToListAsync();
        }

        public async Task<int> ClearAllAttendancesAsync()
        {
            var count = await _context.Attendances.CountAsync();
            _context.Attendances.RemoveRange(_context.Attendances);
            await _context.SaveChangesAsync();
            _logger.LogWarning($"Cleared {count} attendance records.");
            return count;
        }

        public async Task<int> ClearAllAttendanceLogsAsync()
        {
            var count = await _context.AttendanceLogs.CountAsync();
            _context.AttendanceLogs.RemoveRange(_context.AttendanceLogs);
            await _context.SaveChangesAsync();
            _logger.LogWarning($"Cleared {count} attendance log records.");
            return count;
        }

        public async Task<int> DeleteAttendanceLogsAsync(List<int> ids)
        {
            var logs = await _context.AttendanceLogs.Where(l => ids.Contains(l.Id)).ToListAsync();
            var count = logs.Count;
            _context.AttendanceLogs.RemoveRange(logs);
            await _context.SaveChangesAsync();
            return count;
        }
    }
}
