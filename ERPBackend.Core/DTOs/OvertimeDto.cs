namespace ERPBackend.Core.DTOs
{
    public class DailyOTSheetDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeIdCard { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? InTime { get; set; }
        public string? OutTime { get; set; }
        public decimal RegularHours { get; set; }
        public decimal OTHours { get; set; }
        public string? Remarks { get; set; }
    }

    public class DailyOTSummaryDto
    {
        public int Id { get; set; }
        public string Department { get; set; } = string.Empty;
        public int EmployeeCount { get; set; }
        public decimal TotalOTHours { get; set; }
        public decimal AverageOTPerEmployee { get; set; }
        public decimal TotalRegularHours { get; set; }
    }

    public class OTSheetResponseDto
    {
        public List<DailyOTSheetDto> Records { get; set; } = new();
        public decimal TotalOTHours { get; set; }
        public int TotalEmployees { get; set; }
    }

    public class OTSummaryResponseDto
    {
        public List<DailyOTSummaryDto> DepartmentSummaries { get; set; } = new();
        public decimal GrandTotalOTHours { get; set; }
        public int TotalEmployees { get; set; }
        public DateTime Date { get; set; }
    }
}
