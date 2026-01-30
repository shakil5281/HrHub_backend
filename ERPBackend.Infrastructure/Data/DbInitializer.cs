using Microsoft.EntityFrameworkCore;
using ERPBackend.Core.Models;

namespace ERPBackend.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        // Ensure database is created and migrations are applied
        await context.Database.MigrateAsync();

        // Seed Departments if empty
        if (!await context.Departments.AnyAsync())
        {
            var departments = new List<Department>
            {
                new Department { DepartmentCode = "HR", DepartmentName = "Human Resources", Description = "HR Department", Location = "Building A", IsActive = true },
                new Department { DepartmentCode = "IT", DepartmentName = "Information Technology", Description = "IT Department", Location = "Building B", IsActive = true },
                new Department { DepartmentCode = "FIN", DepartmentName = "Finance", Description = "Finance Department", Location = "Building A", IsActive = true },
                new Department { DepartmentCode = "MKT", DepartmentName = "Marketing", Description = "Marketing Department", Location = "Building C", IsActive = true }
            };

            await context.Departments.AddRangeAsync(departments);
            await context.SaveChangesAsync();

            // Seed Employees if empty
            if (!await context.Employees.AnyAsync())
            {
                var hrDept = departments.First(d => d.DepartmentCode == "HR");
                var itDept = departments.First(d => d.DepartmentCode == "IT");

                var employees = new List<Employee>
                {
                    new Employee { 
                        EmployeeCode = "EMP001", 
                        FirstName = "John", 
                        LastName = "Doe", 
                        Email = "john.doe@example.com", 
                        Position = "HR Manager", 
                        Salary = 5000, 
                        DepartmentId = hrDept.Id,
                        IsActive = true,
                        HireDate = DateTime.UtcNow.AddYears(-2)
                    },
                    new Employee { 
                        EmployeeCode = "EMP002", 
                        FirstName = "Jane", 
                        LastName = "Smith", 
                        Email = "jane.smith@example.com", 
                        Position = "Senior Developer", 
                        Salary = 6000, 
                        DepartmentId = itDept.Id,
                        IsActive = true,
                        HireDate = DateTime.UtcNow.AddYears(-1)
                    }
                };

                await context.Employees.AddRangeAsync(employees);
                await context.SaveChangesAsync();
            }
        }
    }
}
