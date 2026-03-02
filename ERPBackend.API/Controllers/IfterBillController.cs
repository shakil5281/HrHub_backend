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
    public class IfterBillController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public IfterBillController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/IfterBill
        [HttpGet]
        public async Task<ActionResult<IfterBillResponseDto>> GetIfterBills(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int? employeeId,
            [FromQuery] int? departmentId,
            [FromQuery] string? status,
            [FromQuery] string? searchTerm)
        {
            try
            {
                var query = _context.IfterBills
                    .Include(i => i.Employee)
                    .ThenInclude(e => e!.Department)
                    .Include(i => i.Employee)
                    .ThenInclude(e => e!.Company)
                    .Include(i => i.Employee)
                    .ThenInclude(e => e!.Designation)
                    .Include(i => i.Shift)
                    .AsQueryable();

                if (fromDate.HasValue)
                    query = query.Where(i => i.Date.Date >= fromDate.Value.Date);

                if (toDate.HasValue)
                    query = query.Where(i => i.Date.Date <= toDate.Value.Date);

                if (employeeId.HasValue)
                    query = query.Where(i => i.EmployeeId == employeeId.Value);

                if (departmentId.HasValue)
                    query = query.Where(i => i.Employee!.DepartmentId == departmentId.Value);

                if (!string.IsNullOrWhiteSpace(status))
                    query = query.Where(i => i.Status == status);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                    query = query.Where(o =>
                        o.Employee!.EmployeeId.Contains(searchTerm) ||
                        o.Employee!.FullNameEn.Contains(searchTerm));

                var records = await query
                    .OrderByDescending(i => i.Date)
                    .Select(i => new IfterBillDto
                    {
                        Id = i.Id,
                        EmployeeCard = i.EmployeeId,
                        EmployeeId = i.Employee!.EmployeeId,
                        EmployeeName = i.Employee!.FullNameEn,
                        Department = i.Employee!.Department!.NameEn,
                        Designation = i.Employee!.Designation!.NameEn,
                        Date = i.Date,
                        Amount = i.Amount,
                        Status = i.Status,
                        CreatedAt = i.CreatedAt,
                        ShiftName = i.Shift != null ? i.Shift.NameEn : "N/A",
                        CompanyName = i.Employee.Company != null ? i.Employee.Company.CompanyNameEn : ""
                    })
                    .ToListAsync();

                var summary = new IfterBillSummaryDto
                {
                    TotalAmount = records.Sum(r => r.Amount),
                    TotalEmployees = records.Select(r => r.EmployeeId).Distinct().Count(),
                    TotalRecords = records.Count
                };

                return Ok(new IfterBillResponseDto
                {
                    Summary = summary,
                    Records = records
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching ifter bills", error = ex.Message });
            }
        }

        // POST: api/IfterBill/process
        [HttpPost("process")]
        public async Task<IActionResult> ProcessIfterBills([FromBody] IfterBillProcessRequestDto request)
        {
            try
            {
                var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
                int processedCount = 0;

                // Load active employees and shifts
                var employees = await _context.Employees
                    .Where(e => e.IsActive && (!request.DepartmentId.HasValue || e.DepartmentId == request.DepartmentId))
                    .Include(e => e.Shift)
                    .ToListAsync();

                var attendanceRecords = await _context.Attendances
                    .Where(a => a.Date >= request.FromDate.Date && a.Date <= request.ToDate.Date)
                    .ToListAsync();

                var existingBills = await _context.IfterBills
                    .Where(i => i.Date >= request.FromDate.Date && i.Date <= request.ToDate.Date)
                    .ToListAsync();

                var formulas = new List<IfterBill>();

                for (var date = request.FromDate.Date; date <= request.ToDate.Date; date = date.AddDays(1))
                {
                    var dateStr = date.ToString("yyyy-MM-dd");

                    foreach (var emp in employees)
                    {
                        var shift = emp.Shift;
                        if (shift == null || !shift.HasSpecialBreak || string.IsNullOrEmpty(shift.SpecialBreakDates)) continue;

                        // Check if today is a special break date
                        if (!shift.SpecialBreakDates.Split(',').Any(d => d.Trim() == dateStr)) continue;

                        var attendance = attendanceRecords.FirstOrDefault(a => a.EmployeeCard == emp.Id && a.Date.Date == date.Date);
                        if (attendance == null || attendance.OutTime == null) continue;

                        if (TimeSpan.TryParse(shift.SpecialBreakEnd, out var sbEnd))
                        {
                            var sbEndDateTime = date.Date.Add(sbEnd);
                            
                            // Handle overnight shift if necessary (though Ifter is usually evening)
                            if (shift.ActualInTime != null && TimeSpan.TryParse(shift.ActualInTime, out var aIn) && sbEnd < aIn)
                            {
                                sbEndDateTime = date.Date.AddDays(1).Add(sbEnd);
                            }

                            if (attendance.OutTime >= sbEndDateTime)
                            {
                                // Check if bill already exists to avoid duplicates
                                var existing = existingBills.FirstOrDefault(i => i.EmployeeId == emp.Id && i.Date.Date == date.Date);
                                if (existing == null)
                                {
                                    formulas.Add(new IfterBill
                                    {
                                        EmployeeId = emp.Id,
                                        Date = date,
                                        Amount = shift.IfterBillAmount,
                                        ShiftId = shift.Id,
                                        CompanyId = emp.CompanyId,
                                        Status = "Approved",
                                        CreatedAt = DateTime.UtcNow,
                                        CreatedBy = userName
                                    });
                                    processedCount++;
                                }
                            }
                        }
                    }
                }

                if (formulas.Any())
                {
                    await _context.IfterBills.AddRangeAsync(formulas);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = $"Successfully processed {processedCount} Ifter Bill records." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error processing ifter bills", error = ex.Message });
            }
        }

        // DELETE: api/IfterBill/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIfterBill(int id)
        {
            try
            {
                var record = await _context.IfterBills.FindAsync(id);
                if (record == null) return NotFound();

                _context.IfterBills.Remove(record);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Record deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting record", error = ex.Message });
            }
        }
    }
}
