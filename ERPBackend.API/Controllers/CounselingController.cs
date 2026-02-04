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
    public class CounselingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CounselingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/counseling
        [HttpGet]
        public async Task<ActionResult<CounselingResponseDto>> GetCounselingRecords(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int? employeeId,
            [FromQuery] int? departmentId,
            [FromQuery] string? issueType,
            [FromQuery] string? status,
            [FromQuery] string? searchTerm)
        {
            try
            {
                var query = _context.CounselingRecords
                    .Include(c => c.Employee)
                    .ThenInclude(e => e!.Department)
                    .Include(c => c.Employee)
                    .ThenInclude(e => e!.Designation)
                    .AsQueryable();

                if (fromDate.HasValue)
                    query = query.Where(c => c.CounselingDate.Date >= fromDate.Value.Date);

                if (toDate.HasValue)
                    query = query.Where(c => c.CounselingDate.Date <= toDate.Value.Date);

                if (employeeId.HasValue)
                    query = query.Where(c => c.EmployeeId == employeeId.Value);

                if (departmentId.HasValue)
                    query = query.Where(c => c.Employee!.DepartmentId == departmentId.Value);

                if (!string.IsNullOrWhiteSpace(issueType))
                    query = query.Where(c => c.IssueType == issueType);

                if (!string.IsNullOrWhiteSpace(status))
                    query = query.Where(c => c.Status == status);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                    query = query.Where(c =>
                        c.Employee!.EmployeeId.Contains(searchTerm) ||
                        c.Employee!.FullNameEn.Contains(searchTerm));

                var records = await query
                    .OrderByDescending(c => c.CounselingDate)
                    .Select(c => new CounselingRecordDto
                    {
                        Id = c.Id,
                        EmployeeId = c.EmployeeId,
                        EmployeeIdCard = c.Employee!.EmployeeId,
                        EmployeeName = c.Employee!.FullNameEn,
                        Department = c.Employee!.Department!.NameEn,
                        Designation = c.Employee!.Designation!.NameEn,
                        CounselingDate = c.CounselingDate,
                        IssueType = c.IssueType,
                        Description = c.Description,
                        ActionTaken = c.ActionTaken,
                        FollowUpNotes = c.FollowUpNotes,
                        Status = c.Status,
                        Severity = c.Severity,
                        FollowUpDate = c.FollowUpDate,
                        CreatedBy = c.CreatedBy,
                        CreatedAt = c.CreatedAt
                    })
                    .ToListAsync();

                var summary = new CounselingSummaryDto
                {
                    TotalRecords = records.Count,
                    OpenCases = records.Count(r => r.Status == "Open"),
                    ClosedCases = records.Count(r => r.Status == "Closed"),
                    HighSeverity = records.Count(r => r.Severity == "High"),
                    RequiringFollowUp = records.Count(r => r.Status == "Follow-up Required")
                };

                return Ok(new CounselingResponseDto
                {
                    Summary = summary,
                    Records = records
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching counseling records.", error = ex.Message });
            }
        }

        // GET: api/counseling/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CounselingRecordDto>> GetCounselingRecord(int id)
        {
            try
            {
                var record = await _context.CounselingRecords
                    .Include(c => c.Employee)
                    .ThenInclude(e => e!.Department)
                    .Include(c => c.Employee)
                    .ThenInclude(e => e!.Designation)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (record == null)
                    return NotFound(new { message = "Counseling record not found" });

                return Ok(new CounselingRecordDto
                {
                    Id = record.Id,
                    EmployeeId = record.EmployeeId,
                    EmployeeIdCard = record.Employee!.EmployeeId,
                    EmployeeName = record.Employee!.FullNameEn,
                    Department = record.Employee!.Department!.NameEn,
                    Designation = record.Employee!.Designation!.NameEn,
                    CounselingDate = record.CounselingDate,
                    IssueType = record.IssueType,
                    Description = record.Description,
                    ActionTaken = record.ActionTaken,
                    FollowUpNotes = record.FollowUpNotes,
                    Status = record.Status,
                    Severity = record.Severity,
                    FollowUpDate = record.FollowUpDate,
                    CreatedBy = record.CreatedBy,
                    CreatedAt = record.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching counseling record.", error = ex.Message });
            }
        }

        // POST: api/counseling
        [HttpPost]
        public async Task<ActionResult<CounselingRecordDto>> CreateCounselingRecord([FromBody] CreateCounselingRecordDto dto)
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

                var record = new CounselingRecord
                {
                    EmployeeId = dto.EmployeeId,
                    CounselingDate = dto.CounselingDate,
                    IssueType = dto.IssueType,
                    Description = dto.Description,
                    ActionTaken = dto.ActionTaken,
                    FollowUpNotes = dto.FollowUpNotes,
                    Status = dto.Status,
                    Severity = dto.Severity,
                    FollowUpDate = dto.FollowUpDate,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userName
                };

                _context.CounselingRecords.Add(record);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetCounselingRecord), new { id = record.Id }, new CounselingRecordDto
                {
                    Id = record.Id,
                    EmployeeId = employee.Id,
                    EmployeeIdCard = employee.EmployeeId,
                    EmployeeName = employee.FullNameEn,
                    Department = employee.Department!.NameEn,
                    Designation = employee.Designation!.NameEn,
                    CounselingDate = record.CounselingDate,
                    IssueType = record.IssueType,
                    Description = record.Description,
                    ActionTaken = record.ActionTaken,
                    FollowUpNotes = record.FollowUpNotes,
                    Status = record.Status,
                    Severity = record.Severity,
                    FollowUpDate = record.FollowUpDate,
                    CreatedBy = record.CreatedBy,
                    CreatedAt = record.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating counseling record.", error = ex.Message });
            }
        }

        // PUT: api/counseling/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCounselingRecord(int id, [FromBody] CreateCounselingRecordDto dto)
        {
            try
            {
                var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

                var record = await _context.CounselingRecords.FindAsync(id);
                if (record == null)
                    return NotFound(new { message = "Counseling record not found" });

                record.CounselingDate = dto.CounselingDate;
                record.IssueType = dto.IssueType;
                record.Description = dto.Description;
                record.ActionTaken = dto.ActionTaken;
                record.FollowUpNotes = dto.FollowUpNotes;
                record.Status = dto.Status;
                record.Severity = dto.Severity;
                record.FollowUpDate = dto.FollowUpDate;
                record.UpdatedAt = DateTime.UtcNow;
                record.UpdatedBy = userName;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Counseling record updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating counseling record.", error = ex.Message });
            }
        }

        // DELETE: api/counseling/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCounselingRecord(int id)
        {
            try
            {
                var record = await _context.CounselingRecords.FindAsync(id);
                if (record == null)
                    return NotFound(new { message = "Counseling record not found" });

                _context.CounselingRecords.Remove(record);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Counseling record deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting counseling record.", error = ex.Message });
            }
        }
    }
}
