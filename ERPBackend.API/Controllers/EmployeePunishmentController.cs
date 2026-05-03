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
    public class EmployeePunishmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EmployeePunishmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/EmployeePunishment
        [HttpGet]
        public async Task<ActionResult<PunishmentResponseDto>> GetPunishments(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int? employeeId,
            [FromQuery] int? departmentId,
            [FromQuery] string? punishmentType,
            [FromQuery] string? status,
            [FromQuery] string? searchTerm)
        {
            try
            {
                var query = _context.EmployeePunishments
                    .Include(p => p.Employee)
                    .ThenInclude(e => e!.Department)
                    .Include(p => p.Employee)
                    .ThenInclude(e => e!.Designation)
                    .AsQueryable();

                if (fromDate.HasValue)
                    query = query.Where(p => p.PunishmentDate.Date >= fromDate.Value.Date);

                if (toDate.HasValue)
                    query = query.Where(p => p.PunishmentDate.Date <= toDate.Value.Date);

                if (employeeId.HasValue)
                    query = query.Where(p => p.EmployeeId == employeeId.Value);

                if (departmentId.HasValue)
                    query = query.Where(p => p.Employee!.DepartmentId == departmentId.Value);

                if (!string.IsNullOrWhiteSpace(punishmentType))
                    query = query.Where(p => p.PunishmentType == punishmentType);

                if (!string.IsNullOrWhiteSpace(status))
                    query = query.Where(p => p.Status == status);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                    query = query.Where(p =>
                        p.Employee!.EmployeeId.Contains(searchTerm) ||
                        p.Employee!.FullNameEn.Contains(searchTerm));

                var records = await query
                    .OrderByDescending(p => p.PunishmentDate)
                    .Select(p => new EmployeePunishmentDto
                    {
                        Id = p.Id,
                        EmployeeId = p.EmployeeId,
                        EmployeeCard = p.Employee!.EmployeeId,
                        EmployeeName = p.Employee!.FullNameEn,
                        Department = p.Employee!.Department!.NameEn,
                        Designation = p.Employee!.Designation!.NameEn,
                        PunishmentType = p.PunishmentType,
                        Reason = p.Reason,
                        FineAmount = p.FineAmount,
                        SuspensionDays = p.SuspensionDays,
                        PunishmentDate = p.PunishmentDate,
                        EffectiveDate = p.EffectiveDate,
                        ExpiryDate = p.ExpiryDate,
                        Status = p.Status,
                        Remarks = p.Remarks,
                        CreatedBy = p.CreatedBy,
                        CreatedAt = p.CreatedAt
                    })
                    .ToListAsync();

                var summary = new PunishmentSummaryDto
                {
                    TotalRecords = records.Count,
                    ActivePunishments = records.Count(r => r.Status == "Active"),
                    Warnings = records.Count(r => r.PunishmentType == "Warning"),
                    Fines = records.Count(r => r.PunishmentType == "Fine"),
                    Suspensions = records.Count(r => r.PunishmentType == "Suspension"),
                    TotalFineAmount = records.Sum(r => r.FineAmount)
                };

                return Ok(new PunishmentResponseDto
                {
                    Summary = summary,
                    Records = records
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching punishment records.", error = ex.Message });
            }
        }

        // GET: api/EmployeePunishment/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeePunishmentDto>> GetPunishment(int id)
        {
            try
            {
                var p = await _context.EmployeePunishments
                    .Include(p => p.Employee)
                    .ThenInclude(e => e!.Department)
                    .Include(p => p.Employee)
                    .ThenInclude(e => e!.Designation)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (p == null)
                    return NotFound(new { message = "Punishment record not found" });

                return Ok(new EmployeePunishmentDto
                {
                    Id = p.Id,
                    EmployeeId = p.EmployeeId,
                    EmployeeCard = p.Employee!.EmployeeId,
                    EmployeeName = p.Employee!.FullNameEn,
                    Department = p.Employee!.Department!.NameEn,
                    Designation = p.Employee!.Designation!.NameEn,
                    PunishmentType = p.PunishmentType,
                    Reason = p.Reason,
                    FineAmount = p.FineAmount,
                    SuspensionDays = p.SuspensionDays,
                    PunishmentDate = p.PunishmentDate,
                    EffectiveDate = p.EffectiveDate,
                    ExpiryDate = p.ExpiryDate,
                    Status = p.Status,
                    Remarks = p.Remarks,
                    CreatedBy = p.CreatedBy,
                    CreatedAt = p.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching punishment record.", error = ex.Message });
            }
        }

        // POST: api/EmployeePunishment
        [HttpPost]
        public async Task<ActionResult<EmployeePunishmentDto>> CreatePunishment([FromBody] CreateEmployeePunishmentDto dto)
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

                var punishment = new EmployeePunishment
                {
                    EmployeeId = dto.EmployeeId,
                    PunishmentType = dto.PunishmentType,
                    Reason = dto.Reason,
                    FineAmount = dto.FineAmount,
                    SuspensionDays = dto.SuspensionDays,
                    PunishmentDate = dto.PunishmentDate,
                    EffectiveDate = dto.EffectiveDate,
                    ExpiryDate = dto.ExpiryDate,
                    Status = dto.Status,
                    Remarks = dto.Remarks,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userName
                };

                _context.EmployeePunishments.Add(punishment);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetPunishment), new { id = punishment.Id }, new EmployeePunishmentDto
                {
                    Id = punishment.Id,
                    EmployeeId = employee.Id,
                    EmployeeCard = employee.EmployeeId,
                    EmployeeName = employee.FullNameEn,
                    Department = employee.Department!.NameEn,
                    Designation = employee.Designation!.NameEn,
                    PunishmentType = punishment.PunishmentType,
                    Reason = punishment.Reason,
                    FineAmount = punishment.FineAmount,
                    SuspensionDays = punishment.SuspensionDays,
                    PunishmentDate = punishment.PunishmentDate,
                    EffectiveDate = punishment.EffectiveDate,
                    ExpiryDate = punishment.ExpiryDate,
                    Status = punishment.Status,
                    Remarks = punishment.Remarks,
                    CreatedBy = punishment.CreatedBy,
                    CreatedAt = punishment.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating punishment record.", error = ex.Message });
            }
        }

        // PUT: api/EmployeePunishment/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePunishment(int id, [FromBody] CreateEmployeePunishmentDto dto)
        {
            try
            {
                var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

                var punishment = await _context.EmployeePunishments.FindAsync(id);
                if (punishment == null)
                    return NotFound(new { message = "Punishment record not found" });

                punishment.PunishmentType = dto.PunishmentType;
                punishment.Reason = dto.Reason;
                punishment.FineAmount = dto.FineAmount;
                punishment.SuspensionDays = dto.SuspensionDays;
                punishment.PunishmentDate = dto.PunishmentDate;
                punishment.EffectiveDate = dto.EffectiveDate;
                punishment.ExpiryDate = dto.ExpiryDate;
                punishment.Status = dto.Status;
                punishment.Remarks = dto.Remarks;
                punishment.UpdatedAt = DateTime.UtcNow;
                punishment.UpdatedBy = userName;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Punishment record updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating punishment record.", error = ex.Message });
            }
        }

        // DELETE: api/EmployeePunishment/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePunishment(int id)
        {
            try
            {
                var punishment = await _context.EmployeePunishments.FindAsync(id);
                if (punishment == null)
                    return NotFound(new { message = "Punishment record not found" });

                _context.EmployeePunishments.Remove(punishment);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Punishment record deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting punishment record.", error = ex.Message });
            }
        }
    }
}
