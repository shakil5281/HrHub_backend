using Microsoft.AspNetCore.Mvc;
using ERPBackend.Core.Interfaces;
using ERPBackend.Core.DTOs;
using ERPBackend.Core.Models;

namespace ERPBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(IUnitOfWork unitOfWork, ILogger<EmployeesController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<Employee>>>> GetAll()
    {
        try
        {
            var repository = _unitOfWork.Repository<Employee>();
            var employees = await repository.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<Employee>>.SuccessResponse(employees));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employees");
            return StatusCode(500, ApiResponse<IEnumerable<Employee>>.ErrorResponse("Failed to retrieve employees"));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Employee>>> GetById(int id)
    {
        try
        {
            var repository = _unitOfWork.Repository<Employee>();
            var employee = await repository.GetByIdAsync(id);
            
            if (employee == null)
                return NotFound(ApiResponse<Employee>.ErrorResponse("Employee not found"));

            return Ok(ApiResponse<Employee>.SuccessResponse(employee));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee {Id}", id);
            return StatusCode(500, ApiResponse<Employee>.ErrorResponse("Failed to retrieve employee"));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Employee>>> Create([FromBody] Employee employee)
    {
        try
        {
            var repository = _unitOfWork.Repository<Employee>();
            await repository.AddAsync(employee);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = employee.Id }, 
                ApiResponse<Employee>.SuccessResponse(employee, "Employee created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee");
            return StatusCode(500, ApiResponse<Employee>.ErrorResponse("Failed to create employee"));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<Employee>>> Update(int id, [FromBody] Employee employee)
    {
        try
        {
            if (id != employee.Id)
                return BadRequest(ApiResponse<Employee>.ErrorResponse("ID mismatch"));

            var repository = _unitOfWork.Repository<Employee>();
            var existing = await repository.GetByIdAsync(id);
            
            if (existing == null)
                return NotFound(ApiResponse<Employee>.ErrorResponse("Employee not found"));

            await repository.UpdateAsync(employee);
            await _unitOfWork.SaveChangesAsync();

            return Ok(ApiResponse<Employee>.SuccessResponse(employee, "Employee updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee {Id}", id);
            return StatusCode(500, ApiResponse<Employee>.ErrorResponse("Failed to update employee"));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var repository = _unitOfWork.Repository<Employee>();
            var employee = await repository.GetByIdAsync(id);
            
            if (employee == null)
                return NotFound(ApiResponse<bool>.ErrorResponse("Employee not found"));

            await repository.DeleteAsync(employee);
            await _unitOfWork.SaveChangesAsync();

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Employee deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to delete employee"));
        }
    }
}
