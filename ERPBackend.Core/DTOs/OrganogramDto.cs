using System.ComponentModel.DataAnnotations;

namespace ERPBackend.Core.DTOs
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string? NameBn { get; set; }
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
    }

    public class CreateDepartmentDto
    {
        [Required] public string NameEn { get; set; } = string.Empty;
        public string? NameBn { get; set; }
        public int CompanyId { get; set; }
    }

    public class SectionDto
    {
        public int Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string? NameBn { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
    }

    public class CreateSectionDto
    {
        [Required] public string NameEn { get; set; } = string.Empty;
        public string? NameBn { get; set; }
        public int? CompanyId { get; set; }
        public int DepartmentId { get; set; }
    }

    public class DesignationDto
    {
        public int Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string? NameBn { get; set; }
        public decimal NightBill { get; set; }
        public decimal HolidayBill { get; set; }
        public decimal AttendanceBonus { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int SectionId { get; set; }
        public string? SectionName { get; set; }
    }

    public class CreateDesignationDto
    {
        [Required] public string NameEn { get; set; } = string.Empty;
        public string? NameBn { get; set; }
        public decimal NightBill { get; set; }
        public decimal HolidayBill { get; set; }
        public decimal AttendanceBonus { get; set; }
        public int? CompanyId { get; set; }
        public int? DepartmentId { get; set; }
        public int SectionId { get; set; }
    }

    public class LineDto
    {
        public int Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string? NameBn { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int SectionId { get; set; }
        public string? SectionName { get; set; }
    }

    public class CreateLineDto
    {
        [Required] public string NameEn { get; set; } = string.Empty;
        public string? NameBn { get; set; }
        public int? CompanyId { get; set; }
        public int? DepartmentId { get; set; }
        public int SectionId { get; set; }
    }

    public class ShiftDto
    {
        public int Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string? NameBn { get; set; }
        public string InTime { get; set; } = string.Empty;
        public string OutTime { get; set; } = string.Empty;
        public string? ActualInTime { get; set; }
        public string? ActualOutTime { get; set; }
        public string? LateInTime { get; set; }
        public string? LunchTimeStart { get; set; }
        public decimal LunchHour { get; set; }
        public string? Weekends { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string Status { get; set; } = "Active";
    }

    public class CreateShiftDto
    {
        [Required] public string NameEn { get; set; } = string.Empty;
        public string? NameBn { get; set; }
        [Required] public string InTime { get; set; } = string.Empty;
        [Required] public string OutTime { get; set; } = string.Empty;
        public string? ActualInTime { get; set; }
        public string? ActualOutTime { get; set; }
        public string? LateInTime { get; set; }
        public string? LunchTimeStart { get; set; }
        public decimal LunchHour { get; set; } = 1.0m;
        public string? Weekends { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        [Required] public string Status { get; set; } = "Active";
    }

    public class GroupDto
    {
        public int Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string? NameBn { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
    }

    public class CreateGroupDto
    {
        [Required] public string NameEn { get; set; } = string.Empty;
        public string? NameBn { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
    }

    public class FloorDto
    {
        public int Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string? NameBn { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
    }

    public class CreateFloorDto
    {
        [Required] public string NameEn { get; set; } = string.Empty;
        public string? NameBn { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
    }
}
