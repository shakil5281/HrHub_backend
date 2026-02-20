using System;
using System.Collections.Generic;
using ERPBackend.Infrastructure.Models.Generated;
using Microsoft.EntityFrameworkCore;

namespace ERPBackend.Infrastructure.Data;

public partial class ManagerContext : DbContext
{
    public ManagerContext(DbContextOptions<ManagerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AdvanceSalary> AdvanceSalaries { get; set; }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<AttendanceLog> AttendanceLogs { get; set; }

    public virtual DbSet<Bonuse> Bonuses { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<CounselingRecord> CounselingRecords { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<DailySalarySheet> DailySalarySheets { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Designation> Designations { get; set; }

    public virtual DbSet<District> Districts { get; set; }

    public virtual DbSet<Division> Divisions { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmployeeShiftRoster> EmployeeShiftRosters { get; set; }

    public virtual DbSet<Floor> Floors { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<LeaveApplication> LeaveApplications { get; set; }

    public virtual DbSet<LeaveType> LeaveTypes { get; set; }

    public virtual DbSet<Line> Lines { get; set; }

    public virtual DbSet<ManpowerRequirement> ManpowerRequirements { get; set; }

    public virtual DbSet<MonthlySalarySheet> MonthlySalarySheets { get; set; }

    public virtual DbSet<OtDeduction> OtDeductions { get; set; }

    public virtual DbSet<PostOffice> PostOffices { get; set; }

    public virtual DbSet<SalaryIncrement> SalaryIncrements { get; set; }

    public virtual DbSet<Section> Sections { get; set; }

    public virtual DbSet<Separation> Separations { get; set; }

    public virtual DbSet<Shift> Shifts { get; set; }

    public virtual DbSet<Thana> Thanas { get; set; }

    public virtual DbSet<Transfer> Transfers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdvanceSalary>(entity =>
        {
            entity.HasIndex(e => e.CompanyId, "IX_AdvanceSalaries_CompanyId");

            entity.HasIndex(e => e.EmployeeId, "IX_AdvanceSalaries_EmployeeId");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Company).WithMany(p => p.AdvanceSalaries).HasForeignKey(d => d.CompanyId);

            entity.HasOne(d => d.Employee).WithMany(p => p.AdvanceSalaries).HasForeignKey(d => d.EmployeeId);
        });

        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasIndex(e => e.CompanyId, "IX_Attendances_CompanyId");

            entity.HasIndex(e => e.EmployeeCard, "IX_Attendances_EmployeeCard");

            entity.Property(e => e.EmployeeId).HasMaxLength(50);
            entity.Property(e => e.Othours)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("OTHours");
            entity.Property(e => e.Reason).HasMaxLength(100);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Company).WithMany(p => p.Attendances).HasForeignKey(d => d.CompanyId);

            entity.HasOne(d => d.EmployeeCardNavigation).WithMany(p => p.Attendances).HasForeignKey(d => d.EmployeeCard);
        });

        modelBuilder.Entity<AttendanceLog>(entity =>
        {
            entity.HasIndex(e => e.CompanyId, "IX_AttendanceLogs_CompanyId");

            entity.HasIndex(e => e.EmployeeCard, "IX_AttendanceLogs_EmployeeCard");

            entity.Property(e => e.DeviceId).HasMaxLength(50);
            entity.Property(e => e.EmployeeId).HasMaxLength(50);
            entity.Property(e => e.VerificationMode).HasMaxLength(50);

            entity.HasOne(d => d.Company).WithMany(p => p.AttendanceLogs).HasForeignKey(d => d.CompanyId);

            entity.HasOne(d => d.EmployeeCardNavigation).WithMany(p => p.AttendanceLogs).HasForeignKey(d => d.EmployeeCard);
        });

        modelBuilder.Entity<Bonuse>(entity =>
        {
            entity.HasIndex(e => e.CompanyId, "IX_Bonuses_CompanyId");

            entity.HasIndex(e => e.EmployeeId, "IX_Bonuses_EmployeeId");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BonusType).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Company).WithMany(p => p.Bonuses).HasForeignKey(d => d.CompanyId);

            entity.HasOne(d => d.Employee).WithMany(p => p.Bonuses).HasForeignKey(d => d.EmployeeId);
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasIndex(e => e.RegistrationNo, "IX_Companies_RegistrationNo").IsUnique();

            entity.Property(e => e.AddressBn)
                .HasMaxLength(500)
                .HasDefaultValue("");
            entity.Property(e => e.AddressEn)
                .HasMaxLength(500)
                .HasDefaultValue("");
            entity.Property(e => e.CompanyNameBn)
                .HasMaxLength(200)
                .HasDefaultValue("");
            entity.Property(e => e.CompanyNameEn).HasMaxLength(200);
            entity.Property(e => e.Industry).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .HasDefaultValue("");
            entity.Property(e => e.RegistrationNo).HasMaxLength(50);

            entity.HasMany(d => d.Users).WithMany(p => p.AssignedCompanies)
                .UsingEntity<Dictionary<string, object>>(
                    "ApplicationUserCompany",
                    r => r.HasOne<AspNetUser>().WithMany().HasForeignKey("UsersId"),
                    l => l.HasOne<Company>().WithMany().HasForeignKey("AssignedCompaniesId"),
                    j =>
                    {
                        j.HasKey("AssignedCompaniesId", "UsersId");
                        j.ToTable("ApplicationUserCompany");
                        j.HasIndex(new[] { "UsersId" }, "IX_ApplicationUserCompany_UsersId");
                    });
        });

