using ERPBackend.Core.Models;
using ERPBackend.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ERPBackend.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Company> Companies { get; set; } = null!;
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<Section> Sections { get; set; } = null!;
        public DbSet<Designation> Designations { get; set; } = null!;
        public DbSet<Line> Lines { get; set; } = null!;
        public DbSet<Country> Countries { get; set; } = null!;
        public DbSet<Division> Divisions { get; set; } = null!;
        public DbSet<District> Districts { get; set; } = null!;
        public DbSet<Thana> Thanas { get; set; } = null!;
        public DbSet<PostOffice> PostOffices { get; set; } = null!;
        public DbSet<Shift> Shifts { get; set; } = null!;
        public DbSet<Group> Groups { get; set; } = null!;
        public DbSet<Floor> Floors { get; set; } = null!;
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<EmployeeShiftRoster> EmployeeShiftRosters { get; set; } = null!;
        public DbSet<ManpowerRequirement> ManpowerRequirements { get; set; } = null!;
        public DbSet<Attendance> Attendances { get; set; } = null!;
        public DbSet<CounselingRecord> CounselingRecords { get; set; } = null!;
        public DbSet<OTDeduction> OTDeductions { get; set; } = null!;
        public DbSet<MonthlySalarySheet> MonthlySalarySheets { get; set; } = null!;
        public DbSet<DailySalarySheet> DailySalarySheets { get; set; } = null!;
        public DbSet<AdvanceSalary> AdvanceSalaries { get; set; } = null!;
        public DbSet<SalaryIncrement> SalaryIncrements { get; set; } = null!;
        public DbSet<Bonus> Bonuses { get; set; } = null!;
        public DbSet<LeaveType> LeaveTypes { get; set; } = null!;
        public DbSet<LeaveApplication> LeaveApplications { get; set; } = null!;
        public DbSet<Transfer> Transfers { get; set; } = null!;
        public DbSet<Separation> Separations { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Explicitly map Identity tables if needed or leave defaults (AspNetUsers etc)

            builder.Entity<Company>()
                .HasIndex(c => c.RegistrationNo)
                .IsUnique();

            // Configure Employee relationships to prevent cascade delete conflicts
            builder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany()
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Employee>()
                .HasOne(e => e.Section)
                .WithMany()
                .HasForeignKey(e => e.SectionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Employee>()
                .HasOne(e => e.Designation)
                .WithMany()
                .HasForeignKey(e => e.DesignationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Employee>()
                .HasOne(e => e.Line)
                .WithMany()
                .HasForeignKey(e => e.LineId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Employee>()
                .HasOne(e => e.Shift)
                .WithMany()
                .HasForeignKey(e => e.ShiftId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Employee>()
                .HasOne(e => e.Group)
                .WithMany()
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Employee>()
                .HasOne(e => e.Floor)
                .WithMany()
                .HasForeignKey(e => e.FloorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Employee unique indexes
            builder.Entity<Employee>()
                .HasIndex(e => e.EmployeeId)
                .IsUnique();

            builder.Entity<Employee>()
                .HasIndex(e => e.Proximity)
                .IsUnique()
                .HasFilter("[Proximity] IS NOT NULL");

            // Optional relation to ApplicationUser
            builder.Entity<Employee>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Roster Relationships
            builder.Entity<EmployeeShiftRoster>()
                .HasOne(r => r.Employee)
                .WithMany()
                .HasForeignKey(r => r.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<EmployeeShiftRoster>()
                .HasOne(r => r.Shift)
                .WithMany()
                .HasForeignKey(r => r.ShiftId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Attendance>()
                .HasOne(a => a.Employee)
                .WithMany()
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // ManpowerRequirement Relationships
            builder.Entity<ManpowerRequirement>()
                .HasOne(m => m.Department)
                .WithMany()
                .HasForeignKey(m => m.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ManpowerRequirement>()
                .HasOne(m => m.Designation)
                .WithMany()
                .HasForeignKey(m => m.DesignationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CounselingRecord>()
                .HasOne(c => c.Employee)
                .WithMany()
                .HasForeignKey(c => c.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OTDeduction>()
                .HasOne(o => o.Employee)
                .WithMany()
                .HasForeignKey(o => o.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MonthlySalarySheet>()
                .HasOne(s => s.Employee)
                .WithMany()
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DailySalarySheet>()
                .HasOne(s => s.Employee)
                .WithMany()
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AdvanceSalary>()
                .HasOne(a => a.Employee)
                .WithMany()
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SalaryIncrement>()
                .HasOne(i => i.Employee)
                .WithMany()
                .HasForeignKey(i => i.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Bonus>()
                .HasOne(b => b.Employee)
                .WithMany()
                .HasForeignKey(b => b.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<LeaveApplication>()
                .HasOne(l => l.Employee)
                .WithMany()
                .HasForeignKey(l => l.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<LeaveApplication>()
                .HasOne(l => l.LeaveType)
                .WithMany()
                .HasForeignKey(l => l.LeaveTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Transfer Configurations
            builder.Entity<Transfer>()
                .HasOne(t => t.Employee)
                .WithMany()
                .HasForeignKey(t => t.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Transfer>()
                .HasOne(t => t.FromDepartment)
                .WithMany()
                .HasForeignKey(t => t.FromDepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Transfer>()
                .HasOne(t => t.FromDesignation)
                .WithMany()
                .HasForeignKey(t => t.FromDesignationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Transfer>()
                .HasOne(t => t.ToDepartment)
                .WithMany()
                .HasForeignKey(t => t.ToDepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Transfer>()
                .HasOne(t => t.ToDesignation)
                .WithMany()
                .HasForeignKey(t => t.ToDesignationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Separation Configurations
            builder.Entity<Separation>()
                .HasOne(s => s.Employee)
                .WithMany()
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
