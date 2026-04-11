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
    public class HolidayController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HolidayController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Holiday>>> GetHolidays()
        {
            return await _context.Holidays
                .OrderByDescending(h => h.StartDate)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Holiday>> GetHoliday(int id)
        {
            var holiday = await _context.Holidays.FindAsync(id);

            if (holiday == null)
            {
                return NotFound();
            }

            return holiday;
        }

        [HttpPost]
        public async Task<ActionResult<Holiday>> CreateHoliday(Holiday holiday)
        {
            _context.Holidays.Add(holiday);
            await _context.SaveChangesAsync();

            await SyncHolidayAttendance(holiday);

            return CreatedAtAction(nameof(GetHoliday), new { id = holiday.Id }, holiday);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHoliday(int id, Holiday holiday)
        {
            if (id != holiday.Id)
            {
                return BadRequest();
            }

            // Get original to compare dates? For simplicity, we just re-sync.
            _context.Entry(holiday).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                await SyncHolidayAttendance(holiday);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HolidayExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHoliday(int id)
        {
            var holiday = await _context.Holidays.FindAsync(id);
            if (holiday == null)
            {
                return NotFound();
            }

            // Remove corresponding holiday attendance records or mark them back to Absent
            await RemoveHolidayAttendance(holiday);

            _context.Holidays.Remove(holiday);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task SyncHolidayAttendance(Holiday holiday)
        {
            var activeEmployees = await _context.Employees
                .Where(e => e.IsActive && (!holiday.CompanyId.HasValue || e.CompanyId == holiday.CompanyId))
                .ToListAsync();

            var startDate = holiday.StartDate.Date;
            var endDate = holiday.EndDate.Date;

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                // Fetch attendance records for this specific day
                var existingAttendances = await _context.Attendances
                    .Where(a => a.Date.Date == date.Date)
                    .ToListAsync();

                foreach (var emp in activeEmployees)
                {
                    var att = existingAttendances.FirstOrDefault(a => a.EmployeeCard == emp.Id);
                    if (att == null)
                    {
                        // Create a new Holiday record
                        _context.Attendances.Add(new Attendance
                        {
                            EmployeeCard = emp.Id,
                            EmployeeId = emp.EmployeeId,
                            CompanyId = emp.CompanyId,
                            Date = date,
                            Status = "Holiday",
                            Remarks = holiday.Name,
                            IsOffDay = true,
                            CreatedAt = DateTime.Now,
                            CreatedBy = User.Identity?.Name ?? "System"
                        });
                    }
                    else if (att.Status == "Absent" || att.Status == "Off Day")
                    {
                        // Update existing Absent/Off Day records to Holiday
                        att.Status = "Holiday";
                        att.Remarks = holiday.Name;
                        att.IsOffDay = true;
                        att.UpdatedAt = DateTime.Now;
                        att.UpdatedBy = User.Identity?.Name ?? "System";
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task RemoveHolidayAttendance(Holiday holiday)
        {
            var startDate = holiday.StartDate.Date;
            var endDate = holiday.EndDate.Date;

            var holidayAttendances = await _context.Attendances
                .Where(a => a.Date.Date >= startDate && a.Date.Date <= endDate && a.Status == "Holiday" && a.Remarks == holiday.Name)
                .ToListAsync();

            foreach (var att in holidayAttendances)
            {
                if (att.InTime == null && att.OutTime == null)
                {
                    // If no punches, just remove it
                    _context.Attendances.Remove(att);
                }
                else
                {
                    // If there were punches, change status back? 
                    // This is complex, but for now we set to Present or let it be re-processed
                    att.Status = "Present";
                    att.IsOffDay = false;
                }
            }
            // Note: SaveChanges is called in the calling method
        }

        private bool HolidayExists(int id)
        {
            return _context.Holidays.Any(e => e.Id == id);
        }
    }
}
