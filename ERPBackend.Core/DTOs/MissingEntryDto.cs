namespace ERPBackend.Core.DTOs
{
    public class MissingEntryDto
    {
        public int Id { get; set; }
        public int EmployeeCard { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public int? CompanyId { get; set; }
        public string Department { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string? Shift { get; set; }
        public DateTime Date { get; set; }
        public DateTime? InTime { get; set; }
        public DateTime? OutTime { get; set; }
        public string MissingType { get; set; } = string.Empty; // "In Time", "Out Time", "Both"
        public string Status { get; set; } = string.Empty; // "Pending", "Critical"
    }

    public class MissingEntrySummaryDto
    {
        public int TotalMissing { get; set; }
        public int MissingInTime { get; set; }
        public int MissingOutTime { get; set; }
        public int MissingBoth { get; set; }
        public int CriticalCount { get; set; }
    }

    public class MissingEntryResponseDto
    {
        public MissingEntrySummaryDto Summary { get; set; } = new();
        public List<MissingEntryDto> Entries { get; set; } = new();
    }
}
