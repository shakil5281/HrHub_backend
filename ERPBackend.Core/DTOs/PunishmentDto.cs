namespace ERPBackend.Core.DTOs
{
    public class EmployeePunishmentDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeCard { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string PunishmentType { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public decimal FineAmount { get; set; }
        public int SuspensionDays { get; set; }
        public DateTime PunishmentDate { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateEmployeePunishmentDto
    {
        public int EmployeeId { get; set; }
        public string PunishmentType { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public decimal FineAmount { get; set; }
        public int SuspensionDays { get; set; }
        public DateTime PunishmentDate { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Status { get; set; } = "Active";
        public string? Remarks { get; set; }
    }

    public class PunishmentSummaryDto
    {
        public int TotalRecords { get; set; }
        public int ActivePunishments { get; set; }
        public int Warnings { get; set; }
        public int Fines { get; set; }
        public int Suspensions { get; set; }
        public decimal TotalFineAmount { get; set; }
    }

    public class PunishmentResponseDto
    {
        public PunishmentSummaryDto Summary { get; set; } = new();
        public List<EmployeePunishmentDto> Records { get; set; } = new();
    }
}
