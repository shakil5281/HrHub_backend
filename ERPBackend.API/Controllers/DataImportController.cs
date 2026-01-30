using Microsoft.AspNetCore.Mvc;
using ERPBackend.Core.Interfaces;
using ERPBackend.Core.DTOs;
using ERPBackend.Core.Models;

namespace ERPBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataImportController : ControllerBase
{
    private readonly IExcelService _excelService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DataImportController> _logger;

    public DataImportController(
        IExcelService excelService,
        IUnitOfWork unitOfWork,
        ILogger<DataImportController> logger)
    {
        _excelService = excelService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpPost("excel/employees")]
    public async Task<ActionResult<ApiResponse<ImportResult>>> ImportEmployeesFromExcel(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<ImportResult>.ErrorResponse("No file uploaded"));

            var startTime = DateTime.UtcNow;
            var importLog = new DataImportLog
            {
                FileName = file.FileName,
                FileType = "Excel",
                EntityType = "Employee",
                ImportStartTime = startTime,
                Status = "InProgress"
            };

            using var stream = file.OpenReadStream();
            var employees = await _excelService.ImportFromExcelAsync<Employee>(stream, "Employees");
            var employeeList = employees.ToList();

            var repository = _unitOfWork.Repository<Employee>();
            await repository.AddRangeAsync(employeeList);
            await _unitOfWork.SaveChangesAsync();

            importLog.ImportEndTime = DateTime.UtcNow;
            importLog.TotalRecords = employeeList.Count;
            importLog.SuccessfulRecords = employeeList.Count;
            importLog.FailedRecords = 0;
            importLog.Status = "Completed";

            var logRepo = _unitOfWork.Repository<DataImportLog>();
            await logRepo.AddAsync(importLog);
            await _unitOfWork.SaveChangesAsync();

            var result = new ImportResult
            {
                TotalRecords = employeeList.Count,
                SuccessfulRecords = employeeList.Count,
                FailedRecords = 0,
                ImportLogId = importLog.Id,
                Duration = DateTime.UtcNow - startTime
            };

            _logger.LogInformation("Imported {Count} employees from {FileName}", employeeList.Count, file.FileName);
            return Ok(ApiResponse<ImportResult>.SuccessResponse(result, "Import completed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing employees from Excel");
            return StatusCode(500, ApiResponse<ImportResult>.ErrorResponse("Import failed", new List<string> { ex.Message }));
        }
    }

    [HttpPost("excel/departments")]
    public async Task<ActionResult<ApiResponse<ImportResult>>> ImportDepartmentsFromExcel(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<ImportResult>.ErrorResponse("No file uploaded"));

            var startTime = DateTime.UtcNow;
            using var stream = file.OpenReadStream();
            var departments = await _excelService.ImportFromExcelAsync<Department>(stream, "Departments");
            var departmentList = departments.ToList();

            var repository = _unitOfWork.Repository<Department>();
            await repository.AddRangeAsync(departmentList);
            await _unitOfWork.SaveChangesAsync();

            var result = new ImportResult
            {
                TotalRecords = departmentList.Count,
                SuccessfulRecords = departmentList.Count,
                FailedRecords = 0,
                Duration = DateTime.UtcNow - startTime
            };

            return Ok(ApiResponse<ImportResult>.SuccessResponse(result, "Import completed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing departments from Excel");
            return StatusCode(500, ApiResponse<ImportResult>.ErrorResponse("Import failed", new List<string> { ex.Message }));
        }
    }

    [HttpGet("logs")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DataImportLog>>>> GetImportLogs()
    {
        try
        {
            var repository = _unitOfWork.Repository<DataImportLog>();
            var logs = await repository.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<DataImportLog>>.SuccessResponse(logs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving import logs");
            return StatusCode(500, ApiResponse<IEnumerable<DataImportLog>>.ErrorResponse("Failed to retrieve logs"));
        }
    }
}
