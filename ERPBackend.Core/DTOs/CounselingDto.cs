namespace ERPBackend.Core.DTOs
{
    public class CounselingRecordDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeIdCard { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public DateTime CounselingDate { get; set; }
        public string IssueType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ActionTaken { get; set; }
        public string? FollowUpNotes { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime? FollowUpDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCounselingRecordDto
    {
        public int EmployeeId { get; set; }
        public DateTime CounselingDate { get; set; }
        public string IssueType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ActionTaken { get; set; }
        public string? FollowUpNotes { get; set; }
        public string Status { get; set; } = "Open";
        public string Severity { get; set; } = "Low";
        public DateTime? FollowUpDate { get; set; }
    }

    public class CounselingSummaryDto
    {
        public int TotalRecords { get; set; }
        public int OpenCases { get; set; }
        public int ClosedCases { get; set; }
        public int HighSeverity { get; set; }
        public int RequiringFollowUp { get; set; }
    }

    public class CounselingResponseDto
    {
        public CounselingSummaryDto Summary { get; set; } = new();
        public List<CounselingRecordDto> Records { get; set; } = new();
    }
}
