namespace ERPBackend.Core.DTOs
{
    public class DashboardSummaryDto
    {
        public int TotalWorkforce { get; set; }
        public int PresentToday { get; set; }
        public int OnLeaveToday { get; set; }
        public int OpenPositions { get; set; }
        
        // Growth stats (mock or calc if possible, let's keep it simple for now or calc vs last month)
        public double WorkforceGrowth { get; set; } // % vs last month
        public double AttendanceTrend { get; set; } // % vs yesterday
    }

    public class AttendanceStatDto
    {
        public string Date { get; set; } = string.Empty; // Format "Mon", "Tue" or "Jan 01"
        public int PresentCount { get; set; }
        public int TargetCount { get; set; } // Total active employees that should be present
    }

    public class DepartmentStatDto
    {
        public string DepartmentName { get; set; } = string.Empty;
        public int EmployeeCount { get; set; }
        public string Color { get; set; } = "#000000"; // Optional, can be assigned by FE
    }

    public class RecentHireDto
    {
        public string Name { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string JoinDate { get; set; } = string.Empty; // Formatted date
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class UpcomingEventDto
    {
        public string Name { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty; // "Birthday", "Work Anniversary"
        public string Date { get; set; } = string.Empty; // "Tomorrow", "Jan 31"
        public string Color { get; set; } = string.Empty; // Optional
    }
}
