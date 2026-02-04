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
    public class SeparationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SeparationController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SeparationDto>>> GetSeparations()
        {
            var separations = await _context.Separations
                .Include(s => s.Employee)
                .ThenInclude(e => e.Department)
                .Include(s => s.Employee)
                .ThenInclude(e => e.Designation)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return separations.Select(s => new SeparationDto
            {
                Id = s.Id,
                EmployeeId = s.EmployeeId,
                EmployeeName = s.Employee?.FullNameEn ?? "Unknown",
                EmployeeCode = s.Employee?.EmployeeId ?? "N/A",
                DepartmentName = s.Employee?.Department?.NameEn ?? "N/A",
                DesignationName = s.Employee?.Designation?.NameEn ?? "N/A",
                LastWorkingDate = s.LastWorkingDate,
                Type = s.Type,
                Reason = s.Reason,
                Status = s.Status,
                AdminRemark = s.AdminRemark,
                IsSettled = s.IsSettled,
                CreatedAt = s.CreatedAt
            }).ToList();
        }

        [HttpPost]
        public async Task<ActionResult<SeparationDto>> CreateSeparation(CreateSeparationDto dto)
        {
            var employee = await _context.Employees.FindAsync(dto.EmployeeId);
            if (employee == null) return NotFound("Employee not found");

            // Check if already has pending or approved separation
            var existing = await _context.Separations
                .AnyAsync(s => s.EmployeeId == dto.EmployeeId && (s.Status == "Pending" || s.Status == "Approved"));
            
            if (existing) return BadRequest("Active separation request already exists for this employee.");

            var separation = new Separation
            {
                EmployeeId = dto.EmployeeId,
                LastWorkingDate = dto.LastWorkingDate,
                Type = dto.Type,
                Reason = dto.Reason,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };

            _context.Separations.Add(separation);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Separation request created", id = separation.Id });
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager)]
        public async Task<IActionResult> UpdateStatus(int id, UpdateSeparationStatusDto dto)
        {
            var separation = await _context.Separations
                .Include(s => s.Employee)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (separation == null) return NotFound();

            if (separation.Status == "Approved" || separation.Status == "Rejected")
            {
                return BadRequest($"Separation is already {separation.Status}");
            }

            if (dto.Status == "Approved")
            {
                var employee = separation.Employee;
                if (employee != null)
                {
                    employee.Status = "Inactive"; // Or "Separated" depending on your enum/string convention
                    employee.IsActive = false;
                    _context.Employees.Update(employee);
                }
                separation.ApprovedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
                separation.ApprovedAt = DateTime.UtcNow;
                separation.IsSettled = true; // Auto-settle or leave for finance? Let's auto-settle for now mostly.
                separation.SettledAt = DateTime.UtcNow;
            }

            separation.Status = dto.Status;
            separation.AdminRemark = dto.AdminRemark;

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Separation {dto.Status}" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSeparation(int id)
        {
            var separation = await _context.Separations.FindAsync(id);
            if (separation == null) return NotFound();

            if (separation.Status == "Approved") return BadRequest("Cannot delete approved separation");

            _context.Separations.Remove(separation);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
