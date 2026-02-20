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
            // 1. Fetch relevant logs: Extended window to cover full 24-hour cycles and long OT
            var endTime = date.Date.AddDays(2);

            var allLogs = await _context.AttendanceLogs
                .Where(l => l.LogTime >= date.Date && l.LogTime < endTime)
                .ToListAsync();

            // 2. Fetch all active employees
            var queryEmployees = _context.Employees
                .Where(e => e.IsActive)
                .Include(e => e.Shift)
                .Include(e => e.Department)
                .AsQueryable();

            if (employeeCodes != null && employeeCodes.Any())
            {
                queryEmployees = queryEmployees.Where(e => employeeCodes.Contains(e.EmployeeId));
            }

            if (departmentId.HasValue)
            {
                queryEmployees = queryEmployees.Where(e => e.DepartmentId == departmentId.Value);
            }

            if (sectionId.HasValue) queryEmployees = queryEmployees.Where(e => e.SectionId == sectionId.Value);
            if (designationId.HasValue)
                queryEmployees = queryEmployees.Where(e => e.DesignationId == designationId.Value);
            if (lineId.HasValue) queryEmployees = queryEmployees.Where(e => e.LineId == lineId.Value);
            if (shiftId.HasValue) queryEmployees = queryEmployees.Where(e => e.ShiftId == shiftId.Value);
            if (groupId.HasValue) queryEmployees = queryEmployees.Where(e => e.GroupId == groupId.Value);
            if (companyId.HasValue)
            {
                queryEmployees = queryEmployees.Where(e => e.Department!.CompanyId == companyId.Value);
            }

            var activeEmployees = await queryEmployees.ToListAsync();

            // 3. Fetch leaves for this date
            var leaves = await _context.LeaveApplications
                .Where(l => l.Status == "Approved" && l.StartDate.Date <= date.Date && l.EndDate.Date >= date.Date)
                .ToListAsync();

            // 4. Fetch roster/off days (with fallback logic)
            var empSecondaryIds = activeEmployees.Select(e => e.Id).ToList();
            var allRosters = await _context.EmployeeShiftRosters
                .Include(r => r.Shift)
                .Where(r => empSecondaryIds.Contains(r.EmployeeId) && r.Date.Date <= date.Date)
                .OrderByDescending(r => r.Date)
                .ToListAsync();

            // 5. Fetch existing attendance for this date in one go
            var existingAttendances = await _context.Attendances
                .Where(a => a.Date == date.Date)
                .ToDictionaryAsync(a => a.EmployeeCard);


            foreach (var emp in activeEmployees)
            {
                var shift = emp.Shift;

                // Determine search window for In/Out punches based on ActualInTime
                DateTime actualInTimeRef = date.Date.AddHours(4); // Default fallback: Start search at 4 AM
                DateTime inWindowEnd = date.Date.AddHours(12); // Default arrival window: until Noon
                DateTime outWindowEnd = date.Date.AddDays(1).Add(new TimeSpan(7, 59, 0)); // Default cutoff

                if (shift != null && TimeSpan.TryParse(shift.ActualInTime, out var actualIn))
                {
                    // InTime search begins 4 hours before shift start (e.g., 04:00 AM for 08:00 AM shift)
                    actualInTimeRef = date.Date.Add(actualIn).AddHours(-4);
                    // Arrival window ends 4 hours after shift start (e.g., 12:00 PM for 08:00 AM shift)
                    inWindowEnd = date.Date.Add(actualIn).AddHours(4);

                    // OutTime cutoff is strictly 24 hours (minus 1 min) from the SHIFT IN time.
                    // This allows punches at 07:14 AM to be counted for a 07:15 AM shift as requested.
                    outWindowEnd = date.Date.AddDays(1).Add(actualIn).AddMinutes(-1);
                }

                // Get all punches for this employee within the extended window
                var empLogs = allLogs.Where(l =>
                        l.EmployeeCard == emp.Id &&
                        l.LogTime >= actualInTimeRef &&
                        l.LogTime <= outWindowEnd)
                    .OrderBy(l => l.LogTime)
                    .ToList();

                var leave = leaves.FirstOrDefault(l => l.EmployeeId == emp.Id);
                
                // Fallback logic: 1. Direct match for date, 2. Last submitted roster before date
                var employeeRosters = allRosters.Where(r => r.EmployeeId == emp.Id).ToList();
                var roster = employeeRosters.FirstOrDefault(r => r.Date.Date == date.Date) 
                             ?? employeeRosters.FirstOrDefault(); // allRosters is already ordered by date DESC and filtered by <= date


                existingAttendances.TryGetValue(emp.Id, out var attendance);

                if (attendance == null)
                {
                    attendance = new Attendance
                    {
                        EmployeeCard = emp.Id,
                        EmployeeId = emp.EmployeeId,
                        CompanyId = emp.Department?.CompanyId,
                        Date = date.Date,
                        Status = "Absent" // Default
                    };
                    _context.Attendances.Add(attendance);
                }
                else
                {
                    attendance.CompanyId = emp.Department?.CompanyId;
                }

                // Reset data for fresh calculation
                attendance.InTime = null;
                attendance.OutTime = null;
                attendance.Status = "Absent";
                attendance.OTHours = 0;
                attendance.ShiftId = roster?.ShiftId ?? emp.ShiftId;
                attendance.IsOffDay = false;


                DateTime? inTime = null;
                DateTime? outTime = null;
                string status = "Absent";

                if (empLogs.Any())
                {
                    // InTime Logic: Find FIRST punch occurring within the InTime window
                    AttendanceLog? inTimeLog = null;
                    var inTimeCandidates = empLogs.Where(l => l.LogTime <= inWindowEnd).ToList();

                    if (inTimeCandidates.Any())
                    {
                        inTimeLog = inTimeCandidates.OrderBy(l => l.LogTime).First();
                        inTime = inTimeLog.LogTime;
                        status = "Present";

                        // Late detection logic...
                        if (shift != null && !string.IsNullOrEmpty(shift.LateInTime) &&
                            TimeSpan.TryParse(shift.LateInTime, out var lateInTimeValue))
                        {
                            var lateInDateTime = date.Date.Add(lateInTimeValue);
                            if (inTimeLog.LogTime > lateInDateTime) status = "Late";
                        }
                    }

                    // OutTime Logic: Find the LATEST punch occurring AFTER the InTime (buffer 30 min)
                    // This avoids picking the arrival punch as departure while catching everything else.
                    var outTimeLog = inTime.HasValue
                        ? empLogs.Where(l => l.LogTime > inTime.Value.AddMinutes(30)).LastOrDefault()
                        : empLogs.Where(l => l.LogTime > inWindowEnd).LastOrDefault();

                    if (outTimeLog != null)
                    {
                        outTime = outTimeLog.LogTime;

                        // Calculate OT based on OFFICE OUT TIME (not Actual Out Time)
                        if (emp.IsOtEnabled && shift != null && !string.IsNullOrEmpty(shift.OutTime) &&
                            TimeSpan.TryParse(shift.OutTime, out var officeOutTimeValue))
                        {
                            var officeOutTime = officeOutTimeValue;
                            DateTime officeOutDateTime = date.Date.Add(officeOutTime);

                            // Handle overnight shift: If Office Out < Actual In, Office Out is next day
                            if (shift.ActualInTime != null && TimeSpan.TryParse(shift.ActualInTime, out var aIn) &&
                                officeOutTime < aIn)
                            {
                                officeOutDateTime = date.Date.AddDays(1).Add(officeOutTime);
                            }

                            if (emp.IsOtEnabled && outTimeLog.LogTime > officeOutDateTime)
                            {
                                var otDuration = outTimeLog.LogTime - officeOutDateTime;
                                double totalMinutes = otDuration.TotalMinutes;

                                // Rounding Rule: 45 minutes up counts as 1 hour
                                int fullHours = (int)(totalMinutes / 60);
                                int remainingMinutes = (int)(totalMinutes % 60);

                                attendance.OTHours = remainingMinutes >= 45 ? fullHours + 1 : fullHours;
                            }
                            else
                            {
                                attendance.OTHours = 0;
                            }
                        }
                    }
                }
                else
                {
                    // No punches found
                    status = leave != null
                        ? "On Leave"
                        : (roster?.IsOffDay == true || IsWeekend(date, emp.Shift?.Weekends) ? "Off Day" : "Absent");
                }

                // Assign final DateTime values
                attendance.InTime = inTime;
                attendance.OutTime = outTime;
                attendance.Status = status;
                attendance.IsOffDay = status == "Off Day";


                if (attendance.Id == 0)
                {
                    attendance.Remarks = "Auto-processed";
                    _context.Attendances.Add(attendance);
                }
                else
                {
                    attendance.UpdatedAt = DateTime.UtcNow;
                    attendance.Remarks = (attendance.Remarks ?? "").Contains("(Updated)")
                        ? attendance.Remarks
                        : (attendance.Remarks ?? "") + " (Updated)";
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
