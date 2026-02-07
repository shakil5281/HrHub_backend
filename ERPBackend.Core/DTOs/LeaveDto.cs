namespace ERPBackend.Core.DTOs
{
    public class LeaveTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int YearlyLimit { get; set; }
        public bool IsCarryForward { get; set; }
        public string? Description { get; set; }
    }

    public class LeaveApplicationDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeIdCard { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public int LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalDays { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime AppliedDate { get; set; }
        public string? Remarks { get; set; }
        public string? AttachmentUrl { get; set; }
    }

    public class CreateLeaveApplicationDto
    {
        public int EmployeeId { get; set; }
        public int LeaveTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }
    }

    public class UpdateLeaveApplicationDto
    {
        public int Id { get; set; }
        public int LeaveTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }
    }

    public class LeaveActionDto
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty; // Approved, Rejected
        public string? Remarks { get; set; }
    }

    public class LeaveBalanceDto
    {
        public int LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public int TotalAllocated { get; set; }
        public decimal TotalTaken { get; set; }
        public decimal Balance { get; set; }
    }
}
