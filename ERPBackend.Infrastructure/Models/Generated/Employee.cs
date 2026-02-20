using System;
using System.Collections.Generic;

namespace ERPBackend.Infrastructure.Models.Generated;

public partial class Employee
{
    public int Id { get; set; }

    public string EmployeeId { get; set; } = null!;

    public string FullNameEn { get; set; } = null!;

    public string? FullNameBn { get; set; }

    public string? Nid { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public int DepartmentId { get; set; }

    public int? SectionId { get; set; }

    public int DesignationId { get; set; }

    public int? LineId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime JoinDate { get; set; }

    public string? ProfileImageUrl { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? PresentAddress { get; set; }

    public string? PermanentAddress { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? BankAccountNo { get; set; }

    public string? BankAccountType { get; set; }

    public string? BankBranchName { get; set; }

    public string? BankName { get; set; }

    public string? BankRoutingNo { get; set; }

    public decimal? BasicSalary { get; set; }

    public decimal? Conveyance { get; set; }

    public string? EmergencyContactAddress { get; set; }

    public string? EmergencyContactName { get; set; }

    public string? EmergencyContactPhone { get; set; }

    public string? EmergencyContactRelation { get; set; }

    public string? FatherNameBn { get; set; }

    public string? FatherNameEn { get; set; }

    public decimal? FoodAllowance { get; set; }

    public string? Gender { get; set; }

    public decimal? GrossSalary { get; set; }

    public decimal? HouseRent { get; set; }

    public string? MaritalStatus { get; set; }

    public decimal? MedicalAllowance { get; set; }

    public string? MotherNameBn { get; set; }

    public string? MotherNameEn { get; set; }

    public decimal? OtherAllowance { get; set; }

    public string? Proximity { get; set; }

    public string? Religion { get; set; }

    public string? SignatureImageUrl { get; set; }

    public string? SpouseContact { get; set; }

    public string? SpouseNameBn { get; set; }

    public string? SpouseNameEn { get; set; }

    public string? SpouseOccupation { get; set; }

    public string? UserId { get; set; }

    public string? PermanentAddressBn { get; set; }

    public int? PermanentDistrictId { get; set; }

    public int? PermanentDivisionId { get; set; }

    public int? PermanentPostOfficeId { get; set; }

    public string? PermanentPostalCode { get; set; }

    public int? PermanentThanaId { get; set; }

    public string? PresentAddressBn { get; set; }

    public int? PresentDistrictId { get; set; }

    public int? PresentDivisionId { get; set; }

    public int? PresentPostOfficeId { get; set; }

    public string? PresentPostalCode { get; set; }

    public int? PresentThanaId { get; set; }

    public int? FloorId { get; set; }

    public int? GroupId { get; set; }

    public bool? IsOtEnabled { get; set; }

    public int? ShiftId { get; set; }

    public string? BloodGroup { get; set; }

    public string? CompanyName { get; set; }

    public int? CompanyId { get; set; }

    public virtual ICollection<AdvanceSalary> AdvanceSalaries { get; set; } = new List<AdvanceSalary>();

    public virtual ICollection<AttendanceLog> AttendanceLogs { get; set; } = new List<AttendanceLog>();

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual ICollection<Bonuse> Bonuses { get; set; } = new List<Bonuse>();

    public virtual Company? Company { get; set; }

    public virtual ICollection<CounselingRecord> CounselingRecords { get; set; } = new List<CounselingRecord>();

    public virtual ICollection<DailySalarySheet> DailySalarySheets { get; set; } = new List<DailySalarySheet>();

    public virtual Department Department { get; set; } = null!;

    public virtual Designation Designation { get; set; } = null!;

    public virtual ICollection<EmployeeShiftRoster> EmployeeShiftRosters { get; set; } = new List<EmployeeShiftRoster>();

    public virtual Floor? Floor { get; set; }

    public virtual Group? Group { get; set; }

    public virtual ICollection<LeaveApplication> LeaveApplications { get; set; } = new List<LeaveApplication>();

    public virtual Line? Line { get; set; }

    public virtual ICollection<MonthlySalarySheet> MonthlySalarySheets { get; set; } = new List<MonthlySalarySheet>();

    public virtual ICollection<OtDeduction> OtDeductions { get; set; } = new List<OtDeduction>();

    public virtual ICollection<SalaryIncrement> SalaryIncrements { get; set; } = new List<SalaryIncrement>();

    public virtual Section? Section { get; set; }

    public virtual ICollection<Separation> Separations { get; set; } = new List<Separation>();

    public virtual Shift? Shift { get; set; }

    public virtual ICollection<Transfer> Transfers { get; set; } = new List<Transfer>();

    public virtual AspNetUser? User { get; set; }
}
