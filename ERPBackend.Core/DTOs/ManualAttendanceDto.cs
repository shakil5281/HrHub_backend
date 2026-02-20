namespace ERPBackend.Core.DTOs
{
    public class ManualAttendanceDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public DateTime Date { get; set; }
        public DateTime? InTime { get; set; }
        public DateTime? OutTime { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public string Status { get; set; } = "Present"; // Present, Late, Absent, On Leave
    }

    public class ManualAttendanceResponseDto
    {
        public int Id { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public DateTime? InTime { get; set; }
        public DateTime? OutTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ManualAttendanceHistoryDto
    {
        public int Id { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public DateTime? InTime { get; set; }
        public DateTime? OutTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class BulkManualAttendanceDto
    {
        public List<string> EmployeeIds { get; set; } = new();
        public int? CompanyId { get; set; }
        public DateTime Date { get; set; }
        public DateTime? InTime { get; set; }
        public DateTime? OutTime { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "Present";
    }

    public class DeleteAttendanceDto
    {
        public List<string>? EmployeeIds { get; set; }
        public int? CompanyId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? DepartmentId { get; set; }
        public int? SectionId { get; set; }
    }
}
