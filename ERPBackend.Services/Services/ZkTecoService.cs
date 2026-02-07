using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Threading.Tasks;
using ERPBackend.Core.Interfaces;
using ERPBackend.Core.Models;
using ERPBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;

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

        public async Task<int> SyncDataFromDeviceAsync(string dbPath, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!System.IO.File.Exists(dbPath))
            {
                throw new FileNotFoundException("ZKTeco database file not found.", dbPath);
            }

            // Connection string for Microsoft Access (works with .mdb and .accdb)
            // Note: Requires Microsoft Access Database Engine (x64 if app is x64)
            string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Persist Security Info=False;";

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
                                string badgeNumber = reader["Badgenumber"]?.ToString() ?? string.Empty;
                                if (!userMap.ContainsKey(userId))
                                {
                                    userMap.Add(userId, badgeNumber);
                                }
                            }
                        }
                    }

                    // 2. Get existing employees to validate map
                    var validEmployees = await _context.Employees
                        .Select(e => new { e.Id, e.EmployeeId }) // EmployeeId in DB matches Badgenumber
                        .ToDictionaryAsync(e => e.EmployeeId, e => e.Id);

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
                                string deviceId = reader["SENSORID"]?.ToString() ?? string.Empty;
                                // checktype: 0/1/etc. we assume standard
                                
                                if (userMap.TryGetValue(userId, out string? badgeNumber) && badgeNumber != null)
                                {
                                    if (validEmployees.TryGetValue(badgeNumber, out int employeeDbId))
                                    {
                                        // Check if already exists
                                        bool exists = await _context.AttendanceLogs.AnyAsync(l => 
                                            l.EmployeeId == employeeDbId && 
                                            l.LogTime == checkTime);

                                        if (!exists)
                                        {
                                            logsToInsert.Add(new AttendanceLog
                                            {
                                                EmployeeId = employeeDbId,
                                                LogTime = checkTime,
                                                DeviceId = deviceId,
                                                VerificationMode = "Device" // Or interpret CHECKTYPE
                                            });
                                            newRecordsCount++;
                                        }
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

        public async Task ProcessDailyAttendanceAsync(DateTime date)
        {
            // 1. Fetch relevant logs: From current date 07:15 AM to next date 07:14 AM (to cover the Out window)
            var startTime = date.Date.AddHours(7).AddMinutes(15);
            var endTime = date.Date.AddDays(1).AddHours(7).AddMinutes(14);

            var allLogs = await _context.AttendanceLogs
                .Where(l => l.LogTime >= date.Date && l.LogTime <= endTime) // Fetch slightly more to be safe
                .ToListAsync();

            // 2. Fetch all active employees
            var activeEmployees = await _context.Employees
                .Where(e => e.IsActive)
                .Include(e => e.Shift)
                .ToListAsync();

            // 3. Fetch leaves for this date
            var leaves = await _context.LeaveApplications
                .Where(l => l.Status == "Approved" && l.StartDate.Date <= date.Date && l.EndDate.Date >= date.Date)
                .ToListAsync();

            // 4. Fetch roster/off days (optional, can add if modeled)
            var rosters = await _context.EmployeeShiftRosters
                .Where(r => r.Date.Date == date.Date)
                .ToListAsync();

            foreach (var emp in activeEmployees)
            {
                var empLogs = allLogs.Where(l => l.EmployeeId == emp.Id).OrderBy(l => l.LogTime).ToList();
                var leave = leaves.FirstOrDefault(l => l.EmployeeId == emp.Id);
                var roster = rosters.FirstOrDefault(r => r.EmployeeId == emp.Id);

                var attendance = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.EmployeeId == emp.Id && a.Date == date.Date);

                if (attendance == null)
                {
                    attendance = new Attendance
                    {
                        EmployeeId = emp.Id,
                        Date = date.Date,
                        Status = "Absent" // Default, updated below
                    };
                }

                string status = "Absent";
                string? inTimeStr = null;
                string? outTimeStr = null;

                if (leave != null)
                {
                    status = "On Leave";
                }
                else if (roster?.IsOffDay == true || IsWeekend(date, emp.Shift?.Weekends))
                {
                    status = "Off Day";
                }
                else
                {
                    // In Time logic: Only 07:15 AM to 11:00 AM
                    var inTimeLog = empLogs.FirstOrDefault(l => 
                        l.LogTime >= date.Date.AddHours(7).AddMinutes(15) && 
                        l.LogTime <= date.Date.AddHours(11));

                    // Out Time logic: 11:01 AM to next date 07:14 AM
                    var outTimeLog = empLogs.LastOrDefault(l => 
                        l.LogTime >= date.Date.AddHours(11).AddMinutes(1) && 
                        l.LogTime <= date.Date.AddDays(1).AddHours(7).AddMinutes(14));
                    
                    var shift = emp.Shift;

                    if (inTimeLog != null)
                    {
                        inTimeStr = inTimeLog.LogTime.ToString("HH:mm:ss");
                        status = "Present";

                        // Late calculation
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
                    }
                    
                    if (outTimeLog != null)
                    {
                        outTimeStr = outTimeLog.LogTime.ToString("HH:mm:ss");
                        // If we have an out time but no in time, it's still some form of presence (One Punch)
                        if (inTimeLog == null)
                        {
                            status = "Present (Out Only)";
                        }

                        // Calculate OT
                        if (emp.IsOTEnabled && shift != null && !string.IsNullOrEmpty(shift.OutTime) && TimeSpan.TryParse(shift.OutTime, out var shiftOutTime))
                        {
                            // Construct Shift Out DateTime
                            // Shift Out is strictly on the date (or next day if overnight, but here specific logic: 11:01 AM to next date 07:14 AM covers it)
                            // Usually Shift Out Time is relative to day start.
                            // If Shift In is 08:00 and Out is 17:00.
                            // If In 20:00 and Out 05:00 (Next Day).

                            DateTime shiftOutDateTime = date.Date.Add(shiftOutTime);
                            // If shiftOutTime < shiftInTime, it might be next day?
                            // Simple logic: If punchOutLog is > shiftOutDateTime, it is OT.
                            // However, we need to handle overnight shifts carefully.
                            // For simplicity, assuming if shiftOutTime is small (e.g. 05:00), it implies next day if InTime is large.
                            // But here let's rely on the fact we matched an outTimeLog that is logically "after" work.
                            
                            // Let's assume standard day shift for now or rely on strict date comparison
                            // If we matched the OutTimeLog in the window [11:01 AM, Next 07:14 AM]
                            // And Shift Out is say 17:00 (5 PM).
                            
                            // Handle cross-day shift out time logic if needed (e.g. if OutTime < InTime)
                            if (TimeSpan.TryParse(shift.InTime, out var sIn) && shiftOutTime < sIn)
                            {
                                shiftOutDateTime = date.Date.AddDays(1).Add(shiftOutTime);
                            }

                            if (outTimeLog.LogTime > shiftOutDateTime)
                            {
                                var otDuration = outTimeLog.LogTime - shiftOutDateTime;
                                // Round to 2 decimal places
                                attendance.OTHours = (decimal)Math.Round(otDuration.TotalHours, 2);
                            }
                        }
                    }
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

        public async Task ProcessBatchAttendanceAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.AttendanceLogs.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(l => l.LogTime.Date >= startDate.Value.Date);
            
            if (endDate.HasValue)
                query = query.Where(l => l.LogTime.Date <= endDate.Value.Date);

            var dates = await query.Select(l => l.LogTime.Date).Distinct().ToListAsync();

            foreach (var date in dates.OrderBy(d => d))
            {
                await ProcessDailyAttendanceAsync(date);
            }
        }
    }
}
