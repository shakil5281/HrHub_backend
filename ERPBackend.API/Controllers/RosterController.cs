using ERPBackend.Core.Constants;
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
    public class RosterController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RosterController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/roster
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RosterDto>>> GetRosters(
            [FromQuery] DateTime? fromDate, 
            [FromQuery] DateTime? toDate, 
            [FromQuery] int? departmentId,
            [FromQuery] string? searchTerm)
        {
            var query = _context.EmployeeShiftRosters
                .Include(r => r.Employee)
                    .ThenInclude(e => e!.Department)
                .Include(r => r.Employee)
                    .ThenInclude(e => e!.Designation)
                .Include(r => r.Shift)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(r => r.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(r => r.Date <= toDate.Value.Date);

            if (departmentId.HasValue)
                query = query.Where(r => r.Employee!.DepartmentId == departmentId.Value);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(r => r.Employee!.EmployeeId.Contains(searchTerm) || 
                                       r.Employee!.FullNameEn.Contains(searchTerm));
            }

            return await query
                .OrderByDescending(r => r.Date)
                .ThenBy(r => r.Employee!.EmployeeId)
                .Select(r => new RosterDto
                {
                    Id = r.Id,
                    EmployeeId = r.EmployeeId,
                    EmployeeIdCard = r.Employee != null ? r.Employee.EmployeeId : string.Empty,
                    EmployeeName = r.Employee != null ? r.Employee.FullNameEn : string.Empty,
                    DepartmentName = (r.Employee != null && r.Employee.Department != null) ? r.Employee.Department.NameEn : string.Empty,
                    DesignationName = (r.Employee != null && r.Employee.Designation != null) ? r.Employee.Designation.NameEn : string.Empty,
                    Date = r.Date,
                    ShiftId = r.ShiftId,
                    ShiftName = r.Shift != null ? r.Shift.NameEn : "N/A",
                    StartTime = r.Shift != null ? r.Shift.InTime : string.Empty,
                    EndTime = r.Shift != null ? r.Shift.OutTime : string.Empty,
                    IsOffDay = r.IsOffDay
                })
                .ToListAsync();
        }

        // POST: api/roster
        [HttpPost]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," + UserRoles.HrOfficer)]
        public async Task<ActionResult<RosterDto>> CreateRoster(CreateRosterDto dto)
        {
            // Check if roster already exists for this employee and date
            var existing = await _context.EmployeeShiftRosters
                .FirstOrDefaultAsync(r => r.EmployeeId == dto.EmployeeId && r.Date.Date == dto.Date.Date);

            if (existing != null)
            {
                existing.ShiftId = dto.ShiftId;
                existing.IsOffDay = dto.IsOffDay;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            else
            {
                var roster = new EmployeeShiftRoster
                {
                    EmployeeId = dto.EmployeeId,
                    Date = dto.Date.Date,
                    ShiftId = dto.ShiftId,
                    IsOffDay = dto.IsOffDay,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier)
                };
                _context.EmployeeShiftRosters.Add(roster);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        // POST: api/roster/bulk
        [HttpPost("bulk")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," + UserRoles.HrOfficer)]
        public async Task<IActionResult> CreateBulkRoster(BulkRosterDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var startDate = dto.StartDate.Date;
            var endDate = dto.EndDate.Date;

            foreach (var empId in dto.EmployeeIds)
            {
                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    var existing = await _context.EmployeeShiftRosters
                        .FirstOrDefaultAsync(r => r.EmployeeId == empId && r.Date.Date == date);

                    if (existing != null)
                    {
                        existing.ShiftId = dto.ShiftId;
                        existing.IsOffDay = dto.IsOffDay;
                        existing.UpdatedAt = DateTime.UtcNow;
                        existing.UpdatedBy = userId;
                    }
                    else
                    {
                        var roster = new EmployeeShiftRoster
                        {
                            EmployeeId = empId,
                            Date = date,
                            ShiftId = dto.ShiftId,
                            IsOffDay = dto.IsOffDay,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = userId
                        };
                        _context.EmployeeShiftRosters.Add(roster);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Bulk roster created successfully" });
        }

        // DELETE: api/roster/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," + UserRoles.HrOfficer)]
        public async Task<IActionResult> DeleteRoster(int id)
        {
            var roster = await _context.EmployeeShiftRosters.FindAsync(id);
            if (roster == null) return NotFound();

            _context.EmployeeShiftRosters.Remove(roster);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
