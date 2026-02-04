namespace ERPBackend.Core.DTOs
{
    public class JobCardDto
    {
        public string Date { get; set; } = string.Empty;
        public string Day { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? InTime { get; set; }
        public string? OutTime { get; set; }
        public int LateMinutes { get; set; }
        public int EarlyMinutes { get; set; }
        public decimal OTHours { get; set; }
        public decimal TotalHours { get; set; }
        public string? Remarks { get; set; }
    }

    public class JobCardSummaryDto
    {
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int WeekendDays { get; set; }
        public int HolidayDays { get; set; }
        public decimal TotalOTHours { get; set; }
        public int TotalLateMinutes { get; set; }
        public int TotalEarlyMinutes { get; set; }
    }

    public class EmployeeJobCardDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeIdCard { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string? JoiningDate { get; set; }
        public string? Grade { get; set; }
        public string? Shift { get; set; }
    }

    public class JobCardResponseDto
    {
        public EmployeeJobCardDto Employee { get; set; } = new();
        public JobCardSummaryDto Summary { get; set; } = new();
        public List<JobCardDto> AttendanceRecords { get; set; } = new();
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
