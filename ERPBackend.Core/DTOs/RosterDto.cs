namespace ERPBackend.Core.DTOs
{
    public class RosterDto
    {
        public int Id { get; set; }
        public int EmployeeCard { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
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
        public int EmployeeCard { get; set; }
        public DateTime Date { get; set; }
        public int ShiftId { get; set; }
        public bool IsOffDay { get; set; }
    }

    public class BulkRosterDto
    {
        public List<int> EmployeeCards { get; set; } = new();
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ShiftId { get; set; }
        public bool IsOffDay { get; set; }
    }
}
