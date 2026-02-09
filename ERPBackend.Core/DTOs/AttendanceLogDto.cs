using System;

namespace ERPBackend.Core.DTOs
{
    public class AttendanceLogDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeIdCard { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public DateTime LogTime { get; set; }
        public string? DeviceId { get; set; }
        public string? VerificationMode { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
