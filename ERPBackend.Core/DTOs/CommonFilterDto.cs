using System;

namespace ERPBackend.Core.DTOs
{
    public class CommonFilterDto
    {
        public DateTime? Date { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public int? DepartmentId { get; set; }
        public int? SectionId { get; set; }
        public int? DesignationId { get; set; }
        public int? LineId { get; set; }
        public int? GroupId { get; set; }
        public int? ShiftId { get; set; }
        public int? FloorId { get; set; }
        public string? Gender { get; set; }
        public string? Religion { get; set; }
        public string? Status { get; set; }
        public bool? IsActive { get; set; }
        public string? EmployeeId { get; set; }
        public int? EmployeeCard { get; set; }
        public string? SearchTerm { get; set; }
        public DateTime? JoinDateFrom { get; set; }
        public DateTime? JoinDateTo { get; set; }
    }
}
