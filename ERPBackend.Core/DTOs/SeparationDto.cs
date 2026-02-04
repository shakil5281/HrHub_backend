using System;

namespace ERPBackend.Core.DTOs
{
    public class SeparationDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string DesignationName { get; set; } = string.Empty;

        public DateTime LastWorkingDate { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public string? AdminRemark { get; set; }
        public bool IsSettled { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class CreateSeparationDto
    {
        public int EmployeeId { get; set; }
        public DateTime LastWorkingDate { get; set; }
        public string Type { get; set; } = "Resignation";
        public string Reason { get; set; } = string.Empty;
    }

    public class UpdateSeparationStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public string? AdminRemark { get; set; }
    }
}
