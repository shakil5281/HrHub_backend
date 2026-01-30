using Microsoft.AspNetCore.Mvc;
using ERPBackend.Core.Interfaces;
using ERPBackend.Core.DTOs;
using ERPBackend.Core.Models;

namespace ERPBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataExportController : ControllerBase
{
    private readonly IExcelService _excelService;
    private readonly IPdfService _pdfService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DataExportController> _logger;

    public DataExportController(
        IExcelService excelService,
        IPdfService pdfService,
        IUnitOfWork unitOfWork,
        ILogger<DataExportController> logger)
    {
        _excelService = excelService;
        _pdfService = pdfService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet("excel/employees")]
    public async Task<IActionResult> ExportEmployeesToExcel()
    {
        try
        {
            var startTime = DateTime.UtcNow;
            var repository = _unitOfWork.Repository<Employee>();
            var employees = await repository.GetAllAsync();

            var excelData = await _excelService.ExportToExcelAsync(employees, "Employees");
            var fileName = $"Employees_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            var exportLog = new DataExportLog
            {
                FileName = fileName,
                FileType = "Excel",
                EntityType = "Employee",
                TotalRecords = employees.Count(),
                ExportStartTime = startTime,
                ExportEndTime = DateTime.UtcNow,
                Status = "Completed",
                FileSizeBytes = excelData.Length
            };

            var logRepo = _unitOfWork.Repository<DataExportLog>();
            await logRepo.AddAsync(exportLog);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Exported {Count} employees to Excel", employees.Count());
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting employees to Excel");
            return StatusCode(500, "Export failed");
        }
    }

    [HttpGet("excel/departments")]
    public async Task<IActionResult> ExportDepartmentsToExcel()
    {
        try
        {
            var repository = _unitOfWork.Repository<Department>();
            var departments = await repository.GetAllAsync();

            var excelData = await _excelService.ExportToExcelAsync(departments, "Departments");
            var fileName = $"Departments_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting departments to Excel");
            return StatusCode(500, "Export failed");
        }
    }

    [HttpGet("pdf/employees")]
    public async Task<IActionResult> ExportEmployeesToPdf()
    {
        try
        {
            var repository = _unitOfWork.Repository<Employee>();
            var employees = await repository.GetAllAsync();

            var columns = new[] { "EmployeeCode", "FirstName", "LastName", "Email", "Position", "Salary" };
            var pdfData = await _pdfService.ExportToPdfAsync(employees, columns, "Employee Report");
            var fileName = $"Employees_{DateTime.Now:yyyyMMddHHmmss}.pdf";

            return File(pdfData, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting employees to PDF");
            return StatusCode(500, "Export failed");
        }
    }

    [HttpGet("pdf/departments")]
    public async Task<IActionResult> ExportDepartmentsToPdf()
    {
        try
        {
            var repository = _unitOfWork.Repository<Department>();
            var departments = await repository.GetAllAsync();

            var pdfData = await _pdfService.GeneratePdfReportAsync(departments, "Department Report");
            var fileName = $"Departments_{DateTime.Now:yyyyMMddHHmmss}.pdf";

            return File(pdfData, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting departments to PDF");
            return StatusCode(500, "Export failed");
        }
    }

    [HttpGet("logs")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DataExportLog>>>> GetExportLogs()
    {
        try
        {
            var repository = _unitOfWork.Repository<DataExportLog>();
            var logs = await repository.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<DataExportLog>>.SuccessResponse(logs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving export logs");
            return StatusCode(500, ApiResponse<IEnumerable<DataExportLog>>.ErrorResponse("Failed to retrieve logs"));
        }
    }
}
