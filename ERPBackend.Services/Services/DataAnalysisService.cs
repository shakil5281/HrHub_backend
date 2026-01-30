using Microsoft.EntityFrameworkCore;
using ERPBackend.Core.Interfaces;
using ERPBackend.Core.Models;
using ERPBackend.Infrastructure.Data;
using System.Linq;

namespace ERPBackend.Services.Services;

public class DataAnalysisService : IDataAnalysisService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPdfService _pdfService;

    public DataAnalysisService(ApplicationDbContext context, IUnitOfWork unitOfWork, IPdfService pdfService)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _pdfService = pdfService;
    }

    public async Task<Dictionary<string, object>> GetEntityStatisticsAsync(string entityType)
    {
        var stats = new Dictionary<string, object>();

        switch (entityType.ToLower())
        {
            case "employee":
                var employeeCount = await _context.Employees.CountAsync();
                var activeEmployees = await _context.Employees.CountAsync(e => e.IsActive);
                var avgSalary = await _context.Employees.AverageAsync(e => e.Salary ?? 0);
                var departmentDistribution = await _context.Employees
                    .GroupBy(e => e.DepartmentId)
                    .Select(g => new { DepartmentId = g.Key, Count = g.Count() })
                    .ToListAsync();

                stats["TotalCount"] = employeeCount;
                stats["ActiveCount"] = activeEmployees;
                stats["AverageSalary"] = avgSalary;
                stats["DepartmentDistribution"] = departmentDistribution;
                break;

            case "department":
                var departmentCount = await _context.Departments.CountAsync();
                var activeDepartments = await _context.Departments.CountAsync(d => d.IsActive);
                
                stats["TotalCount"] = departmentCount;
                stats["ActiveCount"] = activeDepartments;
                break;

            default:
                stats["Error"] = "Unknown entity type";
                break;
        }

        return stats;
    }

    public async Task<IEnumerable<Dictionary<string, object>>> ExecuteCustomQueryAsync(string query)
    {
        var results = new List<Dictionary<string, object>>();
        await Task.CompletedTask;
        return results;
    }

    public async Task<Dictionary<string, object>> GetAggregateDataAsync(
        string entityType, 
        string[] groupByFields, 
        string[] aggregateFields)
    {
        var result = new Dictionary<string, object>();

        switch (entityType.ToLower())
        {
            case "employee":
                if (groupByFields.Contains("DepartmentId"))
                {
                    var grouped = await _context.Employees
                        .GroupBy(e => e.DepartmentId)
                        .Select(g => new
                        {
                            DepartmentId = g.Key,
                            Count = g.Count(),
                            AverageSalary = g.Average(e => e.Salary ?? 0),
                            TotalSalary = g.Sum(e => e.Salary ?? 0)
                        })
                        .ToListAsync();

                    result["GroupedData"] = grouped;
                }
                break;

            default:
                result["Error"] = "Aggregation not implemented for this entity type";
                break;
        }

        return result;
    }

    public async Task<byte[]> GenerateAnalysisReportAsync(
        string entityType, 
        Dictionary<string, object> parameters)
    {
        var stats = await GetEntityStatisticsAsync(entityType);
        var title = $"{entityType} Analysis Report - {DateTime.Now:yyyy-MM-dd}";
        
        return await _pdfService.GeneratePdfFromTemplateAsync(stats, title);
    }
}
