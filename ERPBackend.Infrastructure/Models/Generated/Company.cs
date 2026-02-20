using System;
using System.Collections.Generic;

namespace ERPBackend.Infrastructure.Models.Generated;

public partial class Company
{
    public int Id { get; set; }

    public string CompanyNameEn { get; set; } = null!;

    public string RegistrationNo { get; set; } = null!;

    public string Industry { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int Founded { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string AddressEn { get; set; } = null!;

    public string? AuthorizeSignaturePath { get; set; }

    public string CompanyNameBn { get; set; } = null!;

    public string? LogoPath { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public int Branch { get; set; }

    public string AddressBn { get; set; } = null!;

    public virtual ICollection<AdvanceSalary> AdvanceSalaries { get; set; } = new List<AdvanceSalary>();

    public virtual ICollection<AttendanceLog> AttendanceLogs { get; set; } = new List<AttendanceLog>();

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual ICollection<Bonuse> Bonuses { get; set; } = new List<Bonuse>();

    public virtual ICollection<DailySalarySheet> DailySalarySheets { get; set; } = new List<DailySalarySheet>();

    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();

    public virtual ICollection<Designation> Designations { get; set; } = new List<Designation>();

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<Floor> Floors { get; set; } = new List<Floor>();

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    public virtual ICollection<Line> Lines { get; set; } = new List<Line>();

    public virtual ICollection<MonthlySalarySheet> MonthlySalarySheets { get; set; } = new List<MonthlySalarySheet>();

    public virtual ICollection<SalaryIncrement> SalaryIncrements { get; set; } = new List<SalaryIncrement>();

    public virtual ICollection<Section> Sections { get; set; } = new List<Section>();

    public virtual ICollection<Shift> Shifts { get; set; } = new List<Shift>();

    public virtual ICollection<AspNetUser> Users { get; set; } = new List<AspNetUser>();
}
