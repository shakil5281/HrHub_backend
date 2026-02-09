using ERPBackend.Core.DTOs;
using ERPBackend.Core.Models;
using ERPBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ERPBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OtDeductionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OtDeductionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/OTDeduction
        [HttpGet]
        public async Task<ActionResult<OTDeductionResponseDto>> GetOtDeductions(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int? employeeId,
            [FromQuery] int? departmentId,
            [FromQuery] string? status,
            [FromQuery] string? searchTerm)
        {
            try
            {
                var query = _context.OtDeductions
                    .Include(o => o.Employee)
                    .ThenInclude(e => e!.Department)
                    .Include(o => o.Employee)
                    .ThenInclude(e => e!.Designation)
                    .AsQueryable();

                if (fromDate.HasValue)
                    query = query.Where(o => o.Date.Date >= fromDate.Value.Date);

                if (toDate.HasValue)
                    query = query.Where(o => o.Date.Date <= toDate.Value.Date);

                if (employeeId.HasValue)
                    query = query.Where(o => o.EmployeeId == employeeId.Value);

                if (departmentId.HasValue)
                    query = query.Where(o => o.Employee!.DepartmentId == departmentId.Value);

                if (!string.IsNullOrWhiteSpace(status))
                    query = query.Where(o => o.Status == status);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                    query = query.Where(o =>
                        o.Employee!.EmployeeId.Contains(searchTerm) ||
                        o.Employee!.FullNameEn.Contains(searchTerm));

                var records = await query
                    .OrderByDescending(o => o.Date)
                    .Select(o => new OTDeductionDto
                    {
                        Id = o.Id,
                        EmployeeId = o.EmployeeId,
                        EmployeeIdCard = o.Employee!.EmployeeId,
                        EmployeeName = o.Employee!.FullNameEn,
                        Department = o.Employee!.Department!.NameEn,
                        Designation = o.Employee!.Designation!.NameEn,
                        Date = o.Date,
                        DeductionHours = o.DeductionHours,
                        Reason = o.Reason,
                        Remarks = o.Remarks,
                        Status = o.Status,
                        CreatedAt = o.CreatedAt
                    })
                    .ToListAsync();

                var summary = new OTDeductionSummaryDto
                {
                    TotalDeductedHours = records.Sum(r => r.DeductionHours),
                    TotalEmployeesAffected = records.Select(r => r.EmployeeId).Distinct().Count(),
                    PendingRequests = records.Count(r => r.Status == "Pending"),
                    AverageDeduction = records.Count > 0 ? records.Average(r => r.DeductionHours) : 0
                };

                return Ok(new OTDeductionResponseDto
                {
                    Summary = summary,
                    Records = records
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    new { message = "An error occurred while fetching OT deduction records.", error = ex.Message });
            }
        }

        // GET: api/OTDeduction/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<OTDeductionDto>> GetOtDeduction(int id)
        {
            try
            {
                var o = await _context.OtDeductions
                    .Include(o => o.Employee)
                    .ThenInclude(e => e!.Department)
                    .Include(o => o.Employee)
                    .ThenInclude(e => e!.Designation)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (o == null)
                    return NotFound(new { message = "OT deduction record not found" });

                return Ok(new OTDeductionDto
                {
                    Id = o.Id,
                    EmployeeId = o.EmployeeId,
                    EmployeeIdCard = o.Employee!.EmployeeId,
                    EmployeeName = o.Employee!.FullNameEn,
                    Department = o.Employee!.Department!.NameEn,
                    Designation = o.Employee!.Designation!.NameEn,
                    Date = o.Date,
                    DeductionHours = o.DeductionHours,
                    Reason = o.Reason,
                    Remarks = o.Remarks,
                    Status = o.Status,
                    CreatedAt = o.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    new { message = "An error occurred while fetching OT deduction record.", error = ex.Message });
            }
        }

        // POST: api/OTDeduction
        [HttpPost]
        public async Task<ActionResult<OTDeductionDto>> CreateOtDeduction([FromBody] CreateOTDeductionDto dto)
        {
            try
            {
                var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

                var employee = await _context.Employees
                    .Include(e => e.Department)
                    .Include(e => e.Designation)
                    .FirstOrDefaultAsync(e => e.Id == dto.EmployeeId);

                if (employee == null)
                    return NotFound(new { message = "Employee not found" });

                var o = new OTDeduction
                {
                    EmployeeId = dto.EmployeeId,
                    Date = dto.Date,
                    DeductionHours = dto.DeductionHours,
                    Reason = dto.Reason,
                    Remarks = dto.Remarks,
                    Status = dto.Status,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userName
                };

                _context.OtDeductions.Add(o);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetOtDeduction), new { id = o.Id }, new OTDeductionDto
                {
                    Id = o.Id,
                    EmployeeId = employee.Id,
                    EmployeeIdCard = employee.EmployeeId,
                    EmployeeName = employee.FullNameEn,
                    Department = employee.Department!.NameEn,
                    Designation = employee.Designation!.NameEn,
                    Date = o.Date,
                    DeductionHours = o.DeductionHours,
                    Reason = o.Reason,
                    Remarks = o.Remarks,
                    Status = o.Status,
                    CreatedAt = o.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    new { message = "An error occurred while creating OT deduction record.", error = ex.Message });
            }
        }

        // PUT: api/OTDeduction/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOtDeduction(int id, [FromBody] CreateOTDeductionDto dto)
        {
            try
            {
                var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

                var o = await _context.OtDeductions.FindAsync(id);
                if (o == null)
                    return NotFound(new { message = "OT deduction record not found" });

                o.Date = dto.Date;
                o.DeductionHours = dto.DeductionHours;
                o.Reason = dto.Reason;
                o.Remarks = dto.Remarks;
                o.Status = dto.Status;
                o.UpdatedAt = DateTime.UtcNow;
                o.UpdatedBy = userName;

                await _context.SaveChangesAsync();

                return Ok(new { message = "OT deduction record updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    new { message = "An error occurred while updating OT deduction record.", error = ex.Message });
            }
        }

        // DELETE: api/OTDeduction/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOtDeduction(int id)
        {
            try
            {
                var o = await _context.OtDeductions.FindAsync(id);
                if (o == null)
                    return NotFound(new { message = "OT deduction record not found" });

                _context.OtDeductions.Remove(o);
                await _context.SaveChangesAsync();

                return Ok(new { message = "OT deduction record deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    new { message = "An error occurred while deleting OT deduction record.", error = ex.Message });
            }
        }
    }
}
