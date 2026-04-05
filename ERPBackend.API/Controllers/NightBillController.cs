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
    public class NightBillController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NightBillController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<NightBillResponseDto>> GetNightBills(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int? employeeId,
            [FromQuery] int? departmentId,
            [FromQuery] string? status,
            [FromQuery] string? searchTerm)
        {
            try
            {
                var query = _context.NightBills
                    .Include(i => i.Employee).ThenInclude(e => e!.Department)
                    .Include(i => i.Employee).ThenInclude(e => e!.Company)
                    .Include(i => i.Employee).ThenInclude(e => e!.Designation)
                    .Include(i => i.Shift)
                    .AsQueryable();

                if (fromDate.HasValue) query = query.Where(i => i.Date.Date >= fromDate.Value.Date);
                if (toDate.HasValue) query = query.Where(i => i.Date.Date <= toDate.Value.Date);
                if (employeeId.HasValue) query = query.Where(i => i.EmployeeId == employeeId.Value);
                if (departmentId.HasValue) query = query.Where(i => i.Employee!.DepartmentId == departmentId.Value);
                if (!string.IsNullOrWhiteSpace(status)) query = query.Where(i => i.Status == status);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                    query = query.Where(o => o.Employee!.EmployeeId.Contains(searchTerm) || o.Employee!.FullNameEn.Contains(searchTerm));

                var records = await query
                    .OrderByDescending(i => i.Date)
                    .Select(i => new NightBillDto
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

                var summary = new NightBillSummaryDto
                {
                    TotalAmount = records.Sum(r => r.Amount),
                    TotalEmployees = records.Select(r => r.EmployeeId).Distinct().Count(),
                    TotalRecords = records.Count
                };

                return Ok(new NightBillResponseDto { Summary = summary, Records = records });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching night bills", error = ex.Message });
            }
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessNightBills([FromBody] BillProcessRequestDto request)
        {
            try
            {
                var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
                int processedCount = 0;

                var employees = await _context.Employees
                    .Where(e => e.IsActive && (!request.DepartmentId.HasValue || e.DepartmentId == request.DepartmentId))
                    .Include(e => e.Shift)
                    .ToListAsync();

                var attendanceRecords = await _context.Attendances
                    .Where(a => a.Date >= request.FromDate.Date && a.Date <= request.ToDate.Date && (a.Status.StartsWith("Present") || a.Status == "Late"))
                    .ToListAsync();

                var existingBills = await _context.NightBills
                    .Where(i => i.Date >= request.FromDate.Date && i.Date <= request.ToDate.Date)
                    .ToListAsync();

                var formulas = new List<NightBill>();

                foreach (var att in attendanceRecords)
                {
                    var emp = employees.FirstOrDefault(e => e.Id == att.EmployeeCard);
                    if (emp == null || emp.Shift == null) continue;

                    // Check if it's a night shift (e.g., InTime after 14:00 or name contains Night)
                    bool isNightShift = emp.Shift.NameEn.Contains("Night", StringComparison.OrdinalIgnoreCase);
                    
                    if (!isNightShift && TimeSpan.TryParse(emp.Shift.InTime, out var inTime))
                    {
                        if (inTime.Hours >= 18 || inTime.Hours <= 4) isNightShift = true;
                    }

                    if (isNightShift)
                    {
                        var existing = existingBills.FirstOrDefault(i => i.EmployeeId == emp.Id && i.Date.Date == att.Date.Date);
                        if (existing == null)
                        {
                            formulas.Add(new NightBill
                            {
                                EmployeeId = emp.Id,
                                Date = att.Date,
                                Amount = 0, // Removd from Shift Management
                                ShiftId = emp.Shift.Id,
                                CompanyId = emp.CompanyId,
                                Status = "Approved",
                                CreatedAt = DateTime.UtcNow,
                                CreatedBy = userName
                            });
                            processedCount++;
                        }
                    }
                }

                if (formulas.Any())
                {
                    await _context.NightBills.AddRangeAsync(formulas);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = $"Successfully processed {processedCount} Night Bill records." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error processing night bills", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNightBill(int id)
        {
            try
            {
                var record = await _context.NightBills.FindAsync(id);
                if (record == null) return NotFound();

                _context.NightBills.Remove(record);
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
