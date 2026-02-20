using ERPBackend.Core.DTOs;
using ERPBackend.Core.Interfaces;
using ERPBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPBackend.Services.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
        {
            var today = DateTime.UtcNow.Date;

            var totalEmployees = await _context.Employees.CountAsync(e => e.IsActive);

            // Assuming 'Present' status check. 
            // Note: Attendance Date is DateTime, so compare Date component
            var presentToday = await _context.Attendances
                .CountAsync(a => a.Date.Date == today && a.Status == "Present");

            var onLeaveToday = await _context.LeaveApplications
                .CountAsync(l => l.StartDate.Date <= today && l.EndDate.Date >= today && l.Status == "Approved");

            // Open Positions: Sum of (Required - Fulfilled). 
            // We need to calculate Fulfilled count per requirement (Dept + Designation)
            // But doing that in one query might be complex. 
            // Simplified approach: Sum of RequiredCount from ManpowerRequirements minus Total Employees (naive)
            // Better approach: Get all requirements, then for each, count employees. 
            // Given performance, let's just sum RequiredCount for now as "Open Positions" usually means "Vacancies".
            // If the system tracks "Vacancies" explicitly, good. If not, we have to calculate.
            // Let's assume ManpowerRequirement.RequiredCount IS the vacancy count for now to keep it simple, 
            // OR fetch all and calc.

            // Let's try to calculate roughly.
            var requirements = await _context.ManpowerRequirements
                .Include(m => m.Department)
                .Include(m => m.Designation)
                .ToListAsync();

            int openPositions = 0;

            // This loop might be slow if many requirements, but usually manageable.
            foreach (var req in requirements)
            {
                var currentCount = await _context.Employees
                    .CountAsync(e =>
                        e.DepartmentId == req.DepartmentId && e.DesignationId == req.DesignationId && e.IsActive);

                var vacancy = req.RequiredCount - currentCount;
                if (vacancy > 0) openPositions += vacancy;
            }

            return new DashboardSummaryDto
            {
                TotalWorkforce = totalEmployees,
                PresentToday = presentToday,
                OnLeaveToday = onLeaveToday,
                OpenPositions = openPositions,
                WorkforceGrowth = 0.0, // Placeholder
                AttendanceTrend = 0.0 // Placeholder
            };
        }

        public async Task<List<AttendanceStatDto>> GetAttendanceStatsAsync()
        {
            var stats = new List<AttendanceStatDto>();
            var today = DateTime.UtcNow.Date;

            // Last 7 days including today
            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var presentCount = await _context.Attendances
                    .CountAsync(a => a.Date.Date == date && a.Status == "Present");

                // Target count is total active employees on that day. approximate with current total.
                var targetCount = await _context.Employees.CountAsync(e => e.IsActive); // Simplified

                stats.Add(new AttendanceStatDto
                {
                    Date = date.ToString("ddd"), // Mon, Tue...
                    PresentCount = presentCount,
                    TargetCount = targetCount
                });
            }

            return stats;
        }

        public async Task<List<DepartmentStatDto>> GetDepartmentStatsAsync()
        {
            var stats = await _context.Employees
                .Where(e => e.IsActive && e.Department != null)
                .GroupBy(e => e.Department!.NameEn)
                .Select(g => new DepartmentStatDto
                {
                    DepartmentName = g.Key,
                    EmployeeCount = g.Count(),
                    Color = "#000000" // Frontend will assign colors
                })
                .ToListAsync();

            // Assign colors round-robin mock or random if needed, but safer to let Frontend handle palette
            return stats;
        }

        public async Task<List<RecentHireDto>> GetRecentHiresAsync()
        {
            return await _context.Employees
                .Where(e => e.IsActive)
                .OrderByDescending(e => e.JoinDate)
                .Take(5)
                .Select(e => new RecentHireDto
                {
                    Name = e.FullNameEn,
                    Position = e.Designation != null ? e.Designation.NameEn : "N/A",
                    Department = e.Department != null ? e.Department.NameEn : "N/A",
                    JoinDate = e.JoinDate.ToString("MMM dd, yyyy"),
                    ImageUrl = e.ProfileImageUrl ?? ""
                })
                .ToListAsync();
        }

        /// <summary>
        /// Helper method to safely get the next occurrence of a specific month/day from a reference date.
        /// Handles edge cases like Feb 29 in non-leap years by adjusting to Feb 28.
        /// </summary>
        private DateTime? GetNextOccurrence(DateTime referenceDate, int month, int day)
        {
            try
            {
                // Try to create the date in the current year
                var nextDate = new DateTime(referenceDate.Year, month, day);

                // If the date is in the past, try next year
                if (nextDate < referenceDate)
                {
                    nextDate = new DateTime(referenceDate.Year + 1, month, day);
                }

                return nextDate;
            }
            catch (ArgumentOutOfRangeException)
            {
                // Handle invalid dates (e.g., Feb 29 in non-leap year)
                // Adjust to the last valid day of the month
                try
                {
                    var adjustedDay = DateTime.DaysInMonth(referenceDate.Year, month);
                    var nextDate = new DateTime(referenceDate.Year, month, adjustedDay);

                    if (nextDate < referenceDate)
                    {
                        adjustedDay = DateTime.DaysInMonth(referenceDate.Year + 1, month);
                        nextDate = new DateTime(referenceDate.Year + 1, month, adjustedDay);
                    }

                    return nextDate;
                }
                catch
                {
                    // If still fails, return null
                    return null;
                }
            }
        }

        public async Task<List<UpcomingEventDto>> GetUpcomingEventsAsync()

        {
            var today = DateTime.UtcNow.Date;
            var nextMonth = today.AddDays(30);

            // Upcoming Birthdays
            // EF Core might not translate some Date functions well depending on provider.
            // Fetching candidates and filtering in memory for small datasets is safer if provider is strict (like SQL Server 2008).
            // However, SQL Server 2008 supports DATEPART.

            var employees = await _context.Employees.Where(e => e.IsActive).ToListAsync();

            var events = new List<UpcomingEventDto>();

            foreach (var emp in employees)
            {
                if (emp.DateOfBirth.HasValue)
                {
                    var dob = emp.DateOfBirth.Value;
                    var nextBirthday = GetNextOccurrence(today, dob.Month, dob.Day);

                    if (nextBirthday.HasValue && nextBirthday.Value <= nextMonth)
                    {
                        events.Add(new UpcomingEventDto
                        {
                            Name = emp.FullNameEn,
                            EventType = "Birthday",
                            Date = nextBirthday.Value.ToString("MMM dd")
                        });
                    }
                }

                // Work Anniversary
                var joinDate = emp.JoinDate;
                var nextAnniversary = GetNextOccurrence(today, joinDate.Month, joinDate.Day);

                if (nextAnniversary.HasValue && nextAnniversary.Value <= nextMonth)
                {
                    events.Add(new UpcomingEventDto
                    {
                        Name = emp.FullNameEn,
                        EventType = "Work Anniversary",
                        Date = nextAnniversary.Value.ToString("MMM dd")
                    });
                }
            }

            return events.OrderBy(e => e.Date).Take(5).ToList();
        }
    }
}
