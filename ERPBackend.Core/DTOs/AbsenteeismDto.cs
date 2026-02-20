namespace ERPBackend.Core.DTOs
{
    public class AbsenteeismRecordDto
    {
        public int Id { get; set; }
        public int EmployeeCard { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty; // Absent, On Leave
        public int ConsecutiveDays { get; set; }
        public string? Remarks { get; set; }
    }

    public class AbsenteeismSummaryDto
    {
        public int TotalAbsent { get; set; }
        public int AbsentWithoutLeave { get; set; }
        public int OnLeave { get; set; }
        public int CriticalCases { get; set; } // 3+ consecutive days
    }

    public class AbsenteeismResponseDto
    {
        public AbsenteeismSummaryDto Summary { get; set; } = new();
        public List<AbsenteeismRecordDto> Records { get; set; } = new();
    }
}
