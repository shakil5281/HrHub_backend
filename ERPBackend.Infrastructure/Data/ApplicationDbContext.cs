using Microsoft.EntityFrameworkCore;
using ERPBackend.Core.Models;

namespace ERPBackend.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<DataImportLog> DataImportLogs { get; set; }
    public DbSet<DataExportLog> DataExportLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Employee configuration
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Salary).HasColumnType("decimal(18,2)");
            entity.HasIndex(e => e.EmployeeCode).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            
            entity.HasOne(e => e.Department)
                  .WithMany(d => d.Employees)
                  .HasForeignKey(e => e.DepartmentId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Department configuration
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.DepartmentCode).IsRequired().HasMaxLength(50);
            entity.Property(d => d.DepartmentName).IsRequired().HasMaxLength(200);
            entity.HasIndex(d => d.DepartmentCode).IsUnique();
        });

        // DataImportLog configuration
        modelBuilder.Entity<DataImportLog>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.FileName).IsRequired().HasMaxLength(500);
            entity.Property(d => d.FileType).IsRequired().HasMaxLength(50);
            entity.Property(d => d.EntityType).IsRequired().HasMaxLength(100);
        });

        // DataExportLog configuration
        modelBuilder.Entity<DataExportLog>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.FileName).IsRequired().HasMaxLength(500);
            entity.Property(d => d.FileType).IsRequired().HasMaxLength(50);
            entity.Property(d => d.EntityType).IsRequired().HasMaxLength(100);
        });
    }
}
