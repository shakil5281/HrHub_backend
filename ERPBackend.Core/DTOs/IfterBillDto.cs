using System;
using System.Collections.Generic;

namespace ERPBackend.Core.DTOs
{
    public class IfterBillDto
    {
        public int Id { get; set; }
        public int EmployeeCard { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? ShiftName { get; set; }
        public string? CompanyName { get; set; }
    }

    public class IfterBillSummaryDto
    {
        public decimal TotalAmount { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalRecords { get; set; }
    }

    public class IfterBillResponseDto
    {
        public IfterBillSummaryDto Summary { get; set; } = new();
        public List<IfterBillDto> Records { get; set; } = new();
    }

    public class IfterBillProcessRequestDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? CompanyId { get; set; }
        public int? DepartmentId { get; set; }
    }
}
