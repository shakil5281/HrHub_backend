namespace ERPBackend.Core.DTOs
{
    public class RosterDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeIdCard { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string DesignationName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int ShiftId { get; set; }
        public string ShiftName { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public bool IsOffDay { get; set; }
    }

    public class CreateRosterDto
    {
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public int ShiftId { get; set; }
        public bool IsOffDay { get; set; }
    }
    
    public class BulkRosterDto
    {
        public List<int> EmployeeIds { get; set; } = new();
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ShiftId { get; set; }
        public bool IsOffDay { get; set; }
    }
}
