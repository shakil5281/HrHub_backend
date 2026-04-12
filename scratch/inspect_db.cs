using Microsoft.EntityFrameworkCore;
using ERPBackend.Infrastructure.Data;
using ERPBackend.Core.Models;
using System;
using System.Linq;

var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseSqlServer("Server=unity3\\SQLEXPRESS;Database=HrHubDb;Trusted_Connection=True;TrustServerCertificate=True")
    .Options;

using var context = new ApplicationDbContext(options);

var date = new DateTime(2026, 4, 11);
Console.WriteLine($"Checking Attendance for Date: {date:yyyy-MM-dd}");

var attendances = context.Attendances
    .Where(a => a.Date >= date && a.Date < date.AddDays(1))
    .Take(10)
    .ToList();

if (!attendances.Any()) {
    Console.WriteLine("No attendance records found for this date.");
} else {
    foreach (var a in attendances) {
        Console.WriteLine($"ID={a.Id}, Card={a.EmployeeCard}, EmpCode={a.EmployeeId}, In={a.InTime}, Out={a.OutTime}");
    }
}

var b1090 = context.Employees.FirstOrDefault(e => e.EmployeeId == "1090");
if (b1090 != null) {
    Console.WriteLine($"Employee 1090 PK (Id): {b1090.Id}");
}
