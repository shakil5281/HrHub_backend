using ERPBackend.Core.Constants;
using ERPBackend.Core.DTOs;
using ERPBackend.Core.Models;
using ERPBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ShiftController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ShiftController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/shift
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShiftDto>>> GetShifts()
        {
            return await _context.Shifts
                .Select(s => new ShiftDto
                {
                    Id = s.Id,
                    NameEn = s.NameEn,
                    NameBn = s.NameBn,
                    InTime = s.InTime,
                    OutTime = s.OutTime,
                    LateInTime = s.LateInTime,
                    LunchTimeStart = s.LunchTimeStart,
                    LunchHour = s.LunchHour,
                    Weekends = s.Weekends,
                    Status = s.Status
                })
                .ToListAsync();
        }

        // GET: api/shift/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ShiftDto>> GetShift(int id)
        {
            var s = await _context.Shifts.FindAsync(id);
            if (s == null) return NotFound();

            return new ShiftDto
            {
                Id = s.Id,
                NameEn = s.NameEn,
                NameBn = s.NameBn,
                InTime = s.InTime,
                OutTime = s.OutTime,
                LateInTime = s.LateInTime,
                LunchTimeStart = s.LunchTimeStart,
                LunchHour = s.LunchHour,
                Weekends = s.Weekends,
                Status = s.Status
            };
        }

        // POST: api/shift
        [HttpPost]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer)]
        public async Task<ActionResult<ShiftDto>> CreateShift(CreateShiftDto dto)
        {
            var shift = new Shift
            {
                NameEn = dto.NameEn,
                NameBn = dto.NameBn,
                InTime = dto.InTime,
                OutTime = dto.OutTime,
                LateInTime = dto.LateInTime,
                LunchTimeStart = dto.LunchTimeStart,
                LunchHour = dto.LunchHour,
                Weekends = dto.Weekends,
                Status = dto.Status
            };

            _context.Shifts.Add(shift);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetShift), new { id = shift.Id }, new ShiftDto
            {
                Id = shift.Id,
                NameEn = shift.NameEn,
                NameBn = shift.NameBn,
                InTime = shift.InTime,
                OutTime = shift.OutTime,
                LateInTime = shift.LateInTime,
                LunchTimeStart = shift.LunchTimeStart,
                LunchHour = shift.LunchHour,
                Weekends = shift.Weekends,
                Status = shift.Status
            });
        }

        // PUT: api/shift/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer)]
        public async Task<IActionResult> UpdateShift(int id, CreateShiftDto dto)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift == null) return NotFound();

            shift.NameEn = dto.NameEn;
            shift.NameBn = dto.NameBn;
            shift.InTime = dto.InTime;
            shift.OutTime = dto.OutTime;
            shift.LateInTime = dto.LateInTime;
            shift.LunchTimeStart = dto.LunchTimeStart;
            shift.LunchHour = dto.LunchHour;
            shift.Weekends = dto.Weekends;
            shift.Status = dto.Status;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/shift/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer)]
        public async Task<IActionResult> DeleteShift(int id)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift == null) return NotFound();

            // Check if any employees are assigned to this shift
            var hasEmployees = await _context.Employees.AnyAsync(e => e.ShiftId == id);
            if (hasEmployees)
            {
                return BadRequest(new
                    { message = "Cannot delete shift because it is assigned to one or more employees." });
            }

            _context.Shifts.Remove(shift);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
