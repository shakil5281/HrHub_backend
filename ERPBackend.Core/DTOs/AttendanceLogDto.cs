namespace ERPBackend.Core.DTOs
{
    public class AttendanceLogDto
    {
        public int Id { get; set; }
        public int EmployeeCard { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public DateTime LogTime { get; set; }
        public string? DeviceId { get; set; }
        public string? VerificationMode { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
