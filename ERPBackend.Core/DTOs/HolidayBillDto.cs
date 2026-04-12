using System;
using System.Collections.Generic;

namespace ERPBackend.Core.DTOs
{
    public class HolidayBillDto
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

    public class HolidayBillSummaryDto
    {
        public decimal TotalAmount { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalRecords { get; set; }
    }

    public class HolidayBillResponseDto
    {
        public HolidayBillSummaryDto Summary { get; set; } = new();
        public List<HolidayBillDto> Records { get; set; } = new();
    }
}
