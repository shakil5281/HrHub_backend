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
        }
    }
}
