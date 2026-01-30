using Microsoft.AspNetCore.Mvc;
using ERPBackend.Core.Interfaces;
using ERPBackend.Core.DTOs;
using ERPBackend.Core.Models;

namespace ERPBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DepartmentsController> _logger;

    public DepartmentsController(IUnitOfWork unitOfWork, ILogger<DepartmentsController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<Department>>>> GetAll()
    {
        try
        {
            var repository = _unitOfWork.Repository<Department>();
            var departments = await repository.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<Department>>.SuccessResponse(departments));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving departments");
            return StatusCode(500, ApiResponse<IEnumerable<Department>>.ErrorResponse("Failed to retrieve departments"));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Department>>> GetById(int id)
    {
        try
        {
            var repository = _unitOfWork.Repository<Department>();
            var department = await repository.GetByIdAsync(id);
            
            if (department == null)
                return NotFound(ApiResponse<Department>.ErrorResponse("Department not found"));

            return Ok(ApiResponse<Department>.SuccessResponse(department));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving department {Id}", id);
            return StatusCode(500, ApiResponse<Department>.ErrorResponse("Failed to retrieve department"));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Department>>> Create([FromBody] Department department)
    {
        try
        {
            var repository = _unitOfWork.Repository<Department>();
            await repository.AddAsync(department);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = department.Id }, 
                ApiResponse<Department>.SuccessResponse(department, "Department created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating department");
            return StatusCode(500, ApiResponse<Department>.ErrorResponse("Failed to create department"));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<Department>>> Update(int id, [FromBody] Department department)
    {
        try
        {
            if (id != department.Id)
                return BadRequest(ApiResponse<Department>.ErrorResponse("ID mismatch"));

            var repository = _unitOfWork.Repository<Department>();
            var existing = await repository.GetByIdAsync(id);
            
            if (existing == null)
                return NotFound(ApiResponse<Department>.ErrorResponse("Department not found"));

            await repository.UpdateAsync(department);
            await _unitOfWork.SaveChangesAsync();

            return Ok(ApiResponse<Department>.SuccessResponse(department, "Department updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating department {Id}", id);
            return StatusCode(500, ApiResponse<Department>.ErrorResponse("Failed to update department"));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var repository = _unitOfWork.Repository<Department>();
            var department = await repository.GetByIdAsync(id);
            
            if (department == null)
                return NotFound(ApiResponse<bool>.ErrorResponse("Department not found"));

            await repository.DeleteAsync(department);
            await _unitOfWork.SaveChangesAsync();

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Department deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting department {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to delete department"));
        }
    }
}
