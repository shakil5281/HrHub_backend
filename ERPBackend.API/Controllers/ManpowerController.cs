using ERPBackend.Core.DTOs;
using ERPBackend.Core.Constants;
using ERPBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPBackend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ManpowerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ManpowerController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," + UserRoles.HrOfficer)]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetManpower(
            [FromQuery] int? departmentId,
            [FromQuery] int? sectionId,
            [FromQuery] int? designationId,
            [FromQuery] int? lineId,
            [FromQuery] int? shiftId,
            [FromQuery] int? groupId,
            [FromQuery] int? floorId,
            [FromQuery] string? status,
            [FromQuery] string? searchTerm)
        {
            var query = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Section)
                .Include(e => e.Designation)
                .Include(e => e.Line)
                .Include(e => e.Shift)
                .Include(e => e.Group)
                .Include(e => e.Floor)
                .Where(e => e.IsActive)
                .AsQueryable();

            if (departmentId.HasValue)
                query = query.Where(e => e.DepartmentId == departmentId.Value);

            if (sectionId.HasValue)
                query = query.Where(e => e.SectionId == sectionId.Value);

            if (designationId.HasValue)
                query = query.Where(e => e.DesignationId == designationId.Value);

            if (lineId.HasValue)
                query = query.Where(e => e.LineId == lineId.Value);

            if (shiftId.HasValue)
                query = query.Where(e => e.ShiftId == shiftId.Value);

            if (groupId.HasValue)
                query = query.Where(e => e.GroupId == groupId.Value);

            if (floorId.HasValue)
                query = query.Where(e => e.FloorId == floorId.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(e => e.Status == status);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(e => 
                    e.EmployeeId.Contains(searchTerm) || 
                    e.FullNameEn.Contains(searchTerm) || 
                    (e.FullNameBn != null && e.FullNameBn.Contains(searchTerm)) ||
                    (e.PhoneNumber != null && e.PhoneNumber.Contains(searchTerm))
                );
            }

            var manpower = await query
                .OrderBy(e => e.EmployeeId)
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    EmployeeId = e.EmployeeId,
                    FullNameEn = e.FullNameEn,
                    FullNameBn = e.FullNameBn,
                    DepartmentId = e.DepartmentId,
                    DepartmentName = e.Department != null ? e.Department.NameEn : null,
                    SectionId = e.SectionId,
                    SectionName = e.Section != null ? e.Section.NameEn : null,
                    DesignationId = e.DesignationId,
                    DesignationName = e.Designation != null ? e.Designation.NameEn : null,
                    LineId = e.LineId,
                    LineName = e.Line != null ? e.Line.NameEn : null,
                    ShiftId = e.ShiftId,
                    ShiftName = e.Shift != null ? e.Shift.NameEn : null,
                    GroupId = e.GroupId,
                    GroupName = e.Group != null ? e.Group.NameEn : null,
                    FloorId = e.FloorId,
                    FloorName = e.Floor != null ? e.Floor.NameEn : null,
                    Status = e.Status,
                    JoinDate = e.JoinDate,
                    Email = e.Email,
                    PhoneNumber = e.PhoneNumber,
                    ProfileImageUrl = e.ProfileImageUrl,
                    IsActive = e.IsActive,
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync();

            return Ok(manpower);
        }

        [HttpGet("summary")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," + UserRoles.HrOfficer)]
        public async Task<ActionResult<ManpowerSummaryDto>> GetSummary()
        {
            var allEmployees = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .ToListAsync();

            var total = allEmployees.Count;
            if (total == 0) return Ok(new ManpowerSummaryDto());

            var summary = new ManpowerSummaryDto
            {
                TotalEmployees = total,
                ActiveEmployees = allEmployees.Count(e => e.IsActive && e.Status == "Active"),
                OnLeaveEmployees = allEmployees.Count(e => e.Status == "On Leave"),
                InactiveEmployees = allEmployees.Count(e => !e.IsActive),

                DepartmentSummary = allEmployees
                    .GroupBy(e => e.Department?.NameEn ?? "Unknown")
                    .Select(g => new SummaryItemDto
                    {
                        Id = g.Key,
                        Name = g.Key,
                        Count = g.Count(),
                        Percentage = Math.Round((double)g.Count() / total * 100, 2)
                    })
                    .OrderByDescending(s => s.Count)
                    .ToList(),

                DesignationSummary = allEmployees
                    .GroupBy(e => e.Designation?.NameEn ?? "Unknown")
                    .Select(g => new SummaryItemDto
                    {
                        Id = g.Key,
                        Name = g.Key,
                        Count = g.Count(),
                        Percentage = Math.Round((double)g.Count() / total * 100, 2)
                    })
                    .OrderByDescending(s => s.Count)
                    .Take(10)
                    .ToList(),

                GenderSummary = allEmployees
                    .GroupBy(e => e.Gender ?? "Not Specified")
                    .Select(g => new SummaryItemDto
                    {
                        Id = g.Key,
                        Name = g.Key,
                        Count = g.Count(),
                        Percentage = Math.Round((double)g.Count() / total * 100, 2)
                    })
                    .ToList(),

                StatusSummary = allEmployees
                    .GroupBy(e => e.Status ?? "Unknown")
                    .Select(g => new SummaryItemDto
                    {
                        Id = g.Key,
                        Name = g.Key,
                        Count = g.Count(),
                        Percentage = Math.Round((double)g.Count() / total * 100, 2)
                    })
                    .ToList()
            };

            return Ok(summary);
        }
    }
}
