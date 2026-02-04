namespace ERPBackend.Core.DTOs
{
    public class OTDeductionDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeIdCard { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal DeductionHours { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateOTDeductionDto
    {
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public decimal DeductionHours { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public string Status { get; set; } = "Approved";
    }

    public class OTDeductionSummaryDto
    {
        public decimal TotalDeductedHours { get; set; }
        public int TotalEmployeesAffected { get; set; }
        public int PendingRequests { get; set; }
        public decimal AverageDeduction { get; set; }
    }

    public class OTDeductionResponseDto
    {
        public OTDeductionSummaryDto Summary { get; set; } = new();
        public List<OTDeductionDto> Records { get; set; } = new();
    }
}
