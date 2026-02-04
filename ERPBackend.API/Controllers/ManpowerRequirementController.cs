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
    public class ManpowerRequirementController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ManpowerRequirementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/manpowerrequirement
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ManpowerRequirementDto>>> GetRequirements()
        {
            var currentCounts = await _context.Employees
                .Where(e => e.IsActive && e.Status == "Active")
                .GroupBy(e => new { e.DepartmentId, e.DesignationId })
                .Select(g => new { g.Key.DepartmentId, g.Key.DesignationId, Count = g.Count() })
                .ToListAsync();

            var requirements = await _context.ManpowerRequirements
                .Include(r => r.Department)
                .Include(r => r.Designation)
                .ToListAsync();

            return requirements.Select(r => {
                var current = currentCounts.FirstOrDefault(c => c.DepartmentId == r.DepartmentId && c.DesignationId == r.DesignationId)?.Count ?? 0;
                return new ManpowerRequirementDto
                {
                    Id = r.Id,
                    DepartmentId = r.DepartmentId,
                    DepartmentName = r.Department?.NameEn,
                    DesignationId = r.DesignationId,
                    DesignationName = r.Designation?.NameEn,
                    RequiredCount = r.RequiredCount,
                    CurrentCount = current,
                    Gap = r.RequiredCount - current,
                    Note = r.Note,
                    CreatedAt = r.CreatedAt
                };
            }).ToList();
        }

        // POST: api/manpowerrequirement
        [HttpPost]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager)]
        public async Task<ActionResult<ManpowerRequirementDto>> CreateRequirement(CreateManpowerRequirementDto dto)
        {
            var requirement = new ManpowerRequirement
            {
                DepartmentId = dto.DepartmentId,
                DesignationId = dto.DesignationId,
                RequiredCount = dto.RequiredCount,
                Note = dto.Note,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };

            _context.ManpowerRequirements.Add(requirement);
            await _context.SaveChangesAsync();

            return Ok(requirement);
        }

        // PUT: api/manpowerrequirement/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager)]
        public async Task<IActionResult> UpdateRequirement(int id, CreateManpowerRequirementDto dto)
        {
            var requirement = await _context.ManpowerRequirements.FindAsync(id);
            if (requirement == null) return NotFound();

            requirement.DepartmentId = dto.DepartmentId;
            requirement.DesignationId = dto.DesignationId;
            requirement.RequiredCount = dto.RequiredCount;
            requirement.Note = dto.Note;
            requirement.UpdatedAt = DateTime.UtcNow;
            requirement.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/manpowerrequirement/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager)]
        public async Task<IActionResult> DeleteRequirement(int id)
        {
            var requirement = await _context.ManpowerRequirements.FindAsync(id);
            if (requirement == null) return NotFound();

            _context.ManpowerRequirements.Remove(requirement);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