        modelBuilder.Entity<CounselingRecord>(entity =>
        {
            entity.HasIndex(e => e.EmployeeId, "IX_CounselingRecords_EmployeeId");

            entity.Property(e => e.ActionTaken).HasMaxLength(1000);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.FollowUpNotes).HasMaxLength(500);
            entity.Property(e => e.IssueType).HasMaxLength(100);
            entity.Property(e => e.Severity).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Employee).WithMany(p => p.CounselingRecords).HasForeignKey(d => d.EmployeeId);
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.Property(e => e.NameBn).HasMaxLength(100);
            entity.Property(e => e.NameEn).HasMaxLength(100);
        });

        modelBuilder.Entity<DailySalarySheet>(entity =>
        {
            entity.HasIndex(e => e.CompanyId, "IX_DailySalarySheets_CompanyId");

            entity.HasIndex(e => e.EmployeeId, "IX_DailySalarySheets_EmployeeId");

            entity.Property(e => e.AttendanceStatus).HasMaxLength(20);
            entity.Property(e => e.Deduction).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.GrossSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NetPayable).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Otamount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("OTAmount");
            entity.Property(e => e.Othours)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("OTHours");
            entity.Property(e => e.PerDaySalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalEarning).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Company).WithMany(p => p.DailySalarySheets).HasForeignKey(d => d.CompanyId);

            entity.HasOne(d => d.Employee).WithMany(p => p.DailySalarySheets).HasForeignKey(d => d.EmployeeId);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasIndex(e => e.CompanyId, "IX_Departments_CompanyId");

            entity.Property(e => e.NameBn).HasMaxLength(100);
            entity.Property(e => e.NameEn).HasMaxLength(100);

            entity.HasOne(d => d.Company).WithMany(p => p.Departments).HasForeignKey(d => d.CompanyId);
        });

        modelBuilder.Entity<Designation>(entity =>
        {
            entity.HasIndex(e => e.CompanyId, "IX_Designations_CompanyId");

            entity.HasIndex(e => e.DepartmentId, "IX_Designations_DepartmentId");

            entity.HasIndex(e => e.SectionId, "IX_Designations_SectionId");

            entity.Property(e => e.AttendanceBonus).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.HolidayBill).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NameBn).HasMaxLength(100);
            entity.Property(e => e.NameEn).HasMaxLength(100);
            entity.Property(e => e.NightBill).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Company).WithMany(p => p.Designations).HasForeignKey(d => d.CompanyId);

            entity.HasOne(d => d.Department).WithMany(p => p.Designations).HasForeignKey(d => d.DepartmentId);

            entity.HasOne(d => d.Section).WithMany(p => p.Designations)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<District>(entity =>
        {
            entity.HasIndex(e => e.DivisionId, "IX_Districts_DivisionId");

            entity.Property(e => e.NameBn).HasMaxLength(100);
            entity.Property(e => e.NameEn).HasMaxLength(100);

            entity.HasOne(d => d.Division).WithMany(p => p.Districts).HasForeignKey(d => d.DivisionId);
        });

        modelBuilder.Entity<Division>(entity =>
        {
            entity.HasIndex(e => e.CountryId, "IX_Divisions_CountryId");

            entity.Property(e => e.NameBn).HasMaxLength(100);
            entity.Property(e => e.NameEn).HasMaxLength(100);

            entity.HasOne(d => d.Country).WithMany(p => p.Divisions).HasForeignKey(d => d.CountryId);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasIndex(e => e.CompanyId, "IX_Employees_CompanyId");

            entity.HasIndex(e => e.DepartmentId, "IX_Employees_DepartmentId");

            entity.HasIndex(e => e.DesignationId, "IX_Employees_DesignationId");

            entity.HasIndex(e => new { e.EmployeeId, e.CompanyName }, "IX_Employees_EmployeeId_CompanyName")
                .IsUnique()
                .HasFilter("([CompanyName] IS NOT NULL)");

            entity.HasIndex(e => e.FloorId, "IX_Employees_FloorId");

            entity.HasIndex(e => e.GroupId, "IX_Employees_GroupId");

            entity.HasIndex(e => e.LineId, "IX_Employees_LineId");

            entity.HasIndex(e => new { e.Proximity, e.CompanyName }, "IX_Employees_Proximity_CompanyName")
                .IsUnique()
                .HasFilter("([Proximity] IS NOT NULL AND [CompanyName] IS NOT NULL)");

            entity.HasIndex(e => e.SectionId, "IX_Employees_SectionId");

            entity.HasIndex(e => e.ShiftId, "IX_Employees_ShiftId");

            entity.HasIndex(e => e.UserId, "IX_Employees_UserId");

            entity.Property(e => e.BankAccountNo).HasMaxLength(50);
            entity.Property(e => e.BankAccountType).HasMaxLength(50);
            entity.Property(e => e.BankBranchName).HasMaxLength(100);
            entity.Property(e => e.BankName).HasMaxLength(100);
            entity.Property(e => e.BankRoutingNo).HasMaxLength(50);
            entity.Property(e => e.BasicSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BloodGroup).HasMaxLength(20);
            entity.Property(e => e.CompanyName).HasMaxLength(200);
            entity.Property(e => e.Conveyance).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.EmergencyContactAddress).HasMaxLength(500);
            entity.Property(e => e.EmergencyContactName).HasMaxLength(200);
            entity.Property(e => e.EmergencyContactPhone).HasMaxLength(20);
            entity.Property(e => e.EmergencyContactRelation).HasMaxLength(100);
            entity.Property(e => e.EmployeeId).HasMaxLength(20);
            entity.Property(e => e.FatherNameBn).HasMaxLength(200);
            entity.Property(e => e.FatherNameEn).HasMaxLength(200);
            entity.Property(e => e.FoodAllowance).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.FullNameBn).HasMaxLength(200);
            entity.Property(e => e.FullNameEn).HasMaxLength(200);
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.GrossSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.HouseRent).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.IsOtEnabled)
                .IsRequired()
                .HasDefaultValueSql("(CONVERT([bit],(0),0))");
            entity.Property(e => e.MaritalStatus).HasMaxLength(50);
            entity.Property(e => e.MedicalAllowance).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MotherNameBn).HasMaxLength(200);
            entity.Property(e => e.MotherNameEn).HasMaxLength(200);
            entity.Property(e => e.Nid).HasMaxLength(50);
            entity.Property(e => e.OtherAllowance).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PermanentAddress).HasMaxLength(500);
            entity.Property(e => e.PermanentAddressBn).HasMaxLength(500);
            entity.Property(e => e.PermanentPostalCode).HasMaxLength(20);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.PresentAddress).HasMaxLength(500);
            entity.Property(e => e.PresentAddressBn).HasMaxLength(500);
            entity.Property(e => e.PresentPostalCode).HasMaxLength(20);
            entity.Property(e => e.ProfileImageUrl).HasMaxLength(500);
            entity.Property(e => e.Proximity).HasMaxLength(50);
            entity.Property(e => e.Religion).HasMaxLength(50);
            entity.Property(e => e.SignatureImageUrl).HasMaxLength(500);
            entity.Property(e => e.SpouseContact).HasMaxLength(20);
            entity.Property(e => e.SpouseNameBn).HasMaxLength(200);
            entity.Property(e => e.SpouseNameEn).HasMaxLength(200);
            entity.Property(e => e.SpouseOccupation).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Company).WithMany(p => p.Employees).HasForeignKey(d => d.CompanyId);

            entity.HasOne(d => d.Department).WithMany(p => p.Employees)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Designation).WithMany(p => p.Employees)
                .HasForeignKey(d => d.DesignationId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Floor).WithMany(p => p.Employees).HasForeignKey(d => d.FloorId);

            entity.HasOne(d => d.Group).WithMany(p => p.Employees).HasForeignKey(d => d.GroupId);

            entity.HasOne(d => d.Line).WithMany(p => p.Employees).HasForeignKey(d => d.LineId);

            entity.HasOne(d => d.Section).WithMany(p => p.Employees).HasForeignKey(d => d.SectionId);

            entity.HasOne(d => d.Shift).WithMany(p => p.Employees).HasForeignKey(d => d.ShiftId);

            entity.HasOne(d => d.User).WithMany(p => p.Employees).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<EmployeeShiftRoster>(entity =>
        {
            entity.HasIndex(e => e.EmployeeId, "IX_EmployeeShiftRosters_EmployeeId");

            entity.HasIndex(e => e.ShiftId, "IX_EmployeeShiftRosters_ShiftId");

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeShiftRosters).HasForeignKey(d => d.EmployeeId);

            entity.HasOne(d => d.Shift).WithMany(p => p.EmployeeShiftRosters)
                .HasForeignKey(d => d.ShiftId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Floor>(entity =>
        {
            entity.HasIndex(e => e.CompanyId, "IX_Floors_CompanyId");

            entity.HasIndex(e => new { e.NameEn, e.CompanyId }, "IX_Floors_NameEn_CompanyId")
                .IsUnique()
                .HasFilter("([CompanyId] IS NOT NULL)");

            entity.Property(e => e.CompanyName).HasMaxLength(200);
            entity.Property(e => e.NameBn).HasMaxLength(100);
            entity.Property(e => e.NameEn).HasMaxLength(100);

            entity.HasOne(d => d.Company).WithMany(p => p.Floors).HasForeignKey(d => d.CompanyId);
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasIndex(e => e.CompanyId, "IX_Groups_CompanyId");

            entity.HasIndex(e => new { e.NameEn, e.CompanyId }, "IX_Groups_NameEn_CompanyId")
                .IsUnique()
                .HasFilter("([CompanyId] IS NOT NULL)");

            entity.Property(e => e.CompanyName).HasMaxLength(200);
            entity.Property(e => e.NameBn).HasMaxLength(100);
            entity.Property(e => e.NameEn).HasMaxLength(100);

            entity.HasOne(d => d.Company).WithMany(p => p.Groups).HasForeignKey(d => d.CompanyId);
        });

        modelBuilder.Entity<LeaveApplication>(entity =>
        {
            entity.HasIndex(e => e.EmployeeId, "IX_LeaveApplications_EmployeeId");

            entity.HasIndex(e => e.LeaveTypeId, "IX_LeaveApplications_LeaveTypeId");

            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TotalDays).HasColumnType("decimal(18, 1)");

            entity.HasOne(d => d.Employee).WithMany(p => p.LeaveApplications).HasForeignKey(d => d.EmployeeId);

            entity.HasOne(d => d.LeaveType).WithMany(p => p.LeaveApplications)
                .HasForeignKey(d => d.LeaveTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<LeaveType>(entity =>
        {
            entity.Property(e => e.Code).HasMaxLength(10);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Line>(entity =>
        {
            entity.HasIndex(e => e.CompanyId, "IX_Lines_CompanyId");

            entity.HasIndex(e => e.DepartmentId, "IX_Lines_DepartmentId");

            entity.HasIndex(e => e.SectionId, "IX_Lines_SectionId");

            entity.Property(e => e.NameBn).HasMaxLength(100);
            entity.Property(e => e.NameEn).HasMaxLength(100);

            entity.HasOne(d => d.Company).WithMany(p => p.Lines).HasForeignKey(d => d.CompanyId);

            entity.HasOne(d => d.Department).WithMany(p => p.Lines).HasForeignKey(d => d.DepartmentId);

            entity.HasOne(d => d.Section).WithMany(p => p.Lines)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ManpowerRequirement>(entity =>
        {
            entity.HasIndex(e => e.DepartmentId, "IX_ManpowerRequirements_DepartmentId");

            entity.HasIndex(e => e.DesignationId, "IX_ManpowerRequirements_DesignationId");

            entity.Property(e => e.Note).HasMaxLength(500);

            entity.HasOne(d => d.Department).WithMany(p => p.ManpowerRequirements)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Designation).WithMany(p => p.ManpowerRequirements)
                .HasForeignKey(d => d.DesignationId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<MonthlySalarySheet>(entity =>
        {
            entity.HasIndex(e => e.CompanyId, "IX_MonthlySalarySheets_CompanyId");

            entity.HasIndex(e => e.EmployeeId, "IX_MonthlySalarySheets_EmployeeId");

            entity.Property(e => e.AbsentDeduction).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.AdvanceDeduction).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.AttendanceBonus).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BasicSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.GrossSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NetPayable).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Otamount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("OTAmount");
            entity.Property(e => e.Otdeduction)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("OTDeduction");
            entity.Property(e => e.OtherAllowances).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Othours)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("OTHours");
            entity.Property(e => e.Otrate)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("OTRate");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.TotalDeduction).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalEarning).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Company).WithMany(p => p.MonthlySalarySheets).HasForeignKey(d => d.CompanyId);

            entity.HasOne(d => d.Employee).WithMany(p => p.MonthlySalarySheets).HasForeignKey(d => d.EmployeeId);
        });

        modelBuilder.Entity<OtDeduction>(entity =>
        {
            entity.HasIndex(e => e.EmployeeId, "IX_OtDeductions_EmployeeId");

            entity.Property(e => e.DeductionHours).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Reason).HasMaxLength(200);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Employee).WithMany(p => p.OtDeductions).HasForeignKey(d => d.EmployeeId);
        });

        modelBuilder.Entity<PostOffice>(entity =>
        {
            entity.HasIndex(e => e.DistrictId, "IX_PostOffices_DistrictId");

            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.NameBn).HasMaxLength(100);
            entity.Property(e => e.NameEn).HasMaxLength(100);

            entity.HasOne(d => d.District).WithMany(p => p.PostOffices).HasForeignKey(d => d.DistrictId);
        });

        modelBuilder.Entity<SalaryIncrement>(entity =>
        {
            entity.HasIndex(e => e.CompanyId, "IX_SalaryIncrements_CompanyId");

            entity.HasIndex(e => e.EmployeeId, "IX_SalaryIncrements_EmployeeId");

            entity.Property(e => e.IncrementAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.IncrementType).HasMaxLength(100);
            entity.Property(e => e.NewGrossSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PreviousGrossSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Remarks).HasMaxLength(500);

            entity.HasOne(d => d.Company).WithMany(p => p.SalaryIncrements).HasForeignKey(d => d.CompanyId);

            entity.HasOne(d => d.Employee).WithMany(p => p.SalaryIncrements).HasForeignKey(d => d.EmployeeId);
        });

        modelBuilder.Entity<Section>(entity =>
        {
            entity.HasIndex(e => e.CompanyId, "IX_Sections_CompanyId");

            entity.HasIndex(e => e.DepartmentId, "IX_Sections_DepartmentId");

            entity.Property(e => e.NameBn).HasMaxLength(100);
            entity.Property(e => e.NameEn).HasMaxLength(100);

            entity.HasOne(d => d.Company).WithMany(p => p.Sections).HasForeignKey(d => d.CompanyId);

            entity.HasOne(d => d.Department).WithMany(p => p.Sections)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Separation>(entity =>
        {
            entity.HasIndex(e => e.EmployeeId, "IX_Separations_EmployeeId");

            entity.Property(e => e.AdminRemark).HasMaxLength(500);
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Type).HasMaxLength(50);

            entity.HasOne(d => d.Employee).WithMany(p => p.Separations).HasForeignKey(d => d.EmployeeId);
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.HasIndex(e => e.CompanyId, "IX_Shifts_CompanyId");

            entity.HasIndex(e => new { e.NameEn, e.CompanyId }, "IX_Shifts_NameEn_CompanyId")
                .IsUnique()
                .HasFilter("([CompanyId] IS NOT NULL)");

            entity.Property(e => e.ActualInTime).HasMaxLength(10);
            entity.Property(e => e.ActualOutTime).HasMaxLength(10);
            entity.Property(e => e.CompanyName).HasMaxLength(200);
            entity.Property(e => e.InTime)
                .HasMaxLength(10)
                .HasDefaultValue("");
            entity.Property(e => e.LateInTime).HasMaxLength(10);
            entity.Property(e => e.LunchHour).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.LunchTimeStart).HasMaxLength(10);
            entity.Property(e => e.NameBn).HasMaxLength(100);
            entity.Property(e => e.NameEn).HasMaxLength(100);
            entity.Property(e => e.OutTime)
                .HasMaxLength(10)
                .HasDefaultValue("");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("");
            entity.Property(e => e.Weekends).HasMaxLength(200);

            entity.HasOne(d => d.Company).WithMany(p => p.Shifts).HasForeignKey(d => d.CompanyId);
        });

        modelBuilder.Entity<Thana>(entity =>
        {
            entity.HasIndex(e => e.DistrictId, "IX_Thanas_DistrictId");

            entity.Property(e => e.NameBn).HasMaxLength(100);
            entity.Property(e => e.NameEn).HasMaxLength(100);

            entity.HasOne(d => d.District).WithMany(p => p.Thanas).HasForeignKey(d => d.DistrictId);
        });

        modelBuilder.Entity<Transfer>(entity =>
        {
            entity.HasIndex(e => e.EmployeeId, "IX_Transfers_EmployeeId");

            entity.HasIndex(e => e.FromDepartmentId, "IX_Transfers_FromDepartmentId");

            entity.HasIndex(e => e.FromDesignationId, "IX_Transfers_FromDesignationId");

            entity.HasIndex(e => e.ToDepartmentId, "IX_Transfers_ToDepartmentId");

            entity.HasIndex(e => e.ToDesignationId, "IX_Transfers_ToDesignationId");

            entity.Property(e => e.AdminRemark).HasMaxLength(500);
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Employee).WithMany(p => p.Transfers).HasForeignKey(d => d.EmployeeId);

            entity.HasOne(d => d.FromDepartment).WithMany(p => p.TransferFromDepartments).HasForeignKey(d => d.FromDepartmentId);

            entity.HasOne(d => d.FromDesignation).WithMany(p => p.TransferFromDesignations).HasForeignKey(d => d.FromDesignationId);

            entity.HasOne(d => d.ToDepartment).WithMany(p => p.TransferToDepartments)
                .HasForeignKey(d => d.ToDepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.ToDesignation).WithMany(p => p.TransferToDesignations)
                .HasForeignKey(d => d.ToDesignationId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
