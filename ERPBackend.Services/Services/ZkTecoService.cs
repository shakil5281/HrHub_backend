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

            // Connection string for Microsoft Access (works with .mdb and .accdb)
            // Note: Requires Microsoft Access Database Engine (x64 if app is x64)
            string connectionString =
                $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Persist Security Info=False;";

            var logsToInsert = new List<AttendanceLog>();
            int newRecordsCount = 0;

            try
            {
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
                        var allCheckTimes = tempLogs.Select(l => l.CheckTime).Distinct().ToList();
                        var existingLogsLookup = await _context.AttendanceLogs
                            .Where(l => allCheckTimes.Contains(l.LogTime))
                            .Select(l => new { l.EmployeeId, l.LogTime })
                            .ToListAsync();

                        var existingLogsSet = new HashSet<string>(
                            existingLogsLookup.Select(l => $"{l.EmployeeId}|{l.LogTime:O}")
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
                                            EmployeeId = empInfo.Id,
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
            // 1. Fetch relevant logs: From current date 00:00 AM to next date 10:00 AM (to cover night shifts)
            var endTime = date.Date.AddDays(1).AddHours(10);

            var allLogs = await _context.AttendanceLogs
                .Where(l => l.LogTime >= date.Date && l.LogTime <= endTime) 
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

            // 4. Fetch roster/off days (optional, can add if modeled)
            var rosters = await _context.EmployeeShiftRosters
                .Where(r => r.Date.Date == date.Date)
                .ToListAsync();

            // 5. Fetch existing attendance for this date in one go
            var existingAttendances = await _context.Attendances
                .Where(a => a.Date == date.Date)
                .ToDictionaryAsync(a => a.EmployeeId);

            foreach (var emp in activeEmployees)
            {
                var shift = emp.Shift;
                
                // Determine search window for In/Out punches
                DateTime inWindowStart = date.Date.AddHours(6); // Default
                DateTime outWindowEnd = date.Date.AddDays(1).AddHours(10); // Default

                if (shift != null)
                {
                    if (TimeSpan.TryParse(shift.ActualInTime, out var aIn))
                        inWindowStart = date.Date.Add(aIn);
                    
                    if (TimeSpan.TryParse(shift.ActualOutTime, out var aOut))
                    {
                        // If ActualOut < ActualIn (or Shift In), it implies next day
                        if (TimeSpan.TryParse(shift.InTime, out var sIn) && aOut < sIn)
                            outWindowEnd = date.Date.AddDays(1).Add(aOut);
                        else
                            outWindowEnd = date.Date.Add(aOut);
                    }
                }

                var empLogs = allLogs.Where(l => l.EmployeeId == emp.Id && l.LogTime >= inWindowStart && l.LogTime <= outWindowEnd)
                    .OrderBy(l => l.LogTime)
                    .ToList();

                var leave = leaves.FirstOrDefault(l => l.EmployeeId == emp.Id);
                var roster = rosters.FirstOrDefault(r => r.EmployeeId == emp.Id);

                existingAttendances.TryGetValue(emp.Id, out var attendance);

                if (attendance == null)
                {
                    attendance = new Attendance
                    {
                        EmployeeId = emp.Id,
                        CompanyId = emp.Department?.CompanyId,
                        Date = date.Date,
                        Status = "Absent" // Default, updated below
                    };
                    _context.Attendances.Add(attendance);
                }
                else
                {
                    attendance.CompanyId = emp.Department?.CompanyId;
                }

                // Logic: Use punch starting inTime and last out time
                // If actual outTime < inTime (time of day), it implies next day
                
                var inTimeLog = empLogs.FirstOrDefault();
                var outTimeLog = empLogs.Count > 1 ? empLogs.LastOrDefault() : null;

                string? inTimeStr = null;
                string? outTimeStr = null;
                string status = "Absent";

                if (inTimeLog != null)
                {
                    inTimeStr = inTimeLog.LogTime.ToString("HH:mm:ss");
                    status = "Present";

                    // Late calculation relative to Shift InTime
                    if (shift != null && !string.IsNullOrEmpty(shift.InTime))
                    {
                        if (TimeSpan.TryParse(shift.InTime, out var shiftInTime))
                        {
                            var punchInTime = inTimeLog.LogTime.TimeOfDay;
                            // Use LateInTime if defined, else 15 min grace
                            var lateLimit = TimeSpan.TryParse(shift.LateInTime, out var lLimit)
                                ? lLimit
                                : shiftInTime.Add(TimeSpan.FromMinutes(15));

                            if (punchInTime > lateLimit)
                            {
                                status = "Late";
                            }
                        }
                    }

                    if (outTimeLog != null)
                    {
                        outTimeStr = outTimeLog.LogTime.ToString("HH:mm:ss");

                        // Calculate OT if enabled
                        if (emp.IsOtEnabled && shift != null && !string.IsNullOrEmpty(shift.OutTime) &&
                            TimeSpan.TryParse(shift.OutTime, out var shiftOutTimeValue))
                        {
                            var shiftOutTime = shiftOutTimeValue;
                            DateTime shiftOutDateTime = date.Date.Add(shiftOutTime);

                            // Handle overnight shift: If Shift Out < Shift In, Shift Out is next day
                            if (TimeSpan.TryParse(shift.InTime, out var sIn) && shiftOutTime < sIn)
                            {
                                shiftOutDateTime = date.Date.AddDays(1).Add(shiftOutTime);
                            }

                            if (outTimeLog.LogTime > shiftOutDateTime)
                            {
                                var otDuration = outTimeLog.LogTime - shiftOutDateTime;
                                attendance.OTHours = (decimal)Math.Round(otDuration.TotalHours, 2);
                            }
                        }
                    }
                    else if (status == "Present")
                    {
                        // Check if we should search specifically for next day out time if no out time was found in current window
                        // However, current window [date, date + 1 day 07:14] already covers most night shifts.
                    }
                }
                else
                {
                    // No punches found
                    status = leave != null ? "On Leave" : (roster?.IsOffDay == true || IsWeekend(date, emp.Shift?.Weekends) ? "Off Day" : "Absent");
                }

                // Finalize Assignment
                attendance.InTime = inTimeStr;
                attendance.OutTime = outTimeStr;
                attendance.Status = status;

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
            var query = _context.AttendanceLogs.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(l => l.LogTime.Date >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(l => l.LogTime.Date <= endDate.Value.Date);

            var dates = await query.Select(l => l.LogTime.Date).Distinct().ToListAsync();

            foreach (var date in dates.OrderBy(d => d))
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
                    EmployeeId = l.EmployeeId,
                    EmployeeIdCard = l.Employee!.EmployeeId,
                    EmployeeName = l.Employee.FullNameEn,
                    DepartmentName = l.Employee.Department!.NameEn,
                    LogTime = l.LogTime,
                    DeviceId = l.DeviceId,
                    VerificationMode = l.VerificationMode,
                    CreatedAt = l.CreatedAt
                })
                .Take(500) // Limit for performance
                .ToListAsync();
        }
    }
}
