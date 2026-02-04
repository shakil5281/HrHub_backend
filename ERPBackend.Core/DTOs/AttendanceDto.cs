namespace ERPBackend.Core.DTOs
{
    public class AttendanceDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeIdCard { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string Line { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string Shift { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? InTime { get; set; }
        public string? OutTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal OTHours { get; set; }
    }

    public class CreateAttendanceDto
    {
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public string? InTime { get; set; }
        public string? OutTime { get; set; }
        public string Status { get; set; } = "Present";
        public decimal OTHours { get; set; }
    }

    public class AttendanceSummaryDto
    {
        public int TotalHeadcount { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public int LeaveCount { get; set; }
        public double AttendanceRate { get; set; }
    }

    public class DepartmentDailySummaryDto
    {
        public int Id { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int TotalEmployees { get; set; }
        public int Present { get; set; }
        public int Absent { get; set; }
        public int Late { get; set; }
        public int OnLeave { get; set; }
        public double AttendanceRate { get; set; }
    }

    public class SectionDailySummaryDto
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int TotalEmployees { get; set; }
        public int Present { get; set; }
        public int Absent { get; set; }
        public int Late { get; set; }
        public int OnLeave { get; set; }
        public double AttendanceRate { get; set; }
    }

    public class DesignationDailySummaryDto
    {
        public int Id { get; set; }
        public int DesignationId { get; set; }
        public string DesignationName { get; set; } = string.Empty;
        public int TotalEmployees { get; set; }
        public int Present { get; set; }
        public int Absent { get; set; }
        public int Late { get; set; }
        public int OnLeave { get; set; }
        public double AttendanceRate { get; set; }
    }

    public class LineDailySummaryDto
    {
        public int Id { get; set; }
        public int LineId { get; set; }
        public string LineName { get; set; } = string.Empty;
        public int TotalEmployees { get; set; }
        public int Present { get; set; }
        public int Absent { get; set; }
        public int Late { get; set; }
        public int OnLeave { get; set; }
        public double AttendanceRate { get; set; }
    }

    public class GroupDailySummaryDto
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int TotalEmployees { get; set; }
        public int Present { get; set; }
        public int Absent { get; set; }
        public int Late { get; set; }
        public int OnLeave { get; set; }
        public double AttendanceRate { get; set; }
    }

    public class DeptSectionDailySummaryDto
    {
        public string Id { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int TotalEmployees { get; set; }
        public int Present { get; set; }
        public int Absent { get; set; }
        public int Late { get; set; }
        public int OnLeave { get; set; }
        public double AttendanceRate { get; set; }
    }

    public class DailySummaryResponseDto
    {
        public AttendanceSummaryDto OverallSummary { get; set; } = new();
        public List<DepartmentDailySummaryDto> DepartmentSummaries { get; set; } = new();
        public List<SectionDailySummaryDto> SectionSummaries { get; set; } = new();
        public List<DeptSectionDailySummaryDto> DeptSectionSummaries { get; set; } = new();
        public List<DesignationDailySummaryDto> DesignationSummaries { get; set; } = new();
        public List<LineDailySummaryDto> LineSummaries { get; set; } = new();
        public List<GroupDailySummaryDto> GroupSummaries { get; set; } = new();
    }

    public class DailyProcessDto
    {
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? DepartmentId { get; set; }
    }
}
